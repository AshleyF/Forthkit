require kernel.fs

create registers 16 cells allot  registers 16 cells 0 fill

: reg ( i -- addr ) cells registers + ; \ register address

: nth-bit ( n -- mask ) 1 swap lshift ; \ make nth-bit mask (e.g. 15 -> $8000)
: n-bit-mask ( n -- mask ) nth-bit 1- ; \ make n-bit mask (e.g. 16 -> $ffff)
: >signed-n-bits ( val bits -- signed ) 2dup 1- nth-bit and 0<> if n-bit-mask invert or else drop then ;
: >s16 ( i -- i16 ) 16 >signed-n-bits ; \ to signed 16-bit value
: >s8  ( i -- i8 )   8 >signed-n-bits ; \ to signed 8-bit value
: reg+! ( v addr -- ) swap over +! dup @ >s16 swap ! ; \ +! while ensuring signed 16-bit

: fetch-pc registers @ memory + c@ 1 registers +! ; \ fetch [pc++]

: nybbles ( byte -- n2 n1 ) dup $f and swap 4 rshift ( $f and ) ; \ split byte into nybbles 

: xyz ( x -- reg-x reg-y reg-z ) reg fetch-pc nybbles reg swap reg ;
: mem ( reg -- addr ) @ memory + ; \ memory address pointed to by register
: s16! ( val addr -- ) over 8 rshift over 1+ c! c! ; \ store 16-bit value at address
: s16@ ( addr -- val ) dup c@ swap 1+ c@ 8 lshift or ; \ fetch 16-bit value from address

: binop ( x op -- ) swap xyz >r @ swap @ rot execute r> ! ; \ execute binary operation ( y x -- )
: step ( -- ) fetch-pc nybbles \ fetch instruction
  case
    0 of ( ." halt" .s cr ) reg @ (bye) endof                                \ halt(x) (halt with exit code x)
    1 of ( ." ldc"  .s cr ) fetch-pc >s8 swap reg ! endof                    \ ldc x=v (load constant signed byte into x)
    2 of ( ." ld+"  .s cr ) xyz over mem s16@ swap ! swap @ swap reg+! endof \ ld+ z=[y] y+=x (load from memory and inc/dec pointer)
    3 of ( ." st+"  .s cr ) xyz swap @ over mem s16! swap @ swap reg+! endof \ st+ [z]=y z+=x (store to memory and inc/dec poniter)
    4 of ( ." cp?"  .s cr ) xyz rot @ if swap @ swap ! else 2drop then endof \ cp? z=y if x (conditional copy)
    5 of ( ." add"  .s cr ) ['] + binop endof                                \ add z=y+x (addition)
    6 of ( ." sub"  .s cr ) ['] - binop endof                                \ sub z=y-x (subtraction)
    7 of ( ." mul"  .s cr ) ['] * binop endof                                \ mul z=y*x (multiplication)
    8 of ( ." div"  .s cr ) ['] / binop endof                                \ div z=y/x (division)
    9 of ( ." nand" .s cr ) [: and invert ;] binop endof                     \ nand z=y nand x (not-and)
   10 of ( ." shl"  .s cr ) ['] lshift binop endof                           \ shl z=y<<x (bitwise shift-left)
   11 of ( ." shr"  .s cr ) ['] rshift binop endof                           \ shr z=y>>x (bitwise shift-right)
   12 of ( ." in"   .s cr ) key swap reg s16! endof                          \ in x=getc() (read from console)
   13 of ( ." out"  .s cr ) reg @ emit endof                                 \ out putc(x) (write to console)
\ : in,    (   y x -- ) 12 2nybbles, ;    \ x=getc()     (read from console)
\ : out,   (   y x -- ) 13 2nybbles, ;    \ putc(x)      (write to console)
\ : read,  ( z y x -- ) 14 4nybbles, ;    \ read(z,y,x)  (file z of size y -> address x)
\ : write, ( z y x -- ) 15 4nybbles, ;    \ write(z,y,x) (file z of size y <- address x)
    throw
  endcase ;
: steps ( n -- ) 0 do step loop ;

( --- tests -------------------------------------------------------------- )

: reset memory h ! 0 registers ! ;

: test-ldc reset ." Test ldc "
     42 x ldc,
    step
    x reg @ 42 = . cr ;

: test-ld+ reset ." Test ld+ "
  42 100 memory + s16!
      5 x ldc,
    100 y ldc,
    z y x ld+,
    3 steps
    z reg @ 42 = y reg @ 105 = and . cr ;

: test-st+ reset ." Test st+ "
      5 x ldc,
     42 y ldc,
    100 z ldc,
    z y x st+,
    4 steps
    100 memory + s16@ 42 = z reg @ 105 = and . cr ;

: test-cp?-true reset ." Test cp? true "
     -1 x ldc,
     42 y ldc,
    100 z ldc,
    z y x cp?,
    4 steps
    z reg @ 42 = . cr ;

: test-cp?-false reset ." Test cp? false "
      0 x ldc,
     42 y ldc,
    100 z ldc,
    z y x cp?,
    4 steps
    z reg @ 100 = . cr ;

: test-add reset ." Test add "
      7 x ldc,
     42 y ldc,
    z y x add,
    3 steps
    z reg @ 42 7 + = . cr ;

: test-sub reset ." Test sub "
      7 x ldc,
     42 y ldc,
    z y x sub,
    3 steps
    z reg @ 42 7 - = . cr ;

: test-mul reset ." Test mul "
      7 x ldc,
     42 y ldc,
    z y x mul,
    3 steps
    z reg @ 42 7 * = . cr ;

: test-div reset ." Test div "
      7 x ldc,
     42 y ldc,
    z y x div,
    3 steps
    z reg @ 42 7 / = . cr ;

: test-nand reset ." Test nand "
      7 x ldc,
     42 y ldc,
    z y x nand,
    3 steps
    z reg @ 42 7 and invert = . cr ;

: test-shl reset ." Test shl "
      4 x ldc,
      1 y ldc,
    z y x shl,
    3 steps
    z reg @ 1 4 lshift = . cr ;

: test-shr reset ." Test shr "
      4 x ldc,
      1 y ldc,
    z y x shr,
    3 steps
    z reg @ 1 4 rshift = . cr ;

: test-in reset ." Test in "
        x in,
    step
    x reg @ 120 = . cr ;

: test-out reset ." Test out "
     65 x ldc,
        x out,
    2 steps cr ;

: test-halt reset ." Test halt"
        x halt,
  step ;

: test 
  test-ldc
  test-ld+
  test-st+
  test-cp?-true
  test-cp?-false
  test-add
  test-sub
  test-mul
  test-div
  test-nand
  test-shl
  test-shr
  test-in
  test-out
  test-halt ;
test