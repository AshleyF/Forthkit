variable h
: here h @ ;
:  , here m! here 2 + h ! ;
: c, here b! here 1 + h ! ;

240 constant HIGH_MASK
15 constant LOW_MASK

: i1, or c, ;
: o2, 4 << or c, ;
: i3,  i1,      o2, ;
: i3s, i1, swap o2, ;

: ccp,        i3,  ;
: ccpi,   128 i3,  ;
: add,     16 i3,  ;
: addi,   144 i3,  ;
: mul,     32 i3,  ;
: muli,   160 i3,  ;
: div,     48 i3s, ;
: divi,   176 i3s, ;
: nand,    64 i3,  ;
: nandi,  192 i3,  ;
: shift,   80 i3s, ;
: shifti, 208 i3s, ;

: label here constant ;
: ahead, here 1 + 0 jump, ; ( dummy jump, push address )
: continue, here swap m! ; ( patch jump )

: assemble 0 here 0 write halt ;
