variable h
: here h @ ;
:  , here m! here 2 + h ! ;
: c, here b! here 1 + h ! ;

: 2nib, 4 << or c, ;
: 4nib, 2nib, 2nib, ;

: halt,    0 2nib, ;
: ldc,     1 2nib, c, ;
: ld+,     2 4nib, ;
: st+,     3 4nib, ;
: cp?,     4 4nib, ;
: add,     5 4nib, ;
: sub,     6 4nib, ;
: mul,     7 4nib, ;
: div,     8 4nib, ;
: nand,    9 4nib, ;
: shl,    10 4nib, ;
: shr,    11 4nib, ;
: in,     12 2nib, ;
: out,    13 2nib, ;
: read,   14 4nib, ;
: write,  15 4nib, ;

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