require machine.fs

reset ." Test ldc "
     42 x ldc,
    step
    x reg @ 42 = . cr

reset ." Test ld+ "
  42 100 memory + s!
      5 x ldc,
    100 y ldc,
    z y x ld+,
    3 steps
    z reg @ 42 = y reg @ 105 = and . cr

reset ." Test st+ "
      5 x ldc,
     42 y ldc,
    100 z ldc,
    z y x st+,
    4 steps
    100 memory + s@ 42 = z reg @ 105 = and . cr

reset ." Test cp? true "
     -1 x ldc,
     42 y ldc,
    100 z ldc,
    z y x cp?,
    4 steps
    z reg @ 42 = . cr

reset ." Test cp? false "
      0 x ldc,
     42 y ldc,
    100 z ldc,
    z y x cp?,
    4 steps
    z reg @ 100 = . cr

reset ." Test add "
      7 x ldc,
     42 y ldc,
    z y x add,
    3 steps
    z reg @ 42 7 + = . cr

reset ." Test sub "
      7 x ldc,
     42 y ldc,
    z y x sub,
    3 steps
    z reg @ 42 7 - = . cr

reset ." Test mul "
      7 x ldc,
     42 y ldc,
    z y x mul,
    3 steps
    z reg @ 42 7 * = . cr

reset ." Test div "
      7 x ldc,
     42 y ldc,
    z y x div,
    3 steps
    z reg @ 42 7 / = . cr

reset ." Test nand "
      7 x ldc,
     42 y ldc,
    z y x nand,
    3 steps
    z reg @ 42 7 and invert = . cr

reset ." Test shl "
      4 x ldc,
      1 y ldc,
    z y x shl,
    3 steps
    z reg @ 1 4 lshift = . cr

reset ." Test shr "
      4 x ldc,
      1 y ldc,
    z y x shr,
    3 steps
    z reg @ 1 4 rshift = . cr

reset ." Test in "
        x in,
    step
    x reg @ emit cr

reset ." Test out "
     65 x ldc,
        x out,
    2 steps cr

reset ." Test read "
      1 x ldc,
      2 y ldc,
      3 z ldc,
    z y x read,
    4 steps cr

reset ." Test write "
      1 x ldc,
      2 y ldc,
      3 z ldc,
    z y x write,
    4 steps cr

reset ." Test halt "
     -1 x ldc,
        x halt,
  100 steps