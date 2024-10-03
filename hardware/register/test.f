( simple assembler/VM test - capitalize [-32] console input )
( requires assembler )

0 constant u
1 constant c

  u 32 ldc,
 label &start
     c in,
 c u c sub,
     c out,
&start jump,

assemble
