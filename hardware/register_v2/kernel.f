14 constant z
13 constant y
12 constant x
11 constant w
10 constant u
 9 constant r
 8 constant d
 7 constant one
 6 constant four
 5 constant -four

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

: literal, x lit, x pushd, ; ( TODO: single byte when possible -- couldn't bring myself to make a clit, or cliteral, word! ha! )

variable latest

: header,
  latest @ here latest ! , ( link to previous word, update latest to this word ) ( TODO: doesn't seem to work )
  over or c, ( length/flag )
  0 do c, loop ( name )
;

ahead,

: variable,
  x pc cp,
  pc pc two add, ( skip value )
  0 , ( append value )
  x x two add, ( point at value )
  x pushd,
  ret,
;

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

sym c@ 0 header, label 'c-fetch
              x popd,
            x x ld,
          255 y lit, 
          x x y and,
              x pushd,
                ret,

sym c! 0 header, label 'c-store
              x popd, ( x = address )
              y popd, ( y = value to store )
          255 z lit, 
          y y z and, ( mask to lower )
            z z not,
            w x ld, ( w = existing value )
          w w z and, ( mask to upper )
          y y w or, ( combine )
            x y st,
                ret,

sym c! 0 header, label 'cc-store
              x popd, ( x = address )
              y popd, ( y = values to store )
          255 z lit,
          y y z and,
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
         'fetch jmp,

sym 1+ 0 header, label '1+
              x popd,
        x x one add,
              x pushd,
                ret,

sym 2+ 0 header, label '2+
              x popd,
        x x two add,
              x pushd,
                ret,

sym , 0 header, label 'comma
( here ! here 2+ h ! )
          'here call,
         'store call,
          'here call, ( TODO: dup? )
            '2+ call,
             'h call,
         'store jmp,

sym c, 0 header, label 'c-comma
( here c! here 1+ h ! )
          'here call,
       'c-store call,
          'here call, ( TODO: dup? )
            '1+ call,
             'h call,
         'store jmp,

: true, -1 swap ldc, ;
: false, 0 swap ldc, ;
: sign-bit,  -32768 swap lit, ;

sym = 0 header, label 'equals
             x popd,
             y popd,
         z x y sub, ( zero if equal )
             x true,
             y false,
         y x z cp?,
             y pushd,
               ret,

sym negate 0 header, label 'negate
             x popd,
           x x not,
       x x one add,
             x pushd,
               ret,

sym < 0 header, label 'less-than
             x popd,
             y popd,
         z y x sub, ( negative if less )
          15 x ldc,
         z z x shr, ( sign bit to 1s place - 1 if negative, 0 otherwise )
             z pushd,
               ret,
       'negate jmp, ( -1 if negative, 0 otherwise )

sym word 0 header, label 'word
          'key call,
            33 literal,
    'less-than call,
               ret,

( move compile-time h to runtime h )
       here literal,
      'here call,
     'store call,

continue, ( patch jump ahead, )

(
'key call,
'key call,
'key call,

x popd,
y popd,
z popd,

x out,
y out,
z out,
)

1024 literal,
'h call,
'store call,

42 x ldc,
x pushd,
'c-comma call,

1 x ldc,
x pushd,
'c-comma call,

2 x ldc,
x pushd,
'c-comma call,

3 x ldc,
x pushd,
'c-comma call,

4 x ldc,
x pushd,
'c-comma call,

5 x ldc,
x pushd,
'c-comma call,

7 x ldc,
x pushd,
'c-comma call,

8 x ldc,
x pushd,
'c-comma call,

999 x ldc,
x pushd,
'c-comma call,

: foo
  'key call,
  'c-comma call,
;

foo foo foo foo foo foo

7 y lit, y pushd,
8 x lit, x pushd,
'less-than call,
z popd,

(
      'here call,
     'store call,
         'h call,
          x popd,
    x x two add,
          x pushd,
     'store call,
            ret,

'h call,
'plus-store call,
)

(
x pushd,
'comma call,
y pushd,
'comma call,
z pushd,
'comma call,
)

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