( bootstrap remainder of interpreter )
( load into machine running outer interpreter image )

create : compile create compile ; ( magic! )

( assembler )

: x 1 ; ( shared by outer interpreter )
: d 2 ; ( dictionary pointer - shared by outer interpreter )
: zero 4 ; ( shared by outer interpreter )
: y 31 ; 

: [ interact ; immediate
: ] compile ;

: cp, 3 , , , ; 
: popxy popx [ x y cp, ] popx ;
: pushxy pushx [ y x cp, ] pushx ;

: xor, 15 , , , , ; 
: swap popxy [ x y x xor, x y y xor, x y x xor, ] pushxy ;

: ldc,   0 , , , ;
: ld,    1 , , , ;
: st,    2 , , , ;
: in,    4 , , ;
: out,   5 , , ;
: inc,   6 , , , ;
: dec,   7 , , , ;
: add,   8 , , , , ;
: sub,   9 , , swap , , ;
: mul,  10 , , , , ;
: div,  11 , , swap , , ;
: mod,  12 , , swap , , ;
: and,  13 , , , , ;
: or,   14 , , , , ;
: not,  16 , , , ;
: shr,  17 , , swap , , ;
: shl,  18 , , swap , , ;
: beq,  19 , , , , ;
: bne,  20 , , , , ;
: bgt,  21 , , swap , , ;
: bge,  22 , , swap , , ;
: blt,  23 , , swap , , ;
: ble,  24 , , swap , , ;
: exec, 25 , , ;
: jump, 26 , , ;
: call, 27 , , ;
: ret,  28 , ;
: halt, 29 , ;
: dump, 30 , ;

( instruction words )

: +    popxy [ x y x add, ] pushx ;
: -    popxy [ x y x sub, ] pushx ;
: *    popxy [ x y x mul, ] pushx ;
: /    popxy [ x y x div, ] pushx ;
: mod  popxy [ x y x mod, ] pushx ;
: 2/   popxy [ x y x shr, ] pushx ;
: 2*   popxy [ x y x shl, ] pushx ;
: and  popxy [ x y x and, ] pushx ;
: or   popxy [ x y x or,  ] pushx ;
: xor  popxy [ x y x xor, ] pushx ;
: not  popx [ x x not, ] pushx ;
: 1+   popx [ x x inc, ] pushx ;
: 1-   popx [ x x dec, ] pushx ;
: exec popx [ x exec, ] ;
: dump [ dump, ] ;
: exit [ halt, ] ;

( stack manipulation )

: drop popx ;
: dup popx pushx pushx ;

( vocabulary )

: true 65535 ; ( TODO: support signed literals )
: false 0 ;

: key [ x in, ] pushx ;
: emit popx [ x out, ] ;
: cr 10 emit ;
: space 32 emit ;

: @ popx [ x x ld, ] pushx ;
: ! popxy [ x y st, ] ;

: allot [ d x cp, ] pushx swap popx [ x d d add, ] ;

: here@ [ d x cp, ] pushx ;
: here! popx [ x d cp, ] ;

: >dfa 2 + ;
: ' find >dfa ;

: if [ ' popx literal ] call, here@ 1+ zero x 0 beq, ; immediate
: else here@ 1+ 0 jump, swap here@ swap ! ; immediate
: then here@ swap ! ; immediate

: = popxy 0 [ x y here@ 6 + bne, ] not ;
: <> popxy 0 [ x y here@ 6 + beq, ] not ;
: > popxy 0 [ x y here@ 6 + ble, ] not ;
: < popxy 0 [ x y here@ 6 + bge, ] not ;
: >= popxy 0 [ x y here@ 6 + blt, ] not ;
: <= popxy 0 [ x y here@ 6 + bgt, ] not ;
