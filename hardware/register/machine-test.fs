require machine.fs
require assembler.fs

2 constant x
3 constant y
4 constant z

hard-reset ." Test ldc "
     42 x ldc,
    step
    x reg @ 42 = . cr

hard-reset ." Test ld+ "
  42 100 s!
      5 x ldc,
    100 y ldc,
    z y x ld+,
    3 steps
    z reg @ 42 = y reg @ 105 = and . cr

hard-reset ." Test st+ "
      5 x ldc,
    100 y ldc,
     42 z ldc,
    z y x st+,
    4 steps
    100 s@ 42 = y reg @ 105 = and . cr

hard-reset ." Test cp? zero "
      0 x ldc,
     42 y ldc,
    100 z ldc,
    z y x cp?,
    4 steps
    z reg @ 42 = . cr

hard-reset ." Test cp? non-zero "
      7 x ldc,
     42 y ldc,
    100 z ldc,
    z y x cp?,
    4 steps
    z reg @ 100 = . cr

hard-reset ." Test add "
      7 x ldc,
     42 y ldc,
    z y x add,
    3 steps
    z reg @ 42 7 + = . cr

hard-reset ." Test sub "
      7 x ldc,
     42 y ldc,
    z y x sub,
    3 steps
    z reg @ 42 7 - = . cr

hard-reset ." Test mul "
      7 x ldc,
     42 y ldc,
    z y x mul,
    3 steps
    z reg @ 42 7 * = . cr

hard-reset ." Test div "
     10 x ldc,
    -20 y ldc,
    z y x div,
    3 steps
    z reg @ -20 10 / $ffff and = . cr

hard-reset ." Test nand "
      7 x ldc,
     42 y ldc,
    z y x nand,
    3 steps
    z reg @ 42 7 and invert $ffff and = . cr

hard-reset ." Test shl "
      4 x ldc,
      1 y ldc,
    z y x shl,
    3 steps
    z reg @ 1 4 lshift = . cr

hard-reset ." Test shr "
      4 x ldc,
     -7 y ldc,
    z y x shr,
    3 steps
    z reg @ -7 64 16 4 - - rshift = . cr

hard-reset ." Test in -- press a key: "
        x in,
    step
    x reg @ emit cr

hard-reset ." Test out "
     65 x ldc,
        x out,
    2 steps
    ."  (should print A)" cr

hard-reset ." Test write "
      5 x ldc,
     10 y ldc,
      0 z ldc,
    z y x write,
   memory 10 + h ! \ poke Hello into memory[10]
   'H' c,
   'e' c,
   'l' c,
   'l' c,
   'o' c,
    4 steps ." (confirmed by read)" cr

hard-reset ." Test read "
      5 x ldc,
     20 y ldc,
      0 z ldc,
    z y x read,
    4 steps
    memory 20 + c@ 'H' =
    memory 21 + c@ 'e' = and
    memory 22 + c@ 'l' = and
    memory 23 + c@ 'l' = and
    memory 24 + c@ 'o' = and . cr

hard-reset ." Test halt "
     42 x ldc,
        x halt,
  100 steps