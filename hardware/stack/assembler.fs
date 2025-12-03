require ../shared/memory.fs

false warnings ! \ redefining gforth words

\ TODO: same as in register assembler
\ TODO: all needed? (only using: here , s!)

: here h @ memory - ;
: c, ( c -- ) h @ c! 1 h +! ;
: , ( cc -- ) dup c, 8 rshift c, ;
: s! ( val addr -- ) memory + over 8 rshift over 1+ c! c! ;
: s@ ( addr -- val ) memory + dup c@ swap 1+ c@ 8 lshift or ;

true warnings !

variable shiftbits  11 shiftbits !
variable h'
: here' h' @ memory - ;
: slot, ( i -- )
  shiftbits @ 11 = if
    h @ h' !
    2 h +!
  then
  shiftbits @ lshift \ shift instruction to slot
  0x1f shiftbits @ lshift invert here' s@ and \ fetch and mask off slot
  or 0x1 or here' s! \ place instruction (and ensure low bit set)
  -5 shiftbits +!
  shiftbits @ 0<= if 11 shiftbits ! then
;

: halt,   0 slot,    ; \ halt execution
: add,    1 slot,    ; \ addition
: sub,    2 slot,    ; \ subtraction
: mul,    3 slot,    ; \ multiplication
: div,    4 slot,    ; \ division
: not,    5 slot,    ; \ bitwise not
: and,    6 slot,    ; \ bitwise and
: or,     7 slot,    ; \ bitwise or
: xor,    8 slot,    ; \ bitwise xor
: shl,    9 slot,    ; \ shift left
: shr,   10 slot,    ; \ shift right
: in,    11 slot,    ; \ input character
: out,   12 slot,    ; \ output character
: read,  13 slot,    ; \ read block
: write, 14 slot,    ; \ write block
: ld16+, 15 slot,    ; \ fetch cell at address, and increment over
: ld8+,  16 slot,    ; \ fetch byte at address, and increment over
: st16+, 17 slot,    ; \ store cell at address, and increment over
: st8+,  18 slot,    ; \ store byte at address, and increment over
: lit16, 19 slot,  , ; \ fetch literal next cell
: lit8,  20 slot, c, ; \ fetch literal next byte (signed)
: if,    21 slot,  , ; \ jump to address in next cell if T >= 0
: next,  22 slot,  , ; \ if R <= 0, drop R and continue, otherwise R-- and loop to address in next cell
: drop,  23 slot,    ; \ drop top of stack
: dup,   24 slot,    ; \ duplicate top of stack
: over,  25 slot,    ; \ yx -> yxy
: swap,  26 slot,    ; \ yx -> xy
: push,  27 slot,    ; \ push top of data stack to return stack
: pop,   28 slot,    ; \ pop top of return stack to data stack
: peek,  29 slot,    ; \ copy top of return stack to data stack
: ret,   30 slot,    ; \ return from call
: nop,   31 slot,    ; \ no-op

: finish, begin shiftbits @ 11 <> while nop, repeat ; \ finish slot, padding with no-ops
: align, here 2 mod 0<> if 1 h +! then ; \ align here on even address boundary
: for, push, finish, here ; \ start for/next loop

: call, ( addr -- ) finish, 1 invert and , ; \ TODO: error if low bit set
: jump, ( addr -- ) call, ret, finish, ;

: label ( -- addr ) here constant ;
: branch, ( -- dest ) 0 jump,  here 4 - ;
: patch, ( orig -- ) finish, align, here swap s! ;

: write-boot-block ( -- ) 0 0 here write-block ; \ note: depends on redefined `here`
