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
     14 y ldc, ( count this and following instruction )
    x x y add, ( point just beyond this code -- data field )
        x pushd,
          ret, ( 8 bytes )
          here 2 + h ! ( 2 allot )
;

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

sym or 0 header, label 'or
              x popd,
              y popd,
          z x y or,
              z pushd,
                ret,

sym not 0 header, label 'not
              x popd,
            x x not,
              x pushd,
                ret,

sym lshift 0 header, label 'lshift
              x popd,
              y popd,
          z x y shl,
              z pushd,
                ret,

sym rshift 0 header, label 'rshift
              x popd,
              y popd,
          z x y shr,
              z pushd,
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

 sym write 0 header, label 'write ( block size address -- )
             x popd,
             y popd,
             z popd,
         z y x write,
               ret,
 
sym halt 0 header, label 'halt
          zero halt, ( TODO: with error code? )

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

sym allot 0 header, label 'allot
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
: then, here swap ! ;                ( patch previous branch to here )
: else, branch, swap then, ;          ( patch previous branch to here and start unconditional branch over false condition )

: begin, here ;
: until, 0branch, ! ;

sym over 0 header, label 'over
             x popd,
             y popd,
             y pushd,
             x pushd,
             y pushd,
               ret,

sym pad 0 header, label 'pad ( -- addr  TODO bounds check, 1000? )
         'here call,
          1000 literal,
         'plus jump,

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

( TODO figure out -32768 bug! )
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
         'here call, ( save here )
       'latest call,
        'fetch call,
        'comma call,
         'here call, ( address of length )
             0 literal, ( dummy length )
      'c-comma call,
   'parse-name call,
          'rot call,
         'over call,
         'swap call,
      'c-store call, ( overwrite length -- assumed < 256 )
          'rot call,
            'h call,
        'store call, ( restore here )
               ret,  ( addr len )

sym immediate 0 header, label 'immediate
       'latest call,
        'fetch call,
             2 literal,
         'plus call,  ( latest @ cell+ or >name [non-standard] )
          'dup call,
        'fetch call,
           128 literal,
           'or call,  ( set immediate flag )
         'swap call,
        'store jump,

sym literal 128 header,
          3106 literal, 'comma call, ( 220C -> ld+ two pc x -- x=[pc] pc += 2 -- load and skip literal )
        'comma call,    ( literal value )
        -14283 literal, 'comma jump, ( 35C8 -> st+ -four x d -- [d]=x d+=-4 -- push literal )

sym true 0 header, label 'true
            -1 literal,
               ret,

sym false 0 header, label 'false
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
            10 literal, ( num dig dig 0 10 )
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

sym parse-number 0 header, label 'parse-number ( non-standard )
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
         'true call,    ( num true )
               else,    ( sign num addr len )
        '2drop call,    ( sign num )
        '2drop call,    ( )
        'false call,    ( false )
               then,
               ret,

sym debug 0 header, label 'debug
          'dup call,
          'dot call,
          'c-r jump,

sym parse 0 header, label 'parse ( char -- addr len )
         'here call,    ( char addr )
         'swap call,    ( addr char )
            -1 literal, ( addr char -1 )
               begin,
     'one-plus call,    ( addr char len )
         'over call,    ( addr char len char )
          'key call,    ( addr char len char key )
          'dup call,    ( addr char len char key key )
         '-rot call,    ( addr char len key char key )
       'equals call,    ( addr char len key bool )
          'dup call,    ( addr char len key bool bool )
          'not call,    ( addr char len key bool !bool )
               if,      ( addr char len key bool )
         'swap call,    ( addr char len bool key )
        'comma call,    ( addr char len bool )
               else,    ( addr char len key bool )
         'swap call,    ( addr char len bool key )
         'drop call,    ( addr char len bool )
               then,
               until,   ( addr char len )
         'swap call,    ( addr len char )
         'drop call,    ( addr len )
         'over call,    ( addr len addr )
            'h call,    ( addr len addr h )
        'store call,    ( addr len -- restore h )
               ret,

sym compstr 0 header, label 'compstr ( non-standard )
          'dup call,    ( w0 w1 len len )
             0 literal,
       'equals call,
               if,      ( w0 w1 len )
        '2drop call,    ( w0 )
         'drop call,    ( )
         'true call,    ( true )
               else,    ( w0 w1 len )
         '-rot call,    ( len w0 w1 )
         '2dup call,    ( len w0 w1 w0 w1 )
      'c-fetch call,    ( len w0 w1 w0 c1 )
         'swap call,    ( len w0 w1 c1 w0 )
      'c-fetch call,    ( len w0 w1 c1 c0 )
       'equals call,    ( len w0 w1 bool )
               if,      ( len w0 w1 )
     'one-plus call,    ( len w0 w1 )
          'rot call,    ( w0 w1 len )
    'one-minus call,    ( w0 w1 len )
          'rot call,    ( w1 len w0 )
     'one-plus call,    ( w1 len w0 )
         '-rot call,    ( w0 w1 len )
      'compstr call,    ( recurse! TODO no need to compare len again )
               else,    ( len w0 w1 )
        '2drop call,    ( len )
         'drop call,    ( )
        'false call,    ( false )
               then,
               then,
               ret,

label 'state variable,

sym find 0 header, label 'find
       'latest call,
        'fetch call,    ( c-find cur )
        'state call,    ( c-find cur stateaddr )
        'fetch call,    ( c-find cur state )
               if,      ( c-find cur )
        'fetch call,    ( c-find cur -- skip current word when compiling )
               then,
label 'findnext
         '2dup call,    ( c-find cur c-find cur )
             2 literal,
         'plus call,    ( c-find cur c-find c-word )
         '2dup call,    ( c-find cur c-find c-word c-find c-word )
      'c-fetch call,    ( c-find cur c-find c-word c-find lenflag )
           127 literal, ( mask )
          'and call,    ( c-find cur c-find c-word c-find len )
         'swap call,    ( c-find cur c-find c-word len c-find )
      'c-fetch call,    ( c-find cur c-find c-word len len )
         'over call,    ( c-find cur c-find c-word len len len )
       'equals call,    ( c-find cur c-find c-word len bool )
               if,      ( c-find cur c-find c-word len )
          'rot call,    ( c-find cur c-word len c-find )
     'one-plus call,    ( c-find cur c-word len c-find -- past len byte )
          'rot call,    ( c-find cur len c-find c-word )
     'one-plus call,    ( c-find cur len c-find c-word -- past len byte )
          'rot call,    ( c-find cur c-find c-word len )
      'compstr call,    ( c-find cur bool )
               if,      ( c-find cur )
         'swap call,    ( cur c-find )
         'drop call,    ( cur )
             2 literal, ( cur 2 )
         'plus call,    ( c-found )
          'dup call,    ( c-found c-found )
          'dup call,    ( c-found c-found c-found )
      'c-fetch call,    ( c-found c-found lenflag )
           127 literal, ( mask )
          'and call,    ( c-found c-found len )
         'plus call,    ( c-found xt )
     'one-plus call,    ( c-found xt )
         'swap call,    ( xt c-found )
      'c-fetch call,    ( xt lenflag )
           128 literal, ( xt lenflag flag )
         'swap call,    ( xt flag lenflag )
         'over call,    ( xt flag lenflag flag )
          'and call,    ( xt flag mask )
       'equals call,    ( xt bool )
               if,      ( xt )
             1 literal, ( xt 1 -- immediate )
               else,
            -1 literal, ( xt -1 -- not immediate )
               then,
               ret,
               then,
               else,    ( c-find cur c-find c-word len -- lengths don't match )
        '2drop call,    ( c-find cur c-find )
         'drop call,    ( c-find cur )
               then,
          'dup call,    ( c-find cur cur )
             0 literal, ( c-find cur cur 0 )
       'equals call,    ( c-find cur bool )
               if,      ( c-find cur )
         'drop call,    ( c-find )
             0 literal, ( c-find 0 )
               else,    ( c-find cur )
        'fetch call,    ( c-find next )
     'findnext call,    ( recurse! )
               then,
               ret,

sym execute 0 header, label 'execute
             x popd,
          pc x cp, ( jump from *runtime* stack )

sym postpone 128 header,
       'header call,    ( TODO: use parse-name and append length? )
         'over call,    ( addr len addr )
    'one-minus call,    ( addr len c-addr -- to length )
         'find call,    ( addr len [ c-addr 0 | xt 1 | xt -1 ] )
             1 literal,
       'equals call, ( only works for immediate words -- TODO: error for not found or non-immediate )
               if,
               ( compile call )
          2357 literal, 'comma call, ( 3509 -> st+ -four pc r -- [r]=pc r+=4 -- push pc )
            33 literal, 'comma call, ( 2100 -> ld+ zero pc pc -- pc=[pc] -- jump to following address )
        'comma call,    ( addr len )
               then,
               ret,

( make dict header from next token )
sym make 0 header, label 'make ( non-standard )
       'header call,    ( addr len )
         'swap call,    ( len addr )
             3 literal,
        'minus call,    ( len linkaddr )
       'latest call,
        'store call,    ( len )
             3 literal, ( length of link and lenflag byte )
         'plus call,    ( length of header )
        'allot jump,    ( include into dictionary )

( make dict header, compile push PFA and return )
sym create 0 header, label 'create
         'make call,
          3137 literal, 'comma call, ( 410C -> cp? zero pc x   x=pc )
          3613 literal, 'comma call, ( 1D0E -> ldc y 14        y=14 )
        -13219 literal, 'comma call, ( 5DCC -> add y x x       x=y+x )
        -14283 literal, 'comma call, ( 35C8 -> st+ -four x d   push x )
        -26282 literal, 'comma call, ( 5699 -> add four r r    ret )
        -27871 literal, 'comma call, ( 2193 -> ld+ zero r t )
         13142 literal, 'comma call, ( 5633 -> add four t t )
         12353 literal, 'comma call, ( 4130 -> cp? zero t pc )
               ret,

( patch return to jump to instance code address given )
sym (does) 0 header, label '(does)
       'latest call,
        'fetch call,
             2 literal,
         'plus call,    ( to length/flag )
          'dup call,
      'c-fetch call,
           127 literal, ( mask )
          'and call,    ( length )
         'plus call,    ( to end of name )
     'one-plus call,    ( to code )
             8 literal,
         'plus call,    ( to return )
            33 literal, ( 2100 -> ld+ zero pc pc -- pc=[pc] -- jump to following address )
         'over call,
        'store call,
             2 literal,
         'plus call,
        'store jump,   ( to address passed to us )

( compile push instance code address and jump to internal does )
sym does> 128 header, label 'does
         'here call,
            10 literal,
         'plus call,
          3106 literal, 'comma call, ( 220C -> ld+ two pc x -- x=[pc] pc += 2 -- load and skip literal )
        'comma call,    ( literal value )
        -14283 literal, 'comma call, ( 35C8 -> st+ -four x d -- [d]=x d+=-4 -- push literal )
            33 literal, 'comma call, ( 2100 -> ld+ zero pc pc -- pc=[pc] -- jump to following address )
       '(does) literal,
        'comma jump,

sym [ 128 header, label 'left-bracket
        'false call,
        'state call,
        'store jump,

sym ] 0 header, label 'right-bracket
        'true call,
        'state call,
        'store jump,

sym ; 128 header, label 'semicolon
               ( compile ret, )
        -26282 literal, 'comma call, ( 5699 -> add four r r -- r=r+4 )
        -27871 literal, 'comma call, ( 2193 -> ld+ zero r t -- t=[r] )
         13142 literal, 'comma call, ( 5633 -> add four t t -- t=t+4 )
         12353 literal, 'comma call, ( 4130 -> cp? zero t pc -- pc=t )
 'left-bracket jump,

sym bye 0 header,
             0 halt,

sym quit 0 header, label 'quit ( TODO: empty return stack )
       'header call,    ( addr len )
         'over call,    ( addr len addr )
    'one-minus call,    ( addr len c-addr )
         'find call,    ( addr len [ c-addr 0 | xt 1 | xt -1 ] )
          'dup call,    ( addr len [ c-addr 0 | xt 1 | xt -1 ] [ 0 | 1 | -1 ] )
             0 literal, ( addr len [ c-addr 0 | xt 1 | xt -1 ] [ 0 | 1 | -1 ] 0 )
       'equals call,    ( addr len [ c-addr 0 | xt 1 | xt -1 ] [ 0 | 1 | -1 ] bool )
( 0 )          if,      ( addr len c-addr 0 -- not found )
        '2drop call,    ( addr len )
         '2dup call,    ( addr len addr len )
 'parse-number call,    ( addr len [ num true | false ] )
( 1 )          if,      ( addr len -- is number )
         '-rot call,    ( num addr len )
        '2drop call,    ( num )
        'state call,
        'fetch call,    ( num state )
( 2 )          if,      ( num )
          3106 literal, 'comma call, ( 220C -> ld+ two pc x -- x=[pc] pc += 2 -- load and skip literal )
        'comma call,    ( literal value )
        -14283 literal, 'comma call, ( 35C8 -> st+ -four x d -- [d]=x d+=-4 -- push literal )
( 2 )          then,
( 1 )          else,    ( addr len -- not number, error )
         'type call,    ( )
           '?' literal, ( '?' )
         'emit call,    ( )
( 1 )          then,
( 0 )          else,    ( addr len xt [ 1 | -1 ] -- found )
            -1 literal,
       'equals call,    ( addr len xt [ 0 | -1 ] )
        'state call,
        'fetch call,    ( addr len xt [ 0 | -1 ] [ 0 | non-zero ] )
             0 literal,
       'equals call,
          'not call,    ( addr len xt [ 0 | -1 ] [ 0 | -1 ] )
          'and call,    ( addr len xt [ 0 | -1 ] )
               if,      ( addr len xt -- compile? )
              ( compile call ) 
          2357 literal, 'comma call, ( 3509 -> st+ -four pc r -- [r]=pc r+=4 -- push pc )
            33 literal, 'comma call, ( 2100 -> ld+ zero pc pc -- pc=[pc] -- jump to following address )
        'comma call,    ( addr len )
        '2drop call,    ( )
               else,    ( addr len xt -- immediate/not-compiling )
         '-rot call,    ( xt addr len )
        '2drop call,    ( xt )
      'execute call,    ( )
               then,
( 0 )          then,

32767 t lit,
one t zero write,

         'quit jump, ( TODO: recurse )
   
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

             0 literal,
        'state call, ( initialize state )
        'store call,

10 literal, 'base call, 'store call,

32767 t lit,
one t zero write,

'quit jump,

assemble
