require ../shared/memory.fs

false warnings ! \ redefining gforth words

: here h @ memory - ;
: c, ( c -- ) h @ c! 1 h +! ;
: , ( cc -- ) dup c, 8 rshift c, ;
: s! ( val addr -- ) memory + over 8 rshift over 1+ c! c! ;
: s@ ( addr -- val ) memory + dup c@ swap 1+ c@ 8 lshift or ;

true warnings !

: 2nybbles, ( x i -- ) 4 lshift or c, ;
: 4nybbles, ( z y x i -- ) 2nybbles, 2nybbles, ;

: halt,  (     x -- )  0 2nybbles, ;
: add,   ( z y x -- )  1 4nybbles, ;
: sub,   ( z y x -- )  2 4nybbles, ;
: mul,   ( z y x -- )  3 4nybbles, ;
: div,   ( z y x -- )  4 4nybbles, ;
: nand,  ( z y x -- )  5 4nybbles, ;
: shl,   ( z y x -- )  6 4nybbles, ;
: shr,   ( z y x -- )  7 4nybbles, ;
: in,    (   y x -- )  8 2nybbles, ;
: out,   (   y x -- )  9 2nybbles, ;
: read,  ( z y x -- ) 10 4nybbles, ;
: write, ( z y x -- ) 11 4nybbles, ;
: ld16+, ( z y x -- ) 12 4nybbles, ;
: st16+, ( z y x -- ) 13 4nybbles, ;
: lit8,  (   v x -- ) 14 2nybbles, c, ;
: cp?,   ( z y x -- ) 15 4nybbles, ;

0 constant pc
1 constant zero

: cp, ( y x -- ) zero cp?, ;
: ld16, ( y x -- ) zero ld16+, ;
: st16, ( y x -- ) zero st16+, ;

: jump, ( addr -- ) pc pc ld16, , ;

: not, (   y x -- ) dup nand, ;
: and, 2 pick -rot nand, dup not, ;
: or,  ( z y x -- ) dup dup not, over dup not, nand, ;

: label ( -- addr ) here constant ;
: branch, ( -- dest ) 0 jump,  here 2 - ;
: patch, ( orig -- ) here swap s! ;

: write-boot-block ( -- ) 0 0 here write-block ; \ note: depends on redefined `here`
