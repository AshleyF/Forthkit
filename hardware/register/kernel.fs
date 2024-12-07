require assembler.fs

    2 constant one       1    one ldc, \ literal 1
    3 constant two       2    two ldc, \ literal 2
    4 constant four      4   four ldc, \ literal 4
    5 constant eight     8  eight ldc, \ literal 8
    6 constant twelve   12 twelve ldc, \ literal 12
    
    7 constant #t       -1     #t ldc, \ literal true (-1)
 zero constant #f                      \ literal false (0)
 
    8 constant x
    9 constant y
   10 constant z
   11 constant w

: ldv, ( val reg -- ) pc two ld+, , ;

( --- stacks ----------------------------------------------------------------- )

: push, ( reg ptr -- ) dup dup four sub,  st, ;
: pop,  ( reg ptr -- ) four ld+, ;

12 constant d  memory-size 2 + d ldv, \ data stack pointer

: pushd, ( reg -- ) d push, ;
: popd,  ( reg -- ) d pop, ;

: literal, x ldv,  x pushd, ;

13 constant r  memory-size r ldv, \ return stack pointer

: pushr, ( reg -- ) r push, ;
: popr,  ( reg -- ) r pop, ;

: call, ( addr -- ) pc pushr,  jump, ;    \ 6 bytes
: ret, x popr,  x x four add,  pc x cp, ; \ 6 bytes (pc popr, would complicate calls)

( --- primitive control-flow ------------------------------------------------- )

: 0branch, ( -- dest ) x popd,  0 y ldv,  here 2 -  pc y x cp?, ; \ dummy jump if 0 to address, push pointer to patch

\ ... if ... then
\ ... if ... else ... then
: if, ( C: -- orig ) 0branch, ; \ dummy branch on 0, push pointer to address
: then, ( orig -- ) patch, ; \ patch if/else to continue here
: else, ( C: orig1 -- orig2 ) branch, swap then, ;  \ patch previous branch to here, dummy unconditionally branch over false block

\ begin ... again
\ begin ... until
\ begin ... while ... repeat
: begin, ( C: -- dest ) here ; \ begin loop
: again, ( C: dest -- ) jump, ; \ jump back to beginning
: until, ( C: dest -- ) 0branch, s! ; \ branch on 0 to address
: while, ( C: dest -- orig dest ) 0branch, swap ; \ continue while condition met (0= if), 
: repeat, ( C: orig dest -- ) again, here swap s! ; \ jump back to beginning, patch while to here

( --- dictionary ------------------------------------------------------------- )

false warnings ! \ intentionally redefining (latest header,)

variable latest  0 latest !

: header, ( flag "<spaces>name" -- )
  latest @ \ link to current (soon to be previous) word
  here latest ! \ update latest to this word
  memory - , \ append link, relative to memory
  parse-name \ ( flag addr len -- )
  rot over or c, \ append flag/len
  over + swap \ ( end start -- )
  do i c@ c, loop ; \ append name

true warnings ! \ intentionally redefining (latest, header,)

( --- primitives ------------------------------------------------------------- )

               branch, \ skip dictionary

\ (bye) ( code -- ) halt machine with return code (non-standard)
0 header, bye  label '(bye)
                 x popd,
                 x halt,

\ bye ( -- ) halt machine
0 header, bye  label 'bye
              zero pushd,
            '(bye) jump,

\ @ ( addr -- ) fetch 16-bit value
0 header, @  label 'fetch
                 x popd,
               x x ld,
                 x pushd,
                   ret,

\ ! ( val addr -- ) store 16-bit value
0 header, !  label 'store
                 x popd,
                 y popd,
               y x st,
                   ret,

\ +! ( val addr -- ) store 16-bit value
0 header, +!  label 'plus-store
                 x popd,
                 y popd,
               z x ld,
             z z y add,
               z x st,
                   ret,

\ c@ ( addr -- ) fetch 8-bit value
0 header, c@  label 'c-fetch
                 x popd,
               x x ld,
             $ff y ldv, 
             x x y and,
                 x pushd,
                   ret,

\ c! ( val addr -- ) store 8-bit value
0 header, c!  label 'c-store
                 x popd,
                 y popd,
             $ff z ldv, 
             y y z and,  \ mask to lower
           z z not,  \ upper mask
               w x ld,   \ existing value
             w w z and,  \ mask to upper
             y y w or,   \ combine
               y x st,
                   ret,

\ + ( y x -- sum ) addition
0 header, +  label 'plus
                 x popd,
                 y popd,
             x y x add,
                 x pushd,
                   ret,

\ 1+ ( x -- inc ) increment (1 +)
0 header, 1+  label 'one-plus
                 x popd,
           x x one add,
                 x pushd,
                   ret,

\ 1- ( x -- dec ) decrement (1 -)
0 header, 1-  label 'one-minus
                 x popd,
           x x one sub,
                 x pushd,
                   ret,

\ - ( y x -- differece ) subtraction
0 header, -  label 'minus
                 x popd,
                 y popd,
             x y x sub,
                 x pushd,
                   ret,

\ * ( y x -- product ) subtraction
0 header, *  label 'star
                 x popd,
                 y popd,
             x y x mul,
                 x pushd,
                   ret,

\ / ( y x -- quotient ) division
0 header, /  label 'slash
                 x popd,
                 y popd,
             x y x div,
                 x pushd,
                   ret,

\ mod ( y x -- remainder ) remainder of division
0 header, mod  label 'mod
                 x popd,
                 y popd,
             z y x div,
             z z x mul,
             z y z sub,
                 z pushd,
                   ret,

\ /mod ( y x -- remainder quotient ) remainder and quotient result of division
0 header, /mod  label 'slash-mod
                 x popd,
                 y popd,
             z y x div,
             w z x mul,
             w y w sub,
                 w pushd,
                 z pushd,
                   ret,

\ nand ( y x -- not-and ) not and (non-standard)
0 header, nand  label 'nand
                 x popd,
                 y popd,
             x y x nand,
                 x pushd,
                   ret,

\ invert ( x -- result ) invert bits
0 header, invert  label 'invert
                x d ld,
                x x not,
                x d st,
                    ret,

\ negate ( x -- result ) arithetic inverse (invert 1+) (0 swap -)
0 header, negate  label 'negate
               x d ld,
               x x not,
           x x one add,
               x d st,
                   ret,

\ and ( y x -- result ) logical/bitwise and
0 header, and  label 'and
                 x popd,
                 y popd,
             z x y and,
                 z pushd,
                   ret,

\ or ( y x -- result ) logical/bitwise or
0 header, or  label 'or
                 x popd,
                 y popd,
             z x y or,
                 z pushd,
                   ret,

\ lshift ( y x -- result ) left shift
0 header, lshift  label 'lshift
                 x popd,
                 y popd,
             x y x shl,
                 x pushd,
                   ret,

\ 2* ( x -- result ) multiply by 2 (1 lshift)
0 header, 2*  label 'two-star
                 x popd,
           x x one shl,
                 x pushd,
                   ret,

\ rshift ( y x -- result ) right shift
0 header, rshift  label 'rshift
                 x popd,
                 y popd,
             x y x shr,
                 x pushd,
                   ret,

\ 2/ ( x -- result ) divide by 2 (1 rshift)
0 header, 2/  label 'two-slash
                 x popd,
           x x one shr,
                 x pushd,
                   ret,

\ key ( -- char ) read from console
0 header, key  label 'key
                 x in,
                 x pushd,
                   ret,

\ emit ( char -- ) write to console
0 header, emit  label 'emit
                 x popd,
                 x out,
                   ret,

\ read-block ( file size addr -- ) block file of size -> address
0 header, read-block  label 'read-block
                 x popd,
                 y popd,
                 z popd,
             z y x read,
                   ret,

\ write-block ( file size addr -- ) block file of size -> address
0 header, write-block  label 'write-block
                 x popd,
                 y popd,
                 z popd,
             z y x write,
                   ret,

\ drop ( x -- ) remove top stack value
0 header, drop  label 'drop
          d d four add,
                   ret,

\ 2drop ( y x -- ) remove top two stack values
0 header, 2drop  label 'two-drop
         d d eight add,
                   ret,

\ dup ( x -- x x ) duplicate top stack value
0 header, dup  label 'dupe
               x d ld,
                 x pushd,
                   ret,

\ 2dup ( y x -- y x y x ) duplicate top two stack values
0 header, 2dup  label 'two-dupe
               x d ld,
          y d four add,
               y y ld,
                 y pushd,
                 x pushd,
                   ret,

\ ?dup ( x -- 0 | x x ) duplicate top stack value if non-zero
0 header, ?dup  label 'question-dupe
               x d ld,
                 y popr,  \ return address
          y y four add,
            pc y x cp?,   \ return if x=0
                 x pushd, \ else dup
              pc y cp,    \ return

\ nip ( y x -- x ) drop second stack value
0 header, nip  label 'nip
                 x popd,
               x d st,
                   ret,

\ over ( y x -- y x y ) copy second stack value to top
0 header, over  label 'over
          x d four add,
               x x ld,
                 x pushd,
                   ret,

\ 2over ( w z y x -- w z y x w z ) copy second pair of stack values to top
0 header, 2over  label 'two-over
        x d twelve add,
               x x ld,
                 x pushd,
        x d twelve add,
               x x ld,
                 x pushd,
                   ret,

\ swap ( y x -- x y ) swap top two stack values
0 header, swap  label 'swap
               x d ld,
          z d four add,
               y z ld,
               y d st, \ swap in-place
               x z st,
                   ret,

\ 2swap ( w z y x -- y x w z ) swap top two pairs of stack values
0 header, 2swap  label 'two-swap
               x d ld,
         y d eight add,
               z y ld,
               z d st, \ swap z<->x in-place
               x y st,
          x d four add,
               y x ld,
        z d twelve add,
               w z ld,
               w x st, \ swap w<->y in-place
               y z st,
                   ret,

\ tuck ( y x -- x y x ) copy top stack value under second value
0 header, tuck  label 'tuck
               x d ld,
          z d four add,
               y z ld,
               y d st, \ swap in-place
               x z st,
                 x pushd,
                   ret,

\ rot ( z y x -- y x z ) rotate top three stack values
0 header, rot  label 'rot
                 x popd,
                 y popd,
                 z popd,
                 y pushd,
                 x pushd,
                 z pushd,
                   ret,

\ -rot ( z y x -- x z y ) reverse rotate top three stack values (non-standard - rot rot)
0 header, -rot  label '-rot
                 x popd,
                 y popd,
                 z popd,
                 x pushd,
                 z pushd,
                 y pushd,
                   ret,

\ pick ( n...x -- n...x n ) copy 0-based nth stack value to top
0 header, pick  label 'pick
               x d ld,
          x x four mul,
          x x four add,
             x x d add,
               x x ld,
               x d st,
                   ret,

\ depth ( -- depth ) data stack depth
0 header, depth  label 'depth
 memory-size 2 + x ldv,
             x x d sub,
          x x four div,
                 x pushd,
                   ret,

\ >r ( x -- ) ( R: x -- ) move x to return stack
0 header, >r  label 'to-r
                 x popd,
               y r ld,  \ this return address
               x r st,  \ replace
          y y four add, \ ret,
              pc y cp,

\ 2>r ( y x -- ) ( R: -- y x ) move y x pair to return stack
0 header, 2>r  label 'two-to-r
                 x popd,
                 y popd,
               z r ld,  \ this return address
               y r st,  \ push y in-place
                 x pushr,
          z z four add, \ ret,
              pc z cp,

\ r> ( -- x ) ( R: x -- ) move x from return stack
0 header, r>  label 'r-from
                 z popr, \ this return address
                 x popr, \ top value before call
                 x pushd,
          z z four add,  \ ret,
              pc z cp,

\ 2r> ( -- y x ) ( R: y x -- ) move x from return stack
0 header, 2r>  label 'two-r-from
                 z popr, \ this return address
                 x popr, \ top value before call
                 y popr, \ second value before call
                 y pushd,
                 x pushd,
          z z four add,  \ ret,
              pc z cp,

\ r@ ( -- x ) ( R: x -- x ) copy x from return stack
0 header, r@  label 'r-fetch
                 z popr, \ this return address
               x r ld,   \ top value before call
                 x pushd,
          z z four add,  \ ret,
              pc z cp,

\ 2r@ ( -- y x ) ( R: y x -- y x ) copy y x pair from return stack
0 header, 2r@  label 'two-r-fetch
                 z popr, \ this return address
               x r ld,   \ top value before call
          y r four add,
               y y ld,
                 y pushd,
                 x pushd,
          z z four add,  \ ret,
              pc z cp,

\ = ( y x -- b ) true if equal
0 header, =  label 'equals
                 x popd,
               y d ld,
             z y x sub, \ zero if equal
              y #f cp,
            y #t z cp?,
               y d st,
                   ret,

\ <> ( y x -- b ) true if not equal
0 header, <>  label 'not-equals
                 x popd,
               y d ld,
             z y x sub, \ zero if equal
              y #t cp,
            y #f z cp?,
               y d st,
                   ret,

\ 0= ( y x -- b ) true if equal to zero
0 header, 0=  label 'zero-equals
               x d ld,
              y #f cp,
            y #t x cp?,
               y d st,
                   ret,

\ 0<> ( y x -- b ) true if not equal to zero
0 header, 0=  label 'zero-not-equals
               x d ld,
              y #t cp,
            y #f x cp?,
               y d st,
                   ret,

\ 0< ( x -- b ) true if x less than zero (15 rshift negate 1+)
0 header, 0<  label 'zero-less
               x d ld,
              15 y ldc,
             x x y shr, \ sign bit to 1s place
               x x not, \ negate
           x x one add,
               x d st,
                   ret,

\ < ( y x -- b ) true if y less than x (- 0<)
0 header, <  label 'less-than
                 x popd,
               y d ld,
             x y x sub, \ negative if y less than x
              15 y ldc,
             x x y shr, \ sign bit to 1s place
               x x not, \ negate
           x x one add,
               x d st,
                   ret,

\ 0> ( x -- b ) true if x greater than zero (1- 15 rshift 1-)
0 header, 0>  label 'zero-greater
               x d ld,
           x x one sub, \ negative if not greater than zero
              15 y ldc,
             x x y shr, \ sign bit to 1s place
               x x not, \ negate
           x x one add,
               x x not, \ invert
               x d st,
                   ret,

\ > ( y x -- b ) true if y greater than x (- 0>)
0 header, >  label 'greater-than
                 x popd,
               y d ld,
             x y x sub, \ negative if y less than x
           x x one sub, \ negative if y is equal to x
              15 y ldc,
             x x y shr, \ sign bit to 1s place
               x x not, \ negate
           x x one add,
               x x not, \ invert
               x d st,
                   ret,

\ exit ( -- ) ( R: addr -- ) return from call
0 header, exit  label 'exit
                 x popr, \ discard this call
                   ret,

( --- memory ----------------------------------------------------------------- )

\ align ( -- ) reserve space to align data space pointer (no-op on RM16)
0 header, align  label 'align
                   ret, \ no-op

\ aligned ( addr -- addr ) align address (no-op on RM16)
0 header, aligned  label 'aligned
                   ret, \ no-op

\ cells ( x -- n-cells ) size in address units of n-cells (2*)
0 header, cells  label 'cells
         'two-star jump,

\ cell+ ( addr -- addr ) add size of cell to address (2 +)
0 header, cell+  label 'cell-plus
                 2 literal,
             'plus jump,

\ chars ( x -- n-chars ) size in address units of n-chars (no-op)
0 header, chars  label 'chars
                   ret, \ no-op

\ char+ ( addr -- addr ) add size of char to address (1+)
0 header, char+  label 'char-plus
         'one-plus jump,

: var,
                   label
              x pc cp,
              14 y ldc,     \ count this and following instructions
             x x y add,     \ point just beyond this code -- data field
                 x pushd,   \ 4 bytes
                   ret,     \ 6 bytes
                   2 h +! ; \ 2 allot

\ dp ( -- addr ) return address of dictionary pointer (variable h) (non-standard, common internal)
var, 'dp \ initialized after dictionary (below)

\ here ( -- addr ) current dictionary pointer address (dp @)
0 header, here  label 'here
               'dp call,
            'fetch jump,

\ unused ( -- remaining ) dictionary space remaining
0 header, unused  label 'unused
       memory-size literal,
             'here call,
            'minus jump, \ TODO: consider stack space?

\ allot ( n -- ) advance dictionary pointer (dp +!)
0 header, allot  label 'allot
               'dp call,
       'plus-store jump,

\ , ( x -- ) append x in newly reserved cell (here ! 1 cells allot)
0 header, ,  label 'comma
             'here call,
            'store call,
                 1 literal,
            'cells call,
            'allot jump,

\ c, ( x -- ) append x chars in newly reserved space (here c! 1 chars allot)
0 header, c,  label 'c-comma
             'here call,
          'c-store call,
                 1 literal,
            'chars call,
            'allot jump,

( --- secondaries ------------------------------------------------------------ )

\ 2! ( y x addr -- ) store x y at consecutive addresses (swap over ! cell+ !)
0 header, 2!  label 'two-store
             'swap call,
             'over call,
            'store call,
        'cell-plus call,
            'store jump,

\ 2@ ( addr -- y x ) fetch pair of consecutive addresses (dup cell+ @ swap @)
0 header, 2@  label 'two-fetch
             'dupe call,
        'cell-plus call,
            'fetch call,
             'swap call,
            'fetch jump,

\ xor ( y x -- result ) logical/bitwise exclusive or (2dup or -rot and invert and)
0 header, xor  label 'xor
         'two-dupe call,
               'or call,
             '-rot call,
              'and call,
           'invert call,
              'and jump,

\ abs ( x -- |x| ) absolute value (dup 0< if negate then)
0 header, abs  label 'abs
             'dupe call,
        'zero-less call,
                   if,
           'negate call,
                   then,
                   ret,

\ min ( y x -- min ) lessor value (2dup < if drop exit then nip)
0 header, min  label 'min
         'two-dupe call,
        'less-than call,
                   if,
             'drop call,
             'exit call,
                   then,
              'nip jump,

\ max ( y x -- max ) greater value (2dup < if nip exit then drop)
0 header, max  label 'max
         'two-dupe call,
        'less-than call,
                   if,
              'nip call,
             'exit call,
                   then,
             'drop jump,

( --- secondary control-flow ------------------------------------------------- )

\ <limit> <start> do ... loop
\ <limit> <start> do ... <n> +loop
\ <limit> <start> do ... unloop exit ... loop
\ <limit> <start> do ... if ... leave then ... loop
: do, ( limit start -- ) ( C: -- false addr ) \ begin do-loop (immediate 2>r begin false)
         'two-to-r call,
                   false \ no addresses to patch (initially)
                   begin, ;

: ?do, ( limit start -- ) ( C: -- false addr true addr )
         'two-dupe call,
       'not-equals call,
                   false \ terminator for patching
                   if,
                   true  \ patch if to loop
         'two-to-r call,
                   begin, ;

: leave, ( C: -- addr true )
                   branch,
                   -rot true -rot ; \ patch to loop (swap under if address)

: +loop, ( n -- ) ( C: ... flag addr -- ) \ end do-loop, add n to loop counter (immediate r> + r@ over >r < if again then 2r> 2drop)
           'r-from call,
             'plus call,
          'r-fetch call,
             'over call,
             'to-r call,
        'less-than call,
                   if,
                   swap again,
                   then,
                   begin while
                   patch,
                   repeat
       'two-r-from call,
         'two-drop call, ;

: loop, ( C: addr -- )
                 1 literal,
                   +loop, ; \ end do-loop (immediate 1 +loop)

\ i ( -- x ) ( R: x -- x ) copy innermost loop index (2r@ drop)
0 header, i  label 'i-index
      'two-r-fetch call, \ including return from here
             'drop jump,

\ j ( -- x ) ( R: x -- x ) copy next outer loop index (2r> 2r@ drop -rot 2>r) (x r twelve add, x x ld, x pushd, ret,)
0 header, j  label 'j-index
       'two-r-from call, \ i this
      'two-r-fetch call, \ i this j limit
             'drop call, \ i this j
             '-rot call, \ j i this
         'two-to-r call, \ tricky, don't jump here!
                   ret,

\ unloop ( -- ) ( R: y x -- ) remove loop parameters (2r> 2drop)
0 header, unloop  label 'unloop
       'two-r-from call,
         'two-drop jump,

( --- interpreter ------------------------------------------------------------ )

\ true ( -- true ) return true flag (-1 constant true)
0 header, true  label 'true
                -1 literal,
                   ret,

\ false ( -- false ) return false flag (0 constant false)
0 header, false  label 'false
                 0 literal,
                   ret,

\ bl ( -- c ) space character value (32 constant bl)
0 header, bl  label 'bl
                32 literal,
                   ret,

\ space ( -- ) emit space character (bl emit)
0 header, space  label 'space
               'bl call,
             'emit jump,

\ cr ( -- ) cause newline (10 emit)
0 header, cr  label 'c-r
                10 literal,
             'emit jump,

\ type ( addr len -- ) display the character string (0 max 0 ?do dup c@ emit char+ loop drop)
0 header, type  label 'type
                 0 literal,
              'max call,
                 0 literal,
                   ?do,
             'dupe call,
          'c-fetch call,
             'emit call,
        'char-plus call,
                   loop,
             'drop jump,

\ pad ( -- addr ) address of transient region for intermediate processing
0 header, pad  label 'pad
             'here call,
              1024 literal, \ arbitrary distance away
             'plus jump,

\ np ( -- addr ) return address of pictured numeric output pointer (non-standard)
var, 'np \ initialized after dictionary (below)

\ <# ( -- ) initialize pictured numeric output (pad 64 + np !)
0 header, hold  label 'num-start
              'pad call,
                64 literal, \ arbitrary distance away
             'plus call,
               'np call,
            'store jump,

\ hold ( char -- ) add char to beginning of pictured numeric output (np -1 over +! c!)
0 header, hold  label 'hold
               'np call,
                -1 literal,
             'over call,
       'plus-store call,
            'fetch call,
          'c-store jump,

\ holds ( addr len -- ) add string to beginning of pictured numeric output (begin dup while 1- 2dup + c@ hold repeat 2drop)
0 header, holds  label 'holds
                   begin,
             'dupe call,
                   while,
        'one-minus call,
         'two-dupe call,
             'plus call,
          'c-fetch call,
             'hold call,
                   repeat,
         'two-drop jump,

\ base ( -- base ) address of current number-conversion radix (2..36, initially 10)
0 header, base  var, 'base

\ decimal ( -- ) set number-conversion radix to 10
0 header, decimal  label 'decimal
                10 literal,
             'base call,
            'store jump,

\ hex ( -- ) set number-conversion radix to 16
0 header, hex  label 'hex
                16 literal,
             'base call,
            'store jump,

\ octal ( -- ) set number-conversion radix to 8 (non-standard)
0 header, octal  label 'octal
                 8 literal,
             'base call,
            'store jump,

\ binary ( -- ) set number-conversion radix to 2 (non-standard)
0 header, binary  label 'binary
                 2 literal,
             'base call,
            'store jump,

\ # ( ud -- ud ) prepend least significant digit to pictured numeric output, return ud/base
0 header, #  label 'num
             'swap call, \ TODO: support double numbers
             'base call,
            'fetch call,
         'two-dupe call,
              'mod call,
                48 literal, \ '0'
             'plus call,
             'hold call, \ 0 n b
            'slash call, \ 0 m
             'swap jump, \ TODO: support double numbers

\ #s ( ud -- ud ) convert all digits using # (appends at least 0)
0 header, #s  label 'num-s
             'swap call, \ TODO: support double numbers
                   begin,
             'swap call, \ TODO: support double numbers
              'num call, \ note: at least once even if zero
             'swap call, \ TODO: support double numbers
             'dupe call,
  'zero-not-equals call,
                   while,
                   repeat,
             'swap jump, \ TODO: support double numbers

\ sign ( n -- ) if negative, prepend '-' to pictured numeric output
0 header, sign  label 'sign
        'zero-less call,
                   if,
                45 literal, \ '-'
             'hold call,
                   then,
                   ret,

\ #> ( xd -- addr len ) make pictured numeric output string available (np @ pad 64 + over -)
0 header, #>  label 'num-end \ greater
         'two-drop call,
               'np call,
            'fetch call,
              'pad call,
                64 literal, \ arbitrary distance away
             'plus call,
             'over call,
            'minus jump,

\ . ( n -- ) display value in free field format (dup abs 0 <# #s rot sign #> type space)
0 header, .  label 'dot
             'dupe call,
              'abs call,
                 0 literal,
        'num-start call,
            'num-s call,
              'rot call,
             'sign call,
          'num-end call,
             'type call,
            'space jump,

\ u. ( u -- ) display unsigned value in free field format (0 <# #s #> type space)
0 header, u.  label 'u-dot
                 0 literal,
        'num-start call,
            'num-s call,
          'num-end call,
             'type call,
            'space jump,

\ .s ( -- ) display values on the stack non-destructively (depth dup 0 ?do dup i - pick . loop drop)
0 header, .s  label 'dot-s
            'depth call,
             'dupe call,
                 0 literal,
                   ?do,
             'dupe call,
          'i-index call,
            'minus call,
             'pick call,
              'dot call,
                   loop,
             'drop call,
                   ret,

\ ? ( addr -- ) display value stored at address (@ .)
0 header, ?  label 'question
            'fetch call,
              'dot jump,

( --- end of dictionary ------------------------------------------------------ )

                   patch,
              here literal, \ update dictionary pointer to compile-time position
               'dp call,
            'store call,
          'decimal call, \ default base
              zero halt,