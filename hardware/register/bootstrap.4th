( bootstrap remainder of interpreter )
( load into machine running outer interpreter image )

create : compile create compile ; ( magic! )

( assembler )

: x 1 ; ( shared by outer interpreter )
: d 2 ; ( dictionary pointer - shared by outer interpreter )
: zero 4 ; ( shared by outer interpreter )
: y 30 ; 
: z 31 ; 

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

: allot [ d x cp, ] pushx swap popx [ x d d add, ] ;

: here@ [ d x cp, ] pushx ;
: here! popx [ x d cp, ] ;

: >dfa 2 + ;
: ' find >dfa ;

: if [ ' popxy literal ] call, here@ 1+ zero x 0 beq, ; immediate
: unless [ ' popxy literal ] call, here@ 1+ zero x 0 bne, ; immediate
: else here@ 1+ 0 jump, swap here@ swap ! ; immediate
: then here@ swap ! ; immediate

: = popxy 0 [ x y here@ 6 + bne, ] not ;
: <> popxy 0 [ x y here@ 6 + beq, ] not ;
: > popxy 0 [ x y here@ 6 + ble, ] not ;
: < popxy 0 [ x y here@ 6 + bge, ] not ;
: >= popxy 0 [ x y here@ 6 + blt, ] not ;
: <= popxy 0 [ x y here@ 6 + bgt, ] not ;

: sign 0 < if -1 else 1 then ;
: /mod 2dup / -rot mod ;

: min 2dup > if swap then drop ;
: max 2dup < if swap then drop ;

: negate -1 * ;
: abs dup 0 < if negate then ;

: .sign dup sign negate 44 + emit ; ( happens 44 +/- 1 is ASCII '-'/'+' )
: .dig 10 /mod swap ;
: .digemit 48 + emit ;  ( 48 is ASCII '0' )
: . .sign .dig .dig .dig .dig .dig drop .digemit .digemit .digemit .digemit .digemit cr ;

: ?dup dup unless drop then ;

: factorial dup 1 > if dup 1- factorial * then ;

: begin here@ ; immediate
: until [ ' popxy literal ] call, zero x rot beq, ; immediate
