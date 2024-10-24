init,

14 constant z
13 constant y
12 constant x
11 constant u
10 constant r
 9 constant d
 8 constant one
 7 constant four
 6 constant -four

     1 one ldc,
    4 four ldc,
  -4 -four ldc,

: push, ( rp- ) swap -four st+, ;
: pop, ( rp- ) dup dup four add, ld, ;

32764 d lit,
: pushd, d push, ;
: popd, d pop, ;

32766 r lit,
: pushr, r push, ;
: popr, r pop, ;

: call, pc pushr, jmp, ; ( 6 bytes! )
: ret, t popr, t t four add, pc t cp, ; ( uses t, 8 bytes )

128 constant FLAG
variable latest

: header,
  latest @ here latest ! , ( link to previous word, update latest to this word )
  over or c, ( length/flag )
  0 do c, loop ( name )
;

ahead,

sym key 0 header, label 'key
      x in,
z x one add,
 'key z jmz,
      x pushd,
        ret,

continue, ( patch jump ahead, )

'key call,
'key call,
'key call,

x popd,
y popd,
z popd,

x out,
y out,
z out,

32767 t lit,
one t zero write,

assemble

zero halt,
