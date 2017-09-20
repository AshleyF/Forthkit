( bootstrap remainder of interpreter )
( load into machine running outer interpreter image )

create : compile create compile ; ( magic! )

( assembler )

: x 0 ; immediate
: y 1 ; immediate

: cp, 3 , , , ; immediate
: popyx, popx x y cp, popx ;
: pushxy, pushx y x cp, pushx ;

: xor, 15 , , , , ; immediate
: swap popyx, x y x xor, x y y xor, x y x xor, pushxy, ;

: ldc,   0 , , , ;        immediate
: ld,    1 , , , ;        immediate
: st,    2 , , , ;        immediate
: in,    4 , , ;          immediate
: out,   5 , , ;          immediate
: inc,   6 , , , ;        immediate
: dec,   7 , , , ;        immediate
: add,   8 , , , , ;      immediate
: sub,   9 , , swap , , ; immediate
: mul,  10 , , , , ;      immediate
: div,  11 , , swap , , ; immediate
: mod,  12 , , swap , , ; immediate
: and,  13 , , , , ;      immediate
: or,   14 , , , , ;      immediate
: not,  16 , , , ;        immediate
: shr,  17 , , swap , , ; immediate
: shl,  18 , , swap , , ; immediate
: beq,  19 , , , , ;      immediate
: bne,  20 , , , , ;      immediate
: bgt,  21 , , swap , , ; immediate
: bge,  22 , , swap , , ; immediate
: blt,  23 , , swap , , ; immediate
: ble,  24 , , swap , , ; immediate
: exec, 25 , , ;          immediate
: jump, 26 , , ;          immediate
: call, 27 , , ;          immediate
: ret,  28 , ;            immediate
: halt, 29 , ;            immediate
: dump, 30 , ;            immediate

( instruction words )

: +    popyx, x y x add, pushx ;
: -    popyx, x y x sub, pushx ;
: *    popyx, x y x mul, pushx ;
: /    popyx, x y x div, pushx ;
: mod  popyx, x y x mod, pushx ;
: 2/   popyx, x y x shr, pushx ;
: 2*   popyx, x y x shl, pushx ;
: and  popyx, x y x and, pushx ;
: or   popyx, x y x or,  pushx ;
: xor  popyx, x y x xor, pushx ;
: not  popx x x not, pushx ;
: 1+   popx x x inc, pushx ;
: 1-   popx x x dec, pushx ;
: exec popx x exec, ;
: dump dump, ;
: exit halt, ;

( stack manipulation )

: drop popx ;
: dup popx pushx pushx ;

( vocabulary )

: true 65535 ; ( TODO: support signed literals )
: false 0 ;

: key x in, pushx ;
: emit popx x out, ;
: cr 10 emit ;
: space 32 emit ;
