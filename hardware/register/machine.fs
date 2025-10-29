require memory.fs

create registers 16 cells allot
registers 16 cells erase

: 32bit ( n -- n32 ) dup 15 rshift 0<> if -1 16 lshift or then ;
: 16bit ( n -- n16 ) $ffff and ;
: reg ( i -- addr ) cells registers + ;
: reg+! ( v reg -- ) swap @ over +! dup @ 16bit swap ! ;
: fetch-pc registers @ memory + c@ 1 registers +! ;
: nybbles ( byte -- n2 n1 ) dup $f and swap 4 rshift ;
: xyz ( x -- reg-x reg-y reg-z ) reg fetch-pc nybbles reg swap reg ;
: binop ( x op -- ) swap xyz >r @ 32bit swap @ 32bit rot execute 16bit r> ! ;

: nand and invert ;
: shr swap 16bit swap rshift ;

: step ( -- ) fetch-pc nybbles
  case
     0 of reg @ (bye) endof                                      \ HALT
     1 of fetch-pc dup $80 and if $ff00 or then swap reg ! endof \ LDC
     2 of xyz over @ s@ swap ! reg+! endof                       \ LD+
     3 of xyz @ over @ s! reg+! endof                            \ ST+
     4 of xyz rot @ 0= if swap @ swap ! else 2drop then endof    \ CP?
     5 of ['] + binop endof                                      \ ADD
     6 of ['] - binop endof                                      \ SUB
     7 of ['] * binop endof                                      \ MUL
     8 of ['] / binop endof                                      \ DIV
     9 of ['] nand binop endof                                   \ NAND
    10 of ['] lshift binop endof                                 \ SHL
    11 of ['] shr binop endof                                    \ SHR
    12 of stdin key-file swap reg ! endof                        \ IN
    13 of reg @ emit endof                                       \ OUT
    14 of xyz @ swap @ rot @ read-block endof                    \ READ
    15 of xyz @ swap @ rot @ write-block endof                   \ WRITE
    throw
  endcase ;

: steps ( n -- ) 0 do step loop ;
: run ( -- ) begin step again ;

: soft-reset ( -- ) registers 16 cells erase ;
: hard-reset ( -- ) soft-reset memory memory-size erase memory h ! ;
: reboot ( -- ) hard-reset read-boot-block run ;