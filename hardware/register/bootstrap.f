( bootstrap remainder of kernel )
( load into machine running kernel image )

header : compile header compile ; ( magic! )

( assembler )

: x 2 ; ( shared by kernel )
: d 3 ; ( dictionary pointer - shared by kernel )
: lnk 4 ; ( link pointer - shared by kernel )
: zero 5 ; ( shared by kernel )
: y 18 ; ( beyond registers in kernel )
: z 19 ;

: [ interact ; immediate
: ] compile ;

: cp, 6 c, c, c, ; 
: popxy popx [ x y cp, ] popx ;
: popxyz popxy [ y z cp, x y cp, ] popx ;
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
: read,  32 c, c, c, c, ;
: write, 33 c, c, c, c, ;

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
: lf 10 emit ;
: cr 13 emit ;
: space 32 emit ;

: @ popx [ x x ld, ] pushx ;
: c@ popx [ x x ldb, ] pushx ;
: ! popxy [ x y st, ] ;
: c! popxy [ x y stb, ] ;

: here [ d x cp, ] pushx ;

: magic [ x 5 ldc, lnk x st, x 1 ldc, d x st, ] ; ( store at magic address for image )
: read popxyz [ x y z read, ] ;
: write popxyz [ x y z write, ] ;

: constant header literal ret, ;  ( e.g. 123 constant foo -> foo3 . 0  LDC 123 n  CALL &pushn  RET )
: variable header here 8 + literal ret, 0 , ;  ( e.g. variable foo -> foo3 . 0  LDC <addr> n  CALL &pushn  RET  0 )

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

: _.d 10 /mod swap ;
: _.z char 0 + emit ;
: _.e dup if swap _.z else swap dup 0 = if drop else _.z drop true then then ;
: num dup 0 < if char - emit then abs _.d _.d _.d _.d _.d drop false _.e _.e _.e _.e drop _.z ;
: . num cr lf ;

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

: [: here 10 + ( * ) ( past LIT . . CALL . JUMP . ) literal here 1 + 0 jump, ( jump address field ) ; immediate
: :] ret, here swap ! ; immediate
: call popx [ x exec, ] ;
