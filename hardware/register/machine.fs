require assembler.fs \ memory

create registers 16 cells allot  registers 16 cells 0 fill

: bit ( n -- mask ) 1 swap lshift ; \ make nth-bit mask (e.g. 15 -> $8000)
: >signed ( val bits -- signed ) 2dup 1- bit and 0<> if bit 1- invert or else drop then ; \ signed n-bits
: >s ( val -- signed ) 16 >signed ;

: reg ( i -- addr ) cells registers + ; \ register address
: reg+! ( v reg -- ) swap @ over +! dup @ >s swap ! ; \ +! while ensuring signed 16-bit

: fetch-pc registers @ memory + c@ 1 registers +! ; \ fetch [pc++]
: nybbles ( byte -- n2 n1 ) dup $f and swap 4 rshift ( $f and ) ; \ split byte into nybbles 
: xyz ( x -- reg-x reg-y reg-z ) reg fetch-pc nybbles reg swap reg ;

: mem ( reg -- addr ) @ memory + ; \ memory address pointed to by register
: s! ( val addr -- ) over 8 rshift over 1+ c! c! ; \ store 16-bit value at address
: s@ ( addr -- val ) dup c@ swap 1+ c@ 8 lshift or ; \ fetch 16-bit value from address
: binop ( x op -- ) swap xyz >r @ swap @ rot execute >s r> ! ; \ execute binary operation ( y x -- )

: step ( -- ) fetch-pc nybbles \ fetch instruction
  case
     0 of ." Halt " reg @ . quit endof                        \ halt(x) (halt with exit code x)
     1 of fetch-pc 8 >signed swap reg ! endof                 \ ldc x=v (load constant signed byte into x)
     2 of xyz over mem s@ swap ! reg+! endof                  \ ld+ z=[y] y+=x (load from memory and inc/dec pointer)
     3 of xyz swap @ over mem s! reg+! endof                  \ st+ [z]=y z+=x (store to memory and inc/dec poniter)
     4 of xyz rot @ 0= if swap @ swap ! else 2drop then endof \ cp? z=y if x (conditional copy)
     5 of ['] + binop endof                                   \ add z=y+x (addition)
     6 of ['] - binop endof                                   \ sub z=y-x (subtraction)
     7 of ['] * binop endof                                   \ mul z=y*x (multiplication)
     8 of ['] / binop endof                                   \ div z=y/x (division)
     9 of [: and invert ;] binop endof                        \ nand z=y nand x (not-and)
    10 of ['] lshift binop endof                              \ shl z=y<<x (bitwise shift-left)
    11 of ['] rshift binop endof                              \ shr z=y>>x (bitwise shift-right)
    12 of key swap reg s! endof                               \ in x=getc() (read from console)
    13 of reg @ emit endof                                    \ out putc(x) (write to console)
    14 of xyz @ rot @ rot @ read-block endof                  \ read(z,y,x)  (file z of size y -> address x)
    15 of xyz @ rot @ rot @ write-block endof                 \ write(z,y,x) (file z of size y <- address x)
    throw
  endcase ;
: steps ( n -- ) 0 do step loop ;
: run begin step again ;

: soft-reset ( -- ) registers 16 cells 0 fill ; \ reset registers, memory, h
: hard-reset ( -- ) soft-reset memory memory-size 0 fill memory h ! ; \ reset registers, memory, h
: reboot ( -- ) hard-reset read-boot-block run ; \ reboot from block0 image