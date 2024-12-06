require assembler.fs \ memory

create registers 16 cells allot  registers 16 cells 0 fill

: 16bit ( n -- n16 ) $ffff and ;
: reg ( i -- addr ) cells registers + ; \ register address
: reg+! ( v reg -- ) swap @ over +! dup @ 16bit swap ! ; \ +! while ensuring 16-bit
: fetch-pc registers @ memory + c@ 1 registers +! ; \ fetch [pc++]
: nybbles ( byte -- n2 n1 ) dup $f and swap 4 rshift ( $f and ) ; \ split byte into nybbles 
: xyz ( x -- reg-x reg-y reg-z ) reg fetch-pc nybbles reg swap reg ;
: binop ( x op -- ) swap xyz >r @ swap @ rot execute 16bit r> ! ; \ execute binary operation ( y x -- )

: step ( -- ) fetch-pc nybbles \ fetch instruction
  case
     0 of cr ." Halt " reg @ . quit endof                        \ halt(x) (halt with exit code x)
     1 of fetch-pc dup $80 and if $ff00 or then swap reg ! endof \ ldc x=v (load constant signed byte into x)
     2 of xyz over @ s@ swap ! reg+! endof                       \ ld+ z<-[y] y+=x (load from memory and inc/dec pointer)
     3 of xyz @ over @ s! reg+! endof                            \ st+ z->[y] y+=x (store to memory and inc/dec poniter)
     4 of xyz rot @ 0= if swap @ swap ! else 2drop then endof    \ cp? z=y if x=0 (conditional copy)
     5 of ['] + binop endof                                      \ add z=y+x (addition)
     6 of ['] - binop endof                                      \ sub z=y-x (subtraction)
     7 of ['] * binop endof                                      \ mul z=y*x (multiplication)
     8 of ['] / binop endof                                      \ div z=y/x (division)
     9 of [: and invert ;] binop endof                           \ nand z=y nand x (not-and)
    10 of ['] lshift binop endof                                 \ shl z=y<<x (bitwise shift-left)
    11 of [: swap 16bit swap rshift ;] binop endof               \ shr z=y>>x (bitwise shift-right)
    12 of key swap reg s! endof                                  \ in x=getc() (read from console)
    13 of reg @ emit endof                                       \ out putc(x) (write to console)
    14 of xyz @ rot @ rot @ read-block endof                     \ read(z,y,x)  (file z of size y -> address x)
    15 of xyz @ rot @ rot @ write-block endof                    \ write(z,y,x) (file z of size y <- address x)
    throw
  endcase ;
: steps ( n -- ) 0 do step loop ;
: run begin step again ;

: soft-reset ( -- ) registers 16 cells 0 fill ; \ reset registers, memory, h
: hard-reset ( -- ) soft-reset memory memory-size 0 fill memory h ! ; \ reset registers, memory, h
: reboot ( -- ) hard-reset read-boot-block run ; \ reboot from block0 image