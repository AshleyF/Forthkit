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

: call, pc pushr, jump, ; ( 6 bytes! )
: ret, t popr, t t four add, pc t cp, ; ( uses t, 8 bytes )

: literal, x lit, x pushd, ; ( TODO: single byte when possible -- couldn't bring myself to make a clit, or cliteral, word! ha! )

variable latest

: header,
  latest @ here latest ! , ( link to previous word, update latest to this word )
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

sym >r 0 header, label 'to-r
              x popd,
              y popr,
              x pushr,
              y pushr, ( keep return address on top! )
                ret,
                
sym r> 0 header, label 'r-from
              x popr, ( actual return address )
              y popr, ( value on top before calling )
              y pushd,
              x pushr, ( keep return address! )
                ret,

sym r@ 0 header, label 'r-fetch
              x popr, ( actual return address )
              y popr, ( value on top before calling )
              y pushd,
              y pushr,
              x pushr, ( keep return address! )
                ret,

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

sym -rot 0 header, label '-rot ( non-standard )
           'rot call,
           'rot jump,

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
         'fetch jump,

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
         'store jump,

sym , 0 header, label 'comma
( here ! here 2+ h ! )
          'here call,
         'store call,
              2 literal,
         'allot jump,

sym c, 0 header, label 'c-comma
( here c! here 1+ h ! )
          'here call,
       'c-store call,
              1 literal,
         'allot jump,

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
       'negate jump, ( -1 if negative, 0 otherwise )

: branch, 0 jump, here 2 - ( pointer to address ) ;

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
         'here call, ( save h )
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
          'rot call,
            'h call,
        'store call, ( restore h )
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
       'minus jump,

sym emit 0 header, label 'emit
             x popd,
             x out,
               ret,

sym cr 0 header, label 'c-r
            10 literal, ( TODO CRLF? )
         'emit jump,

sym base 0 header, label 'base
               variable,

45 constant '-'
48 constant '0'
63 constant '?'
65 constant 'A'
97 constant 'a'

sym . 0 header, label 'dot
          'dup call,
             z popd,
          15 x ldc,
         z z x shr, ( sign bit to 1s place - 1 if negative, 0 otherwise )
             z pushd,
               if,
           '-' literal,
         'emit call,
            -1 literal,
        'slash call, ( TODO negate )
               then,
          zero pushd,
         'swap call,
               begin,
          'dup call,
         'base call,
        'fetch call,
          'mod call,
            -1 literal, ( TODO true )
          'rot call,
         'base call,
        'fetch call,
        'slash call,
          'dup call,
             0 literal,
       'equals call,
               until,
         'drop call,
         label 'repeat ( TODO: structured )
               if, ( TODO while, or something )
         'dup call,
            10 literal,
    'less-than call,
               if,
           '0' literal, ( '0' )
               else,
      'A' 10 - literal, ( 10 = 'A' )
               then,
         'plus call,
         'emit call,
       'repeat jump,
               then,
               ret,

label 'latest variable, ( internal )

sym header 0 header, label 'header ( non-standard )
       'latest call,
        'fetch call,
         'here call,
       'latest call,
        'store call,
        'comma call,
         'here call, ( address of length )
             0 literal, ( dummy length )
      'c-comma call,
   'parse-name call,
          'dup call,
        'allot call, ( reserve space for name )
         'swap call,
         'drop call, ( don't need address of name because we know it uses dictionary space )
         'swap call,
      'c-store call, ( overwrite length -- assumed < 256 )
               ret,

sym true 0 header, label 'true ( TODO: check name )
            -1 literal,
               ret,

sym false 0 header, label 'false ( TODO: check name )
             0 literal,
               ret,

sym 2dup 0 header, label '2dup
         'over call,
         'over jump,
               
sym 2drop 0 header, label '2drop
         'drop call,
         'drop jump,

: do,
  'swap call,
  'to-r call,
  'to-r call,
  here ( compile-time )
;

: loop,
       'r-from call, ( start )
             1 literal,
         'plus call,
          'dup call,
      'r-fetch call,
         'swap call,
         'to-r call,
    'less-than call,
               if,
          swap jump, ( to addr of do )
               then,
       'r-from call,
         'drop call,
       'r-from call,
         'drop call,
;

sym type 0 header, label 'type
         'over call, ( addr len addr )
         'plus call, ( addr end )
         'swap call, ( end start )
               do,
      'r-fetch call, ( addr ) ( TODO i word )
      'c-fetch call, ( char )
         'emit call, ( )
               loop,
               ret,
  
sym within 0 header, label 'within ( definition from standard )
         'over call,
        'minus call,
         'to-r call,
        'minus call,
       'r-from call,
    'less-than jump, ( TODO: u< )

sym >number 0 header, label 'to-number
         '2dup call,    ( num addr len addr len )
         'to-r call,    ( num addr len addr )
         'to-r call,    ( num addr len )
         'over call,    ( num addr len addr )
         'plus call,    ( num start end )
         'swap call,    ( num end start )
               do,      ( num )
         'base call,    ( num baseaddr )
        'fetch call,    ( num base ) ( TODO: support base > 10 )
         'star call,    ( num )
      'r-fetch call,    ( num addr ) ( TODO i word )
      'c-fetch call,    ( num char )
           '0' literal, ( num char '0' )
        'minus call,    ( num dig )
          'dup call,    ( num dig dig )
             0 literal, ( num dig dig 0 )
             9 literal, ( num dig dig 0 9 )
       'within call,    ( num dig bool )
          'not call,    ( num dig bool )
               if,      ( num dig )
         'drop call,    ( num )
       'r-from call,    ( num start )
         'drop call,    ( num )
       'r-from call,    ( num end )
         'drop call,    ( num )
       'r-from call,    ( num addr )
       'r-from call,    ( num addr len )
               ret,
               then,
         'plus call,    ( num )
               loop,
       'r-from call,    ( num addr )
       'r-from call,    ( num addr len )
         'plus call,    ( num next )
             0 literal, ( num next len )
               ret,

sym repl 0 header, label 'repl ( TODO: rename )
   'parse-name call,    ( addr len )
         'over call,    ( addr len addr )
      'c-fetch call,    ( addr len first )
           '-' literal, ( addr len first '-' )
       'equals call,    ( addr len bool )
               if,      ( addr len )
         'swap call,    ( len addr )
             1 literal, ( len addr 1 )
         'plus call,    ( len addr )
         'swap call,    ( addr len )
    'one-minus call,    ( addr len )
            -1 literal, ( addr len sign )
               else,
             1 literal, ( addr len sign )
               then,
         '-rot call,    ( sign addr len )
             0 literal, ( sign addr len num )
         '-rot call,    ( sign num addr len )
    'to-number call,    ( sign num addr len )
          'dup call,    ( sign num addr len )
             0 literal, ( sign num addr len len 0 )
       'equals call,    ( sign num addr len bool )
               if,      ( sign num addr len )
        '2drop call,    ( sign num )
         'star call,    ( num )
          
          'dot call,
          'c-r call,
          
               else,    ( sign num addr len )
         'type call,    ( sign num )
           '?' literal, ( sign num '?' )
         'emit call,    ( sign num )
        '2drop call,    ( )
               then,
32767 t lit,
one t zero write,

         'repl jump, ( TODO: recurse )
   
continue, ( patch jump ahead, )

( code from here down is off the end of the dictionary and will be overwritten/reclaimed! )

( move compile-time h to runtime h )
          here literal,
            'h call,
        'store call,

( move compile-time latest to runtime latest )
      latest @ literal,
       'latest call,
        'store call,

10 literal, 'base call, 'store call,

32767 t lit,
one t zero write,

'repl jump,

assemble

zero halt, ( TODO: remove )