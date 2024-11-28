$8000 constant memory-size
create memory memory-size allot  memory memory-size 0 fill
variable h  memory h !

false warnings ! \ intentionally redefining (here c, ,)

: here h @ ;
: c, ( c -- ) here c! 1 h +! ;
: , ( cc -- ) dup c, 8 rshift c, ; \ 16-bit little endian

true warnings !

: 2nybbles, ( x i -- ) 4 lshift or c, ;
: 4nybbles, ( z y x i -- ) 2nybbles, 2nybbles, ;

: halt,  (     x -- )  0 2nybbles, ;    \ halt(x)      (halt with exit code x)
: ldc,   (   v x -- )  1 2nybbles, c, ; \ x=v          (load constant signed v into x)
: ld+,   ( z y x -- )  2 4nybbles, ;    \ z<-[y] y+=x  (load from memory and inc/dec pointer)
: st+,   ( z y x -- )  3 4nybbles, ;    \ z->[y] y+=x  (store to memory and inc/dec poniter)
: cp?,   ( z y x -- )  4 4nybbles, ;    \ z=y if x     (conditional copy)
: add,   ( z y x -- )  5 4nybbles, ;    \ z=y+x        (addition)
: sub,   ( z y x -- )  6 4nybbles, ;    \ z=y-x        (subtraction)
: mul,   ( z y x -- )  7 4nybbles, ;    \ z=y*x        (multiplication)
: div,   ( z y x -- )  8 4nybbles, ;    \ z=y/x        (division)
: nand,  ( z y x -- )  9 4nybbles, ;    \ z=y nand x   (not-and)
: shl,   ( z y x -- ) 10 4nybbles, ;    \ z=y<<x       (bitwise shift-left)
: shr,   ( z y x -- ) 11 4nybbles, ;    \ z=y>>x       (bitwise shift-right)
: in,    (   y x -- ) 12 2nybbles, ;    \ x=getc()     (read from console)
: out,   (   y x -- ) 13 2nybbles, ;    \ putc(x)      (write to console)
: read,  ( z y x -- ) 14 4nybbles, ;    \ read(z,y,x)  (file z of size y -> address x)
: write, ( z y x -- ) 15 4nybbles, ;    \ write(z,y,x) (file z of size y <- address x)

( --- secondary instructions ------------------------------------------------- )

0 constant pc
1 constant zero

: cp, ( y x -- ) zero cp?, ; \ y=x    (unconditional copy)
: ld, ( y x -- ) zero ld+, ; \ y<-[x] (load from memory)
: st, ( y x -- ) zero st+, ; \ y->[x] (store to memory)

: jump, ( addr -- ) pc pc ld, , ; \ unconditional jump to address (following cell)

: not, (   y x -- ) dup nand, ;                        \ y=~x  (bitwise/logical not)
: and, 2 pick -rot nand, dup not, ;
: or,  ( z y x -- ) dup dup not, over dup not, nand, ; \ z=y|x (bitwise/logical and)

( --- assembler tools -------------------------------------------------------- )

: label ( -- ) here memory - constant ; \ current address within memory buffer
: skip, ( -- ) here 2 + 0 jump, ; \ dummy jump, push address
: then, ( -- ) here memory - swap ! ; \ patch jump to continue here

( --- read/write blocks ------------------------------------------------------ )

: read-block ( block memaddr len )
  rot drop \ TODO: support numbered blocks
  s" block0.bin" 2dup file-status 0<> if ." Block file not found " else drop then
  r/o open-file throw
  rot memory + -rot dup >r
  read-file throw drop r>
  close-file throw ;

: write-block ( block memaddr len )
  rot drop \ TODO: support numbered blocks
  s" block0.bin" w/o create-file throw
  rot memory + -rot dup >r
  write-file throw r>
  close-file throw ;

: read-boot-block  ( -- ) 0 0 memory-size read-block ;
: write-boot-block ( -- ) 0 0 here memory - write-block ;