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
 2 constant one
 3 constant two
 4 constant a
 5 constant b

: cp, zero cp?, ;
: ld, zero ld+, ;
: st, zero st+, ;

: lit, pc two ld+, , ;
: jmp, pc pc ld, , ;
: jmz, swap a lit, pc a rot cp?, ;

: not, over swap nand, ;
: and, -rot 2 pick nand, dup not, ;
: or, rot a not, swap over not, a over nand, ; ( uses a )
( TODO xor nor xnor )
( TODO *i variants )

( TODO: needed? )
: zero, 0 swap lit, ; ( TODO: with nand? )
: one, 1 swap lit, ;
: -one, -1 swap lit, ;

: inc, a  one, a swap add, ; ( uses a )
: dec, a -one, a swap add, ; ( uses a )

: negate, swap over not, dup inc, ; ( uses a via inc, )

: init, 1 one ldc, 2 two ldc, ;
: ahead, here 2 + 0 zero jmz, ; ( dummy jump, push address )
: continue, here swap m! ; ( patch jump )
