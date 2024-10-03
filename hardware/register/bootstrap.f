( bootstrap remainder of interpreter )
( load into machine running interpreter image )

create : compile create compile ; ( magic! )

( assembler )

: x 1 ; ( shared by interpreter )
: d 2 ; ( dictionary pointer - shared by interpreter )
: lnk 3 ; ( link pointer - shared by interpreter )
: zero 5 ; ( shared by interpreter )
: y 31 ; ( beyond registers in interpreter )
: z 32 ;

: [ interact ; immediate
: ] compile ;

: cp, 6 c, c, c, ; 
: popxy popx [ x y cp, ] popx ;
: pushxy pushx [ y x cp, ] pushx ;

: xor, 18 c, c, c, c, ; 
: swap popxy [ x y x xor, x y y xor, x y x xor, ] pushxy ;

: halt,   0 c, ;
: ldc,    1 c,  , c, ;
: ld,     2 c, c, c, ;
: st,     3 c, c, c, ;
: ldb,    4 c, c, c, ;
: stb,    5 c, c, c, ;
( cp,     defined above )
: in,     7 c, c, ;
: out,    8 c, c, ;
: inc,    9 c, c, c, ;
: dec,   10 c, c, c, ;
: add,   11 c, c, c, c, ;
: sub,   12 c, c, swap c, c, ;
: mul,   13 c, c, c, c, ;
: div,   14 c, c, swap c, c, ;
: mod,   15 c, c, swap c, c, ;
: and,   16 c, c, c, c, ;
: or,    17 c, c, c, c, ;
( xor    defined above )
: not,   19 c, c, c, ;
: shl,   20 c, c, swap c, c, ;
: shr,   21 c, c, swap c, c, ;
: beq,   22 c,  , c, c, ;
: bne,   23 c,  , c, c, ;
: bgt,   24 c,  , swap c, c, ;
: bge,   25 c,  , swap c, c, ;
: blt,   26 c,  , swap c, c, ;
: ble,   27 c,  , swap c, c, ;
: jump,  28 c,  , ;
: call,  29 c,  , ;
: exec,  30 c, c, ;
: ret,   31 c, ;
: dump,  32 c, c, c, ;
: debug, 33 c, ;

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

: dump [ x 5 ldc, lnk x st, x 1 ldc, d x st, ] popxy [ x y dump, ] ; ( store at magic address for image )

: constant create literal ret, ;  ( e.g. 123 constant foo -> foo3 . 0  LDC 123 n  CALL &pushn  RET )
: variable create here 8 + literal ret, 0 , ;  ( e.g. variable foo -> foo3 . 0  LDC <addr> n  CALL &pushn  RET  0 )

: allot popx [ x d d add, ] ;

: if [ ' popx literal ] call, here 1 + zero x 0 beq, ; immediate
: else here 1 + 0 jump, swap here swap ! ; immediate
: then here swap ! ; immediate

: _dp+ here 8 + ; ( * ) ( over BRANCH addr x y CALL invert )
: =  popxy 0 [ x y _dp+ bne, ] invert ;
: <> popxy 0 [ x y _dp+ beq, ] invert ;
: >  popxy 0 [ x y _dp+ ble, ] invert ;
: <  popxy 0 [ x y _dp+ bge, ] invert ;
: >= popxy 0 [ x y _dp+ blt, ] invert ;
: <= popxy 0 [ x y _dp+ bgt, ] invert ;

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

: [: here 10 + ( * ) ( past LIT . . CALL . JUMP . ) literal here 1 + 0 jump, ( jump address field ); immediate
: :] ret, here swap ! ; immediate
: call popx [ x exec, ] ;
