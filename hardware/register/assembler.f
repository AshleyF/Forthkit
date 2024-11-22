create memory $8000 allot
variable m memory m !

: m, m @ c! 1 m +! ;

: 2nybbles, 4 lshift or m, ;
: 4nybbles, 2nybbles, 2nybbles, ;

: halt,  (     x -- )  0 2nybbles, ;    \ halt(x)      (halt with exit code x)
: ldc,   (   v x -- )  1 2nybbles, m, ; \ x=v          (load constant signed v into x)
: ld+,   ( z y x -- )  2 4nybbles, ;    \ z=[y] y+=x   (load from memory and inc/dec pointer)
: st+,   ( z y x -- )  3 4nybbles, ;    \ [z]=y z+=x   (store to memory and inc/dec poniter)
: cp?,   ( z y x -- )  4 4nybbles, ;    \ z=y if x     (conditional copy)
: add,   ( z y x -- )  5 4nybbles, ;    \ z=y+x        (addition)
: sub,   ( z y x -- )  6 4nybbles, ;    \ z=y-x        (subtraction)
: mul,   ( z y x -- )  7 4nybbles, ;    \ z=y*x        (multiplication)
: div,   ( z y x -- )  8 4nybbles, ;    \ z=y/x        (divition)
: nand,  ( z y x -- )  9 4nybbles, ;    \ z=y nand x   (not-and)
: shl,   ( z y x -- ) 10 4nybbles, ;    \ z=y<<x       (bitwise shift-left)
: shr,   ( z y x -- ) 11 4nybbles, ;    \ z=y>>x       (bitwise shift-right)
: in,    (   y x -- ) 12 2nybbles, ;    \ x=getc()     (read from console)
: out,   (   y x -- ) 13 2nybbles, ;    \ putc(x)      (write to console)
: read,  ( z y x -- ) 14 4nybbles, ;    \ read(z,y,x)  (file z of size y -> address x)
: write, ( z y x -- ) 15 4nybbles, ;    \ write(z,y,x) (file z of size y <- address x)

: label m @ constant ;

0 constant pc
1 constant zero
\ 2 constant two
\ 3 constant t

\ 2 two ldc,

: cp, zero cp?, ;
: ld, zero ld+, ;
: st, zero st+, ;

\ : lit, pc two ld+, , ;
: jump, pc pc ld, , ;
\ : jmz, swap t lit, pc t rot cp?, ; ( uses t )

: not, dup nand, ;
: and, 2 pick -rot nand, dup not, ;

: or, dup dup not, over dup not, nand, ;
( TODO xor nor xnor )

( TODO: needed? )
: zero, 0 swap ldc, ; ( TODO: with nand? )
: one, 1 swap ldc, ;
: -one, -1 swap ldc, ;

\ : inc, t  one, t swap add, ; ( uses t )
\ : dec, t -one, t swap add, ; ( uses t )

\ : negate, swap over not, dup inc, ; ( uses t via inc, )
\ 
\ : ahead, here 2 + 0 zero jmz, ; ( dummy jump, push address )
\ : continue, here swap ! ; ( patch jump )
\ : assemble 0 here 0 write halt ;