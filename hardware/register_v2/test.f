init,

12 constant one
13 constant x
14 constant y
15 constant z

        init,
  1 one ldc,
   32 y ldc,

label 'loop
      x in,
z x one add,
'loop z jmz,
  x x y sub,
      x out,
  'loop jmp,

assemble
