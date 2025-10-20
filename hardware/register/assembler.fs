require memory.fs

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
: ldc,   (   v x -- )  1 2nybbles, c, ;
: ld+,   ( z y x -- )  2 4nybbles, ;
: st+,   ( z y x -- )  3 4nybbles, ;
: cp?,   ( z y x -- )  4 4nybbles, ;
: add,   ( z y x -- )  5 4nybbles, ;
: sub,   ( z y x -- )  6 4nybbles, ;
: mul,   ( z y x -- )  7 4nybbles, ;
: div,   ( z y x -- )  8 4nybbles, ;
: nand,  ( z y x -- )  9 4nybbles, ;
: shl,   ( z y x -- ) 10 4nybbles, ;
: shr,   ( z y x -- ) 11 4nybbles, ;
: in,    (   y x -- ) 12 2nybbles, ;
: out,   (   y x -- ) 13 2nybbles, ;
: read,  ( z y x -- ) 14 4nybbles, ;
: write, ( z y x -- ) 15 4nybbles, ;

0 constant pc
1 constant zero

: cp, ( y x -- ) zero cp?, ;
: ld, ( y x -- ) zero ld+, ;
: st, ( y x -- ) zero st+, ;

: jump, ( addr -- ) pc pc ld, , ;

: not, (   y x -- ) dup nand, ;
: and, 2 pick -rot nand, dup not, ;
: or,  ( z y x -- ) dup dup not, over dup not, nand, ;

: label ( -- addr ) here constant ;
: branch, ( -- dest ) 0 jump,  here 2 - ;
: patch, ( orig -- ) here swap s! ;

: block-file ( n -- c-addr u ) s" block" rot 0 <# #s #> s+ s" .bin" s+ ;
: write-boot-block ( -- ) 0 memory here write-block ;