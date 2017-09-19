( bootstrap remainder of interpreter )
( load into machine running outer interpreter image )

create : compile create compile ;

: n 19 ; immediate
: z 28 ; immediate
: cp, 3 , , , ; immediate
: popzn, popn n z cp, popn ;
: pushnz, pushn z n cp, pushn ;

: xor, 15 , , , , ; immediate
: swap popzn, n z n xor, n z z xor, n z n xor, pushnz, ;

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
: dbg,  30 , ;            immediate
: dmp,  31 , , , ;        immediate

: key n in, pushn ;
: emit popn n out, ;

: 1+ popn n n inc, pushn ;
: 1- popn n n dec, pushn ;

: +   popzn, n z n add, pushn ;
: -   popzn, n z n sub, pushn ;
: *   popzn, n z n mul, pushn ;
: /   popzn, n z n div, pushn ;
: mod popzn, n z n mod, pushn ;
: 2/  popzn, n z n shr, pushn ;
: 2*  popzn, n z n shl, pushn ;
: and popzn, n z n and, pushn ;
: or  popzn, n z n or,  pushn ;
: xor popzn, n z n xor, pushn ;
: not popn n n not, pushn ;
: halt halt, ;

: drop popn ;
: dup popn pushn pushn ;
