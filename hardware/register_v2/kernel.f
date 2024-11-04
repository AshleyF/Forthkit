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

sym and 0 header, label 'and
              x popd,
              y popd,
          z x y and,
              z pushd,
                ret,

sym not 0 header, label 'not
              x popd,
            x x not,
              x pushd,
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

sym rot 0 header, label 'rot
              x popd,
              y popd,
              z popd,
              y pushd,
              x pushd,
              z pushd,
                ret,

sym drop 0 header, label 'drop
              x popd,
                ret,

sym '+ 0 header, label 'plus
              x popd,
              y popd,
          x x y add,
              x pushd,
                ret,

sym '1+ 0 header, label 'one-plus
              x popd,
        x x one add,
              x pushd,
                ret,

sym '1- 0 header, label 'one-minus
              x popd,
        x x one sub, ( TODO -1 add, )
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
sym 1- 0 header, label '1-
              x popd,
        x x one sub,
              x pushd,
                ret,


sym 2+ 0 header, label '2+
              x popd,
        x x two add,
              x pushd,
                ret,

sym 2- 0 header, label '2-
              x popd,
        x x two sub,
              x pushd,
                ret,

sym 'allot 0 header, label 'allot
          'here call,
          'plus call,
             'h call,
         'store jmp,

sym , 0 header, label 'comma
( here ! here 2+ h ! )
          'here call,
         'store call,
              2 literal,
         'allot jmp,

sym c, 0 header, label 'c-comma
( here c! here 1+ h ! )
          'here call,
       'c-store call,
              1 literal,
         'allot jmp,

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

: branch, 0 jmp, here 2 - ( pointer to address ) ;

: 0branch, ( compile branch if TOS is 0, push address of branch address )
  x popd, ( condition )
  0 y lit, ( address )
  here 2 - ( pointer to address )
  pc y x cp?, ( branch to address if condition is zero )
;

: if, 0branch, ;                      ( compile branch if TOS is 0, push address of branch address )
: then, here swap m! ;                ( patch previous branch to here )
: else, branch, swap then, ;          ( patch previous branch to here and start unconditional branch over false condition )

: begin, here ;
: until, 0branch, m! ;

sym parse-name 0 header, label 'parse-name
             0 literal, ( dummy )
               begin,
         'drop call,
          'key call,
          'dup call,
            33 literal,
    'less-than call,
          'not call,
               until,
         'here call, ( address )
         'swap call,
      'c-comma call, ( first char )
             0 literal, ( length )
               begin,
     'one-plus call,
          'key call,
          'dup call,
      'c-comma call,
            33 literal,
    'less-than call,
               until,
         'here call,
    'one-minus call,
            'h call,
        'store call,
               ret,

sym over 0 header, label 'over
             x popd,
             y popd,
             y pushd,
             x pushd,
             y pushd,
               ret,

sym / 0 header, label 'slash
             x popd,
             y popd,
         z y x div,
             z pushd,
               ret,

sym * 0 header, label 'star
             x popd,
             y popd,
         z y x mul,
             z pushd,
               ret,

sym - 0 header, label 'minus
             x popd,
             y popd,
         z y x sub,
             z pushd,
               ret,

sym + 0 header, label 'plus
             x popd,
             y popd,
         z x y add,
             z pushd,
               ret,

sym mod 0 header, label 'mod
        'over call,
        'over call,
       'slash call,
        'star call,
       'minus jmp,

sym emit 0 header, label 'emit
             x popd,
             x out,
               ret,

sym cr 0 header, label 'c-r
            10 literal, ( TODO CRLF? )
         'emit jmp,

sym . 0 header, label 'dot
          'dup call,
             z popd,
          15 x ldc,
         z z x shr, ( sign bit to 1s place - 1 if negative, 0 otherwise )
             z pushd,
               if,
            45 literal,
         'emit call,
            -1 literal,
        'slash call, ( TODO negate )
               then,
          zero pushd,
         'swap call,
               begin,
          'dup call,
            10 literal,
          'mod call,
            -1 literal, ( TODO true )
          'rot call,
            10 literal,
        'slash call,
          'dup call,
             0 literal,
       'equals call,
               until,
         'drop call,
         label 'repeat ( TODO: structured )
               if, ( TODO while, or something )
            48 literal,
         'plus call,
         'emit call,
       'repeat jmp,
               then,
               ret,

: header,
  latest @ here latest ! , ( link to previous word, update latest to this word ) ( TODO: doesn't seem to work )
  over or c, ( length/flag )
  0 do c, loop ( name )
;

sym create-header 0 header, label 'create-header ( non-standard )
             0 literal, ( TODO: latest link )
        'comma call,
         'here call, ( address of length )
             0 literal, ( dummy length )
      'c-comma call,
   'parse-name call,
         'swap call,
         'drop call, ( don't need address of name because we know it uses dictionary space )
         'swap call,
      'c-store call, ( overwrite length -- assumed < 256 )
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

-123 literal,
'dot call,
'c-r call,

123 literal,
'dot call,
'c-r call,

0 literal,
'dot call,
'c-r call,

42 literal,
'dot call,
'c-r call,

2024 literal,
'h call,
'store call,

0 x ldc,
x pushd,

if,

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

else,

4 x ldc,
x pushd,
'c-comma call,

5 x ldc,
x pushd,
'c-comma call,

6 x ldc,
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

then,

begin,
  'key call,
  'dup call,
  'c-comma call,
  32 literal,
  'equals call,
until,

'create-header call,
'create-header call,
'create-header call,

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