14 constant z
13 constant y
12 constant x
11 constant u
10 constant r
 9 constant d
 8 constant one
 7 constant four
 6 constant -four

2 two ldc,

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

variable latest

: header,
  latest @ here latest ! , ( link to previous word, update latest to this word ) ( TODO: doesn't seem to work )
  over or c, ( length/flag )
  0 do c, loop ( name )
;

ahead,

: variable,
       10 y ldc,
     x pc y add, ( address of inline 0 value below )
          x pushd,
            ret,
          0 , ;

( TODO remove labels below )

sym key 0 header, label 'key
          x in,
    y x one add,
     'key y jmz,
          x pushd,
            ret,

sym @ 0 header, label 'fetch
          x popd,
        x x ld,
          x pushd,
            ret,

sym ! 0 header, label 'store
          x popd,
          y popd,
        x y st,
            ret,

sym dup 0 header, label 'dup
          x popd,
          x pushd,
          x pushd,
            ret,

sym swap 0 header, label 'swap
          x popd,
          y popd,
          x pushd,
          y pushd,
            ret,

sym '+ 0 header, label 'plus
          x popd,
          y popd,
      x x y add,
          x pushd,
            ret,

label 'h variable, ( internal )

sym here 0 header, label 'here
         'h call,
       'dup call,
     'fetch call,
            ret,

sym >r 0 header, label 'to-r
          x popd,
          x pushr,
            ret,

sym r> 0 header, label 'r-from
          x popr,
          x pushd,
            ret,

sym +! 0 header, label 'plus-store
( dup >r @ + r> ! )
       'dup call,
      'to-r call,
     'fetch call,
      'plus call,
    'r-from call,
     'store call,
            ret,

: literal,
       x pc ld,
          ,
          x pushd,
            ret,
;

sym , 0 header, label 'append
( here ! 2 h +! )
      'here call,
     'store call,
          2 literal,
         'h call,
'plus-store call,
            ret,

( move compile-time h to runtime h )
      h @ x lit,
          x pushd,
         'h call,
     'store call,

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

x pushd,
'append call,
y pushd,
'append call,
z pushd,
'append call,

32767 t lit,
one t zero write,

(
42 x ldc,
x pushd,
'h call,
'store call,

'h call,
'fetch call,
y popd,
)

assemble

zero halt,
