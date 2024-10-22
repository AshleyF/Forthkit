init,

11 constant x
12 constant y
13 constant z
14 constant callsize
15 constant r

8 callsize ldc,
32767 r lit,

: call, a pc cp, a a callsize add, r r two sub, r a st, jmp, ;
: ret, pc r two ld+, ;

128 constant FLAG
variable latest

: header,
  latest @ here latest ! , ( link to previous word, update latest to this word )
  over or c, ( length/flag )
  0 do c, loop ( name )
;

ahead,

sym key 0 header,
  label 'key
      x in,
y x one add,
 'key y jmz,
        ret,

continue, ( patch jump ahead, )

65 x lit,
x out,
'key call,
x out,

0 halt,

assemble
