create memory $8000 allot
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
: ld+,   ( z y x -- )  2 4nybbles, ;    \ z=[y] y+=x   (load from memory and inc/dec pointer)
: st+,   ( z y x -- )  3 4nybbles, ;    \ [z]=y z+=x   (store to memory and inc/dec poniter)
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

: label here memory - constant ; \ current address within memory buffer

0 constant pc
1 constant zero

: cp, ( y x -- ) zero cp?, ; \ y=x   (unconditional copy)
: ld, ( y x -- ) zero ld+, ; \ y=[x] (load from memory)
: st, ( y x -- ) zero st+, ; \ [y]=x (store to memory)

: jump, ( addr -- ) pc pc ld, , ; \ unconditional jump to address (following cell)

: not, (   y x -- ) dup nand, ;                        \ y=~x  (bitwise/logical not)
: and, 2 pick -rot nand, dup not, ;
\ : and, ( z y x -- ) t -rot nand, t not, ;       \ z=y&x (bitwise/logical and)
\ : and2, ( z y x -- ) >r >r dup dup r> r> nand, not, ;       \ z=y&x (bitwise/logical and)
: or,  ( z y x -- ) dup dup not, over dup not, nand, ; \ z=y|x (bitwise/logical and)
\ : xor, ( z y x -- ) 2dup t -rot and, >r >r dup dup r> r> or, t and, ;

\ sample
\ : bnand and invert ; \ primitive
\ : bnot dup bnand ;
\ : band bnand bnot ;
\ : bor bnot swap bnot band bnot ;
\ : bxor 2dup bor -rot band bnot band ;
\ : bnor bor bnot ;
\ : bxnor bxor bnot ;

\ : inc, t  one, t swap add, ; ( uses t )
\ : dec, t -one, t swap add, ; ( uses t )

\ : negate, swap over not, dup inc, ; ( uses t via inc, )
\ 
\ : ahead, here 2 + 0 zero jmz, ; ( dummy jump, push address )
\ : continue, here swap ! ; ( patch jump )
\ : assemble 0 here 0 write halt ;

\ : jumpz, swap x literal, pc x rot cp?, ;

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

: write-boot-block ( -- ) 0 0 here memory - write-block ;