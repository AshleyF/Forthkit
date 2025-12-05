require ../shared/memory.fs

false warnings ! \ redefining gforth words

: here h @ memory - ;
: c, ( c -- ) h @ c! 1 h +! ;
: , ( cc -- ) dup c, 8 rshift c, ;
: s! ( val addr -- ) memory + over 8 rshift over 1+ c! c! ;
: s@ ( addr -- val ) memory + dup c@ swap 1+ c@ 8 lshift or ;

true warnings !

variable shiftbits
: initslot 11 shiftbits ! ;
initslot

variable h'
: slot, ( i -- )
  shiftbits @ 11 = if \ first slot?
    $ffff h @ dup h' ! ! 2 h +! \ h'=h, initialize no-ops, h+=2
  then
  shiftbits @ lshift \ shift instruction to slot
  0x1f shiftbits @ lshift invert h' @ @ and \ fetch and mask off slot
  or h' @ ! \ place instruction
  -5 shiftbits +!
  shiftbits @ 0<= if 11 shiftbits ! then ;

: halt,   0 slot,    ; \ halt execution
: add,    1 slot,    ; \ addition
: sub,    2 slot,    ; \ subtraction
: mul,    3 slot,    ; \ multiplication
: div,    4 slot,    ; \ division
: mod,    5 slot,    ; \ modulus
: not,    6 slot,    ; \ bitwise not
: and,    7 slot,    ; \ bitwise and
: or,     8 slot,    ; \ bitwise or
: xor,    9 slot,    ; \ bitwise xor
: shl,   10 slot,    ; \ shift left
: shr,   11 slot,    ; \ shift right
: in,    12 slot,    ; \ input character
: out,   13 slot,    ; \ output character
: read,  14 slot,    ; \ read block
: write, 15 slot,    ; \ write block
: ld16+, 16 slot,    ; \ fetch cell at address, and increment over
: ld8+,  17 slot,    ; \ fetch byte at address, and increment over
: st16+, 18 slot,    ; \ store cell at address, and increment over
: st8+,  19 slot,    ; \ store byte at address, and increment over
: lit16, 20 slot,  , ; \ fetch literal next cell
: 0jump, 21 slot,  , ; \ jump to address in next cell if T >= 0
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

: align, initslot here 2 mod 0<> if 1 h +! then ; \ align here on even address boundary
: for, push, align, here ; \ start for/next loop

: call, ( addr -- ) initslot dup 2 mod 0= if , else ." Expected even-aligned address" throw then ; \ TODO: proper error?
: jump, ( addr -- ) call, ret, ;

: >r, push, ; \ ( x -- ) ( R: x -- ) move x to return stack [synonym]
: r>, pop, ; \ ( -- x ) ( R: x -- ) move x from return stack [synonym]
: r@, peek, ; \ ( -- x ) ( R: x -- x ) copy x from return stack [synonym]
: emit, out, ; \ emit ( char -- ) write to console [synonym]
: key, in, ; \ key ( -- char ) read from console [synonym]
: literal, lit16, ; \ synonym

: 2>r, push, push, ; \ ( y x -- ) ( R: -- y x ) move y x pair to return stack
: 2r>, pop, pop, ; \ ( -- y x ) ( R: y x -- ) move x from return stack
: nip, swap, drop, ; \ ( y x -- x ) drop second stack value
: tuck, swap, over, ; \ ( y x -- x y x ) copy top stack value under second value
: 2dup, over, over, ; \ ( y x -- y x y x ) duplicate top two stack values
: 2drop, drop, drop, ; \ ( y x -- ) remove top two stack values

: label ( -- addr ) here constant ;
: branch, ( -- dest ) 0 jump,  here 4 - ;
: patch, ( orig -- ) align, here swap s! ;

: write-boot-block ( -- ) 0 0 here write-block ; \ note: depends on redefined `here`
