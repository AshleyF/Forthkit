 0 constant pc
 1 constant zero
 2 constant x
 3 constant y
 4 constant z
 7 constant t
 8 constant [pc]
10 constant [x]
11 constant [y]
12 constant [z]
15 constand [t]

: 2dup over over ;

: cp, zero -rot ccp, ;
: cpi, zero -rot ccpi, ;

: lit, [pc] swap cpi, , ;
: halt, -2 t lit, t pc pc add, ;

: not, dup dup nand, ;
(
x y z
z x y z
: and, nand, not, ;
)

1 x lit,
-3 y lit,
x y x shift,
3 y lit,
x y x shift,
halt,

assemble
