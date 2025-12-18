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

: verify-sbyte dup -128 127 within invert if ." Signed byte out of range: " . throw then ; \ TODO: does within handle negatives?
: verify-ubyte dup    0 255 within invert if ." Unsigned byte out of range: " . throw then ; \ TODO: does within handle negatives?

: halt,   0 slot,                             ; \ halt execution
: add,    1 slot,                             ; \ addition
: sub,    2 slot,                             ; \ subtraction
: mul,    3 slot,                             ; \ multiplication
: div,    4 slot,                             ; \ division
: not,    5 slot,                             ; \ bitwise not
: and,    6 slot,                             ; \ bitwise and
: or,     7 slot,                             ; \ bitwise or
: xor,    8 slot,                             ; \ bitwise xor
: shl,    9 slot,                             ; \ shift left
: shr,   10 slot,                             ; \ shift right
: in,    11 slot,                             ; \ input character
: out,   12 slot,                             ; \ output character
: read,  13 slot,                             ; \ read block
: write, 14 slot,                             ; \ write block
: ld16+, 15 slot,                             ; \ fetch cell at address, and increment over
: ld8+,  16 slot,                             ; \ fetch byte at address, and increment over
: st16+, 17 slot,                             ; \ store cell at address, and increment over
: st8+,  18 slot,                             ; \ store byte at address, and increment over
: lit16, 19 slot,                           , ; \ fetch literal next cell
: lit8,  20 slot,             verify-sbyte c, ; \ fetch literal next signed byte
: 0jump, 21 slot, here      - verify-sbyte c, ; \ jump to relative address in next cell if T >= 0
: next,  22 slot, here swap - verify-ubyte c, ; \ if R <= 0, drop R and continue, otherwise R-- and loop back to relative address in next cell
: drop,  23 slot,                             ; \ drop top of stack
: dup,   24 slot,                             ; \ duplicate top of stack
: over,  25 slot,                             ; \ yx -> yxy
: swap,  26 slot,                             ; \ yx -> xy
: push,  27 slot,                             ; \ push top of data stack to return stack
: pop,   28 slot,                             ; \ pop top of return stack to data stack
: peek,  29 slot,                             ; \ copy top of return stack to data stack
: ret,   30 slot,                             ; \ return from call
: nop,   31 slot,                             ; \ no-op

: align, initslot here 1 and 0<> if 1 h +! then ; \ align here on even address boundary
: for, push, initslot here ; \ start for/next loop

: call, ( addr -- ) initslot dup 1 and 0= if , else ." Expected even-aligned address" throw then ;
: jump, ( addr -- ) call, ret, ;

: literal, dup -128 127 within if lit8, else lit16, then ; \ TODO: does within handle negatives?
: zero, dup, dup, xor, ; \ trick to push a zero

: >r, push, ; \ ( x -- ) ( R: x -- ) move x to return stack [synonym]
: r>, pop, ; \ ( -- x ) ( R: x -- ) move x from return stack [synonym]
: r@, peek, ; \ ( -- x ) ( R: x -- x ) copy x from return stack [synonym]
: emit, out, ; \ emit ( char -- ) write to console [synonym]
: key, in, ; \ key ( -- char ) read from console [synonym]

: 2>r, swap, push, push, ; \ ( y x -- ) ( R: -- y x ) move y x pair to return stack
: 2r>, pop, pop, swap, ; \ ( -- y x ) ( R: y x -- ) move y x pair from return stack
: nip, swap, drop, ; \ ( y x -- x ) drop second stack value
: tuck, swap, over, ; \ ( y x -- x y x ) copy top stack value under second value
: 2dup, over, over, ; \ ( y x -- y x y x ) duplicate top two stack values
: 2drop, drop, drop, ; \ ( y x -- ) remove top two stack values

: label ( -- addr ) here constant ;
: skip, ( -- dest ) 0 call, here 2 - ; \ call, but expected to halt and never return
: start, ( orig -- ) align, here swap s! ;

: write-boot-block ( -- ) 0 0 here write-block ; \ note: depends on redefined `here`