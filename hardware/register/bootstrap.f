( bootstrap remainder of interpreter )
( load into machine running interpreter image )

create : compile create compile ; ( magic! )

( assembler )

: x 1 ; ( shared by interpreter )
: d 2 ; ( dictionary pointer - shared by interpreter )
: zero 4 ; ( shared by interpreter )
: y 31 ; ( beyond registers in interpreter )
: z 32 ; 

: [ interact ; immediate
: ] compile ;

: cp, 6 , , , ; 
: popxy popx [ x y cp, ] popx ;
: pushxy pushx [ y x cp, ] pushx ;

: xor, 18 , , , , ; 
: swap popxy [ x y x xor, x y y xor, x y x xor, ] pushxy ;

: halt,   0 , ;
: ldc,    1 , , , ;
: ld,     2 , , , ;
: st,     3 , , , ;
: ldb,    4 , , , ;
: stb,    5 , , , ;
( cp,     defined above )
: in,     7 , , ;
: out,    8 , , ;
: inc,    9 , , , ;
: dec,   10 , , , ;
: add,   11 , , , , ;
: sub,   12 , , swap , , ;
: mul,   13 , , , , ;
: div,   14 , , swap , , ;
: mod,   15 , , swap , , ;
: and,   16 , , , , ;
: or,    17 , , , , ;
( xor    defined above )
: not,   19 , , , ;
: shl,   20 , , swap , , ;
: shr,   21 , , swap , , ;
: beq,   22 , , , , ;
: bne,   23 , , , , ;
: bgt,   24 , , swap , , ;
: bge,   25 , , swap , , ;
: blt,   26 , , swap , , ;
: ble,   27 , , swap , , ;
: jump,  28 , , ;
: call,  29 , , ;
: exec,  30 , , ;
: ret,   31 , ;
: dump,  32 , ;
: debug, 33 , ;

( instruction words )

: +       popxy [ x y x add,  ] pushx ;
: -       popxy [ x y x sub,  ] pushx ;
: *       popxy [ x y x mul,  ] pushx ;
: /       popxy [ x y x div,  ] pushx ;
: mod     popxy [ x y x mod,  ] pushx ;
: 2*      popxy [ x y x shl,  ] pushx ;
: 2/      popxy [ x y x shr,  ] pushx ;
: and     popxy [ x y x and,  ] pushx ;
: or      popxy [ x y x or,   ] pushx ;
: xor     popxy [ x y x xor,  ] pushx ;
: invert  popx  [   x x not,  ] pushx ;
: 1+      popx  [   x x inc,  ] pushx ;
: 1-      popx  [   x x dec,  ] pushx ;
: execute popx  [     x exec, ] ;
: halt [ halt, ] ;
: dump [ dump, ] ;
: debug [ debug, ] ;

( stack manipulation )

: drop popx ;
: dup popx pushx pushx ;
: over popxy pushx pushx [ y x cp, ] pushx swap ;
: nip swap drop ;
: tuck swap over ;
: -rot swap popxy pushx [ y z cp, ] swap [ z x cp, ] pushx ;
: rot -rot -rot ;

( vocabulary )

: true -1 ;
: false 0 ;

: key [ x in, ] pushx ;
: emit popx [ x out, ] ;
: cr 10 emit ;
: space 32 emit ;

: @ popx [ x x ld, ] pushx ;
: c@ popx [ x x ldb, ] pushx ;
: ! popxy [ x y st, ] ;
: c! popxy [ x y stb, ] ;

: here [ d x cp, ] pushx ;

: _dp+12 here 12 + ; ( * )
: constant create literal ret, ;  ( e.g. 123 constant foo -> foo3 . 0  LDC n 123  CALL &pushn  RET )
: variable create _dp+12 literal ret, 0 , ;  ( e.g. variable foo -> foo3 . 0  LDC n <addr>  CALL &pushn  RET  0 )

: allot popx [ x d d add, ] ;

: if [ ' popx literal ] call, here 2 + ( * ) zero x 0 beq, ; immediate
: else here 2 + ( * ) 0 jump, swap here swap ! ; immediate
: then here swap ! ; immediate

: =  popxy 0 [ x y _dp+12 bne, ] invert ;
: <> popxy 0 [ x y _dp+12 beq, ] invert ;
: >  popxy 0 [ x y _dp+12 ble, ] invert ;
: <  popxy 0 [ x y _dp+12 bge, ] invert ;
: >= popxy 0 [ x y _dp+12 blt, ] invert ;
: <= popxy 0 [ x y _dp+12 bgt, ] invert ;

: negate -1 * ;
: abs dup 0 < if negate then ;
: 2dup over over ;
: /mod 2dup / -rot mod ;

: _sign dup 0 < if -1 else 1 then negate 44 + emit ; ( happens 44 +/- 1 is ASCII '-'/'+' )
: _dig 10 /mod swap ;
: _digemit 48 + emit ;  ( 48 is ASCII '0' )
: . _sign abs _dig _dig _dig _dig _dig drop _digemit _digemit _digemit _digemit _digemit cr ;

: begin here ; immediate
: until [ ' popx literal ] call, zero x rot beq, ; immediate
: again [ ' popx literal ] call, jump, ; immediate

here
64 allot
variable r r !

: >r r @ ! r @ 2 + ( * ) r ! ;
: r> r @ 2 - ( * ) r ! r @ @ ;
: r@ r @ 2 - ( * ) @ ;

: _do swap >r >r ;
: do [ ' _do literal ] call, here ; immediate
: _loop0 r> 1+ dup >r r @ 4 ( * ) - @ >= popx ;
: _loop1 r> r> drop drop ;
: loop [ ' _loop0 literal ] call, zero x rot beq, [ ' _loop1 literal ] call, ; immediate

: i r @ 2 - ( * ) @ ;
: j r @ 6 - ( * ) @ ;

: [: here 14 + ( * ) ( past LIT . . CALL . JUMP . ) literal 0 jump, here 2 - ( * ) ( jump address field ); immediate
: :] ret, here swap ! ; immediate
: call popx [ x exec, ] ;
