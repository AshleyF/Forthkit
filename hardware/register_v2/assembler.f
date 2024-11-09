variable h
: here h @ ;
:  , here m! here 2 + h ! ;
: c, here b! here 1 + h ! ;

: 2nyb, 4 << or c, ;
: 4nyb, 2nyb, 2nyb, ;

: halt,    0 2nyb, ;
: ldc,     1 2nyb, c, ;
: ld+,     2 4nyb, ;
: st+,     3 4nyb, ;
: cp?,     4 4nyb, ;
: add,     5 4nyb, ;
: sub,     6 4nyb, ;
: mul,     7 4nyb, ;
: div,     8 4nyb, ;
: nand,    9 4nyb, ;
: shl,    10 4nyb, ;
: shr,    11 4nyb, ;
: in,     12 2nyb, ;
: out,    13 2nyb, ;
: read,   14 4nyb, ;
: write,  15 4nyb, ;

: label here constant ;
: assemble 0 here 0 write halt ;

 0 constant pc
 1 constant zero
 2 constant two
 3 constant t

2 two ldc,

: cp, zero cp?, ;
: ld, zero ld+, ;
: st, zero st+, ;

: lit, pc two ld+, , ;
: jump, pc pc ld, , ;
: jmz, swap t lit, pc t rot cp?, ; ( uses t )

: not, dup nand, ;
: and, 2 pick -rot nand, dup not, ;
: or, dup dup not, over dup not, nand, ;
( TODO xor nor xnor )

( TODO: needed? )
: zero, 0 swap ldc, ; ( TODO: with nand? )
: one, 1 swap ldc, ;
: -one, -1 swap ldc, ;

: inc, t  one, t swap add, ; ( uses t )
: dec, t -one, t swap add, ; ( uses t )

: negate, swap over not, dup inc, ; ( uses t via inc, )

: ahead, here 2 + 0 zero jmz, ; ( dummy jump, push address )
: continue, here swap m! ; ( patch jump )
