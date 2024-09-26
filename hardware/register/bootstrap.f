( bootstrap remainder of interpreter )
( load into machine running interpreter image )

create : compile create compile ; ( magic! )

( assembler )

: x 1 ; ( shared by interpreter )
: d 2 ; ( dictionary pointer - shared by interpreter )
: zero 4 ; ( shared by interpreter )
: y 30 ; 
: z 31 ; 

: [ interact ; immediate
: ] compile ;

: cp, 3 , , , ; 
: popxy popx [ x y cp, ] popx ;
: pushxy pushx [ y x cp, ] pushx ;

: xor, 15 , , , , ; 
: swap popxy [ x y x xor, x y y xor, x y x xor, ] pushxy ;

: ldc,    0 , , , ;
: ld,     1 , , , ;
: st,     2 , , , ;
( cp,     defined above )
: in,     4 , , ;
: out,    5 , , ;
: inc,    6 , , , ;
: dec,    7 , , , ;
: add,    8 , , , , ;
: sub,    9 , , swap , , ;
: mul,   10 , , , , ;
: div,   11 , , swap , , ;
: mod,   12 , , swap , , ;
: and,   13 , , , , ;
: or,    14 , , , , ;
( xor    defined above )
: not,   16 , , , ;
: shl,   17 , , swap , , ;
: shr,   18 , , swap , , ;
: beq,   19 , , , , ;
: bne,   20 , , , , ;
: bgt,   21 , , swap , , ;
: bge,   22 , , swap , , ;
: blt,   23 , , swap , , ;
: ble,   24 , , swap , , ;
: jump,  25 , , ;
: call,  26 , , ;
: exec,  27 , , ;
: ret,   28 , ;
: halt,  29 , ;
: dump,  30 , ;
: debug, 31 , , , ;

( instruction words )

: +    popxy [ x y x add, ] pushx ;
: -    popxy [ x y x sub, ] pushx ;
: *    popxy [ x y x mul, ] pushx ;
: /    popxy [ x y x div, ] pushx ;
: mod  popxy [ x y x mod, ] pushx ;
: 2*   popxy [ x y x shl, ] pushx ;
: 2/   popxy [ x y x shr, ] pushx ;
: and  popxy [ x y x and, ] pushx ;
: or   popxy [ x y x or,  ] pushx ;
: xor  popxy [ x y x xor, ] pushx ;
: not  popx [ x x not, ] pushx ;
: 1+   popx [ x x inc, ] pushx ;
: 1-   popx [ x x dec, ] pushx ;
: exec popx [ x exec, ] ;
: exit [ halt, ] ;
: dump [ dump, ] ;
: debug [ debug, ] ;

( stack manipulation )

: drop ( a- ) popx ;
: 2drop ( ab- ) drop drop ;
: dup ( a-aa ) popx pushx pushx ;
: over ( ab-aba ) popxy pushx pushx [ y x cp, ] pushx swap ;
: 2dup ( ab-abab ) over over ;
: nip ( ab-b ) swap drop ;
: tuck ( ab-bab ) swap over ;
: -rot ( abc-cab ) swap popxy pushx [ y z cp, ] swap [ z x cp, ] pushx ;
: rot ( abc-bca ) -rot -rot ;

( vocabulary )

: true -1 ;
: false 0 ;

: key [ x in, ] pushx ;
: emit popx [ x out, ] ;
: cr 10 emit ;
: space 32 emit ;

: @ popx [ x x ld, ] pushx ;
: ! popxy [ x y st, ] ;

: here [ d x cp, ] pushx ;

: dp+6 here 6 + ;
: const create literal ret, ;  ( e.g. 123 const foo -> foo3 . 0  LDC n 123  CALL &pushn  RET )
: var create dp+6 literal ret, 0 , ;  ( e.g. var foo -> foo3 . 0  LDC n <addr>  CALL &pushn  RET  0 )

: allot popx [ x d d add, ] ;
: buffer create dp+6 literal ret, allot ;

: if [ ' popxy literal ] call, here 1+ zero x 0 beq, ; immediate
: unless [ ' popxy literal ] call, here 1+ zero x 0 bne, ; immediate
: else here 1+ 0 jump, swap here swap ! ; immediate
: then here swap ! ; immediate

: =  popxy 0 [ x y dp+6 bne, ] not ;
: <> popxy 0 [ x y dp+6 beq, ] not ;
: >  popxy 0 [ x y dp+6 ble, ] not ;
: <  popxy 0 [ x y dp+6 bge, ] not ;
: >= popxy 0 [ x y dp+6 blt, ] not ;
: <= popxy 0 [ x y dp+6 bgt, ] not ;

: sign 0 < if -1 else 1 then ;
: /mod 2dup / -rot mod ;

: min 2dup > if swap then drop ;
: max 2dup < if swap then drop ;

: negate -1 * ;
: abs dup 0 < if negate then ;

: .sign dup sign negate 44 + emit ; ( happens 44 +/- 1 is ASCII '-'/'+' )
: .dig 10 /mod swap ;
: .digemit 48 + emit ;  ( 48 is ASCII '0' )
: . .sign abs .dig .dig .dig .dig .dig drop .digemit .digemit .digemit .digemit .digemit cr ;

: ?dup dup unless drop then ;

: begin here ; immediate
: until [ ' popxy literal ] call, zero x rot beq, ; immediate
: again [ ' popxy literal ] call, jump, ; immediate

here
32 allot
var r r !

: >r r @ ! r @ 1+ r ! ;
: r> r @ 1- r ! r @ @ ;
: r@ r @ 1- @ ;

: _do swap >r >r ;
: do [ ' _do 2 - literal ] call, here ; immediate
: _loop0 r> 1+ dup >r r @ 2 - @ >= popx ;
: _loop1 r> r> drop drop ;
: loop [ ' _loop0 2 - literal ] call, zero x rot beq, [ ' _loop1 2 - literal ] call, ; immediate

: i r @ 1- @ ;
: j r @ 3 - @ ;

: [: here 7 + ( past LIT . . CALL . JUMP . ) literal 0 jump, here 1- ( jump address field ); immediate
: :] ret, here swap ! ; immediate
: call popx [ x exec, ] ;

: factorial dup 1 > if dup 1- factorial * then ;
