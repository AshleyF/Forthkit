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

12 constant d \ data stack pointer (initialized after dictionary below)

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
\ begin ... while ... repeat  (note: not begin ... while ... again!)
: begin, ( C: -- dest ) here ; \ begin loop
: again, ( C: dest -- ) jump, ; \ jump back to beginning
: until, ( C: dest -- ) 0branch, s! ; \ branch on 0 to address
: while, ( C: dest -- orig dest ) 0branch, swap ; \ continue while condition met (0= if), 
: repeat, ( C: orig dest -- ) again, here swap s! ; \ jump back to beginning, patch while to here

( --- dictionary ------------------------------------------------------------- )

false warnings ! \ intentionally redefining (latest header, ')

variable latest  0 latest !

: header, ( flag "<spaces>name" -- )
  latest @          \ link to current (soon to be previous) word
  here latest !     \ update latest to this word
  ,                 \ append link
  parse-name        \ ( flag addr len -- )
  rot over or c,    \ append flag/len
  over + swap       \ ( end start -- )
  do i c@ c, loop ; \ append name

: '
  bl word latest @ \ ( find link -- )
  begin
    2dup memory + 3 + dup 1- c@ $7f and \ get name/len
    rot count 2over compare 0= if + memory - -rot 2drop exit then \ return xt if names match
    2drop s@ dup 0= if ." Word not found: " drop count type cr exit then \ follow link, until end
  again ;

: ['] ' postpone literal ; immediate

true warnings ! \ intentionally redefining (latest, header, ')

( --- primitives ------------------------------------------------------------- )

               branch, \ skip dictionary

\ (clear-data) empty return stack
0 header, (clear-data)
 memory-size 2 + d ldv,
                   ret,

\ (clear-return) empty return stack
0 header, (clear-return)
                 x popr,
     memory-size r ldv,
          x x four add, \ ret,
              pc x cp,

\ execute ( i * x xt -- j * x ) perform the semantics identified by xt
0 header, execute
                 x popd,
              pc x cp,

\ (bye) ( code -- ) halt machine with return code (non-standard)
0 header, (bye)
                 x popd,
                 x halt,

\ bye ( -- ) halt machine
0 header, bye
              zero pushd,
           ' (bye) jump,

\ @ ( addr -- ) fetch 16-bit value
0 header, @
                 x popd,
               x x ld,
                 x pushd,
                   ret,

\ ! ( val addr -- ) store 16-bit value
0 header, !
                 x popd,
                 y popd,
               y x st,
                   ret,

\ +! ( val addr -- ) store 16-bit value (dup @ rot + swap !)
0 header, +!
                 x popd,
                 y popd,
               z x ld,
             z z y add,
               z x st,
                   ret,

\ c@ ( addr -- ) fetch 8-bit value
0 header, c@
                 x popd,
               x x ld,
             $ff y ldv, 
             x x y and,
                 x pushd,
                   ret,

\ c! ( val addr -- ) store 8-bit value
0 header, c!
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
0 header, +
                 x popd,
                 y popd,
             x y x add,
                 x pushd,
                   ret,

\ 1+ ( x -- inc ) increment (1 +)
0 header, 1+
                 x popd,
           x x one add,
                 x pushd,
                   ret,

\ 1- ( x -- dec ) decrement (1 -)
0 header, 1-
                 x popd,
           x x one sub,
                 x pushd,
                   ret,

\ - ( y x -- differece ) subtraction
0 header, -
                 x popd,
                 y popd,
             x y x sub,
                 x pushd,
                   ret,

\ * ( y x -- product ) subtraction
0 header, *
                 x popd,
                 y popd,
             x y x mul,
                 x pushd,
                   ret,

\ / ( y x -- quotient ) division
0 header, /
                 x popd,
                 y popd,
             x y x div,
                 x pushd,
                   ret,

\ mod ( y x -- remainder ) remainder of division
0 header, mod
                 x popd,
                 y popd,
             z y x div,
             z z x mul,
             z y z sub,
                 z pushd,
                   ret,

\ /mod ( y x -- remainder quotient ) remainder and quotient result of division
0 header, /mod
                 x popd,
                 y popd,
             z y x div,
             w z x mul,
             w y w sub,
                 w pushd,
                 z pushd,
                   ret,

\ nand ( y x -- not-and ) not and (non-standard)
0 header, nand
                 x popd,
                 y popd,
             x y x nand,
                 x pushd,
                   ret,

\ invert ( x -- result ) invert bits
0 header, invert
                x d ld,
                x x not,
                x d st,
                    ret,

\ negate ( x -- result ) arithetic inverse (invert 1+) (0 swap -)
0 header, negate
               x d ld,
               x x not,
           x x one add,
               x d st,
                   ret,

\ and ( y x -- result ) logical/bitwise and
0 header, and
                 x popd,
                 y popd,
             z x y and,
                 z pushd,
                   ret,

\ or ( y x -- result ) logical/bitwise or
0 header, or
                 x popd,
                 y popd,
             z x y or,
                 z pushd,
                   ret,

\ lshift ( y x -- result ) left shift
0 header, lshift
                 x popd,
                 y popd,
             x y x shl,
                 x pushd,
                   ret,

\ 2* ( x -- result ) multiply by 2 (1 lshift)
0 header, 2*
                 x popd,
           x x one shl,
                 x pushd,
                   ret,

\ rshift ( y x -- result ) right shift
0 header, rshift
                 x popd,
                 y popd,
             x y x shr,
                 x pushd,
                   ret,

\ 2/ ( x -- result ) divide by 2 (1 rshift)
0 header, 2/
                 x popd,
           x x one shr,
                 x pushd,
                   ret,

\ key ( -- char ) read from console
0 header, key
                 x in,
                 x pushd,
                   ret,

\ emit ( char -- ) write to console
0 header, emit
                 x popd,
                 x out,
                   ret,

\ read-block ( file size addr -- ) block file of size -> address
0 header, read-block
                 x popd,
                 y popd,
                 z popd,
             z y x read,
                   ret,

\ write-block ( file size addr -- ) block file of size -> address
0 header, write-block
                 x popd,
                 y popd,
                 z popd,
             z y x write,
                   ret,

\ drop ( x -- ) remove top stack value
0 header, drop
          d d four add,
                   ret,

\ 2drop ( y x -- ) remove top two stack values
0 header, 2drop
         d d eight add,
                   ret,

\ dup ( x -- x x ) duplicate top stack value
0 header, dup
               x d ld,
                 x pushd,
                   ret,

\ 2dup ( y x -- y x y x ) duplicate top two stack values
0 header, 2dup
               x d ld,
          y d four add,
               y y ld,
                 y pushd,
                 x pushd,
                   ret,

\ ?dup ( x -- 0 | x x ) duplicate top stack value if non-zero
0 header, ?dup
               x d ld,
                 y popr,  \ return address
          y y four add,
            pc y x cp?,   \ return if x=0
                 x pushd, \ else dup
              pc y cp,    \ return

\ nip ( y x -- x ) drop second stack value
0 header, nip
                 x popd,
               x d st,
                   ret,

\ over ( y x -- y x y ) copy second stack value to top
0 header, over
          x d four add,
               x x ld,
                 x pushd,
                   ret,

\ 2over ( w z y x -- w z y x w z ) copy second pair of stack values to top
0 header, 2over
        x d twelve add,
               x x ld,
                 x pushd,
        x d twelve add,
               x x ld,
                 x pushd,
                   ret,

\ swap ( y x -- x y ) swap top two stack values
0 header, swap
               x d ld,
          z d four add,
               y z ld,
               y d st, \ swap in-place
               x z st,
                   ret,

\ 2swap ( w z y x -- y x w z ) swap top two pairs of stack values
0 header, 2swap
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
0 header, tuck
               x d ld,
          z d four add,
               y z ld,
               y d st, \ swap in-place
               x z st,
                 x pushd,
                   ret,

\ rot ( z y x -- y x z ) rotate top three stack values
0 header, rot
                 x popd,
                 y popd,
                 z popd,
                 y pushd,
                 x pushd,
                 z pushd,
                   ret,

\ -rot ( z y x -- x z y ) reverse rotate top three stack values (non-standard - rot rot)
0 header, -rot
                 x popd,
                 y popd,
                 z popd,
                 x pushd,
                 z pushd,
                 y pushd,
                   ret,

\ pick ( n...x -- n...x n ) copy 0-based nth stack value to top
0 header, pick
               x d ld,
          x x four mul,
          x x four add,
             x x d add,
               x x ld,
               x d st,
                   ret,

\ depth ( -- depth ) data stack depth
0 header, depth
 memory-size 2 + x ldv,
             x x d sub,
          x x four div,
                 x pushd,
                   ret,

\ >r ( x -- ) ( R: x -- ) move x to return stack
0 header, >r
                 x popd,
               y r ld,  \ this return address
               x r st,  \ replace
          y y four add, \ ret,
              pc y cp,

\ 2>r ( y x -- ) ( R: -- y x ) move y x pair to return stack
0 header, 2>r
                 x popd,
                 y popd,
               z r ld,  \ this return address
               y r st,  \ push y in-place
                 x pushr,
          z z four add, \ ret,
              pc z cp,

\ r> ( -- x ) ( R: x -- ) move x from return stack
0 header, r>
                 z popr, \ this return address
                 x popr, \ top value before call
                 x pushd,
          z z four add,  \ ret,
              pc z cp,

\ 2r> ( -- y x ) ( R: y x -- ) move x from return stack
0 header, 2r>
                 z popr, \ this return address
                 x popr, \ top value before call
                 y popr, \ second value before call
                 y pushd,
                 x pushd,
          z z four add,  \ ret,
              pc z cp,

\ r@ ( -- x ) ( R: x -- x ) copy x from return stack
0 header, r@
                 z popr, \ this return address
               x r ld,   \ top value before call
                 x pushd,
          z z four add,  \ ret,
              pc z cp,

\ 2r@ ( -- y x ) ( R: y x -- y x ) copy y x pair from return stack
0 header, 2r@
                 z popr, \ this return address
               x r ld,   \ top value before call
          y r four add,
               y y ld,
                 y pushd,
                 x pushd,
          z z four add,  \ ret,
              pc z cp,

\ = ( y x -- b ) true if equal
0 header, =
                 x popd,
               y d ld,
             z y x sub, \ zero if equal
              y #f cp,
            y #t z cp?,
               y d st,
                   ret,

\ <> ( y x -- b ) true if not equal
0 header, <>
                 x popd,
               y d ld,
             z y x sub, \ zero if equal
              y #t cp,
            y #f z cp?,
               y d st,
                   ret,

\ 0= ( y x -- b ) true if equal to zero
0 header, 0=
               x d ld,
              y #f cp,
            y #t x cp?,
               y d st,
                   ret,

\ 0<> ( y x -- b ) true if not equal to zero
0 header, 0<>
               x d ld,
              y #t cp,
            y #f x cp?,
               y d st,
                   ret,

\ 0< ( x -- b ) true if x less than zero (15 rshift negate 1+)
0 header, 0<
               x d ld,
              15 y ldc,
             x x y shr, \ sign bit to 1s place
               x x not, \ negate
           x x one add,
               x d st,
                   ret,

\ < ( y x -- b ) true if y less than x (- 0<)
0 header, <
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
0 header, 0>
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
0 header, >
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
0 header, exit
                 x popr, \ discard this call
                   ret,

( --- memory ----------------------------------------------------------------- )

\ align ( -- ) reserve space to align data space pointer (no-op on RM16)
0 header, align
                   ret, \ no-op

\ aligned ( addr -- addr ) align address (no-op on RM16)
0 header, aligned
                   ret, \ no-op

\ cells ( x -- n-cells ) size in address units of n-cells (2*)
0 header, cells
              ' 2* jump,

\ cell+ ( addr -- addr ) add size of cell to address (2 +)
0 header, cell+
                 2 literal,
               ' + jump,

\ chars ( x -- n-chars ) size in address units of n-chars (no-op)
0 header, chars
                   ret, \ no-op

\ char+ ( addr -- addr ) add size of char to address (1+)
0 header, char+
              ' 1+ jump,

: var,
                 0 header,
              x pc cp,
              14 y ldc,     \ count this and following instructions
             x x y add,     \ point just beyond this code -- data field
                 x pushd,   \ 4 bytes
                   ret,     \ 6 bytes
                   2 h +! ; \ 2 allot

\ dp ( -- addr ) return address of dictionary pointer (variable h) (non-standard, common internal)
var, dp \ initialized after dictionary (below)

\ here ( -- addr ) current dictionary pointer address (dp @)
0 header, here
              ' dp call,
               ' @ jump,

\ unused ( -- remaining ) dictionary space remaining
0 header, unused
       memory-size literal,
            ' here call,
               ' - jump, \ TODO: consider stack space?

\ allot ( n -- ) advance dictionary pointer (dp +!)
0 header, allot
              ' dp call,
              ' +! jump,

\ , ( x -- ) append x in newly reserved cell (here ! 1 cells allot)
0 header, ,
            ' here call,
               ' ! call,
                 1 literal,
           ' cells call,
           ' allot jump,

\ c, ( x -- ) append x chars in newly reserved space (here c! 1 chars allot)
0 header, c,
            ' here call,
              ' c! call,
                 1 literal,
           ' chars call,
           ' allot jump,

( --- secondaries ------------------------------------------------------------ )

\ 2! ( y x addr -- ) store x y at consecutive addresses (tuck ! cell+ !)
0 header, 2!
            ' tuck call,
               ' ! call,
           ' cell+ call,
               ' ! jump,

\ 2@ ( addr -- y x ) fetch pair of consecutive addresses (dup cell+ @ swap @)
0 header, 2@
             ' dup call,
           ' cell+ call,
               ' @ call,
            ' swap call,
               ' @ jump,

\ xor ( y x -- result ) logical/bitwise exclusive or (2dup or -rot and invert and)
0 header, xor
            ' 2dup call,
              ' or call,
            ' -rot call,
             ' and call,
          ' invert call,
             ' and jump,

\ abs ( x -- |x| ) absolute value (dup 0< if negate then)
0 header, abs
             ' dup call,
              ' 0< call,
                   if,
          ' negate call,
                   then,
                   ret,

\ min ( y x -- min ) lessor value (2dup < if drop exit then nip)
0 header, min
            ' 2dup call,
               ' < call,
                   if,
            ' drop call,
            ' exit call,
                   then,
             ' nip jump,

\ max ( y x -- max ) greater value (2dup < if nip exit then drop)
0 header, max
            ' 2dup call,
               ' < call,
                   if,
             ' nip call,
            ' exit call,
                   then,
            ' drop jump,

( --- secondary control-flow ------------------------------------------------- )

\ <limit> <start> do ... loop
\ <limit> <start> do ... <n> +loop
\ <limit> <start> do ... unloop exit ... loop
\ <limit> <start> do ... if ... leave then ... loop
: do, ( limit start -- ) ( C: -- false addr ) \ begin do-loop (immediate 2>r begin false)
           ['] 2>r call,
                   false \ no addresses to patch (initially)
                   begin, ;

: ?do, ( limit start -- ) ( C: -- false addr true addr )
          ['] 2dup call,
            ['] <> call,
                   false \ terminator for patching
                   if,
                   true  \ patch if to loop
           ['] 2>r call,
                   begin, ;

: leave, ( C: -- addr true )
                   branch,
                   -rot true -rot ; \ patch to loop (swap under if address)

: +loop, ( n -- ) ( C: ... flag addr -- ) \ end do-loop, add n to loop counter (immediate r> + r@ over >r < if again then 2r> 2drop)
             ['] r> call,
              ['] + call,
             ['] r@ call,
           ['] over call,
             ['] >r call,
              ['] < call,
                    if,
                    swap again,
                    then,
                    begin while
                    patch,
                    repeat
            ['] 2r> call,
          ['] 2drop call, ;

: loop, ( C: addr -- )
                 1 literal,
                   +loop, ; \ end do-loop (immediate 1 +loop)

\ i ( -- x ) ( R: x -- x ) copy innermost loop index (2r@ drop)
0 header, i
             ' 2r@ call, \ including return from here
            ' drop jump,

\ j ( -- x ) ( R: x -- x ) copy next outer loop index (2r> 2r@ drop -rot 2>r) (x r twelve add, x x ld, x pushd, ret,)
0 header, j
             ' 2r> call, \ i this
             ' 2r@ call, \ i this j limit
            ' drop call, \ i this j
            ' -rot call, \ j i this
             ' 2>r call, \ tricky, don't jump here!
                   ret,

\ unloop ( -- ) ( R: y x -- ) remove loop parameters (r> 2r> rot >r 2drop)
0 header, unloop
              ' r> call, \ this return address
             ' 2r> call, \ loop parameters
             ' rot call,
              ' >r call, \ replace this return address
           ' 2drop jump,

( --- interpreter ------------------------------------------------------------ )

\ true ( -- true ) return true flag (-1 constant true)
0 header, true
                -1 literal,
                   ret,

\ false ( -- false ) return false flag (0 constant false)
0 header, false
                 0 literal,
                   ret,

\ bl ( -- c ) space character value (32 constant bl)
0 header, bl
                32 literal,
                   ret,

\ space ( -- ) emit space character (bl emit)
0 header, space
              ' bl call,
            ' emit jump,

\ cr ( -- ) cause newline (10 emit)
0 header, cr
                10 literal,
            ' emit jump,

\ type ( addr len -- ) display the character string (0 max 0 ?do dup c@ emit char+ loop drop)
0 header, type
                 0 literal,
             ' max call,
                 0 literal,
                   ?do,
             ' dup call,
              ' c@ call,
            ' emit call,
           ' char+ call,
                   loop,
            ' drop jump,

\ pad ( -- addr ) address of transient region for intermediate processing
0 header, pad
            ' here call,
              1024 literal, \ arbitrary distance away
               ' + jump,

\ np ( -- addr ) return address of pictured numeric output pointer (non-standard)
var, np \ initialized after dictionary (below)

\ <# ( -- ) initialize pictured numeric output (pad 64 + np !)
0 header, <#
             ' pad call,
                64 literal, \ arbitrary distance away
               ' + call,
              ' np call,
               ' ! jump,

\ hold ( char -- ) add char to beginning of pictured numeric output (np -1 over +! c!)
0 header, hold
              ' np call,
                -1 literal,
            ' over call,
              ' +! call,
               ' @ call,
          ' c! jump,

\ holds ( addr len -- ) add string to beginning of pictured numeric output (begin dup while 1- 2dup + c@ hold repeat 2drop)
0 header, holds
                   begin,
             ' dup call,
                   while,
              ' 1- call,
            ' 2dup call,
               ' + call,
              ' c@ call,
            ' hold call,
                   repeat,
           ' 2drop jump,

\ base ( -- base ) address of current number-conversion radix (2..36, initially 10)
var, base

\ decimal ( -- ) set number-conversion radix to 10
0 header, decimal
                10 literal,
            ' base call,
               ' ! jump,

\ hex ( -- ) set number-conversion radix to 16
0 header, hex
                16 literal,
            ' base call,
               ' ! jump,

\ octal ( -- ) set number-conversion radix to 8 (non-standard)
0 header, octal
                 8 literal,
            ' base call,
               ' ! jump,

\ binary ( -- ) set number-conversion radix to 2 (non-standard)
0 header, binary
                 2 literal,
            ' base call,
               ' ! jump,

\ # ( ud -- ud ) prepend least significant digit to pictured numeric output, return ud/base
0 header, #
            ' swap call, \ TODO: support double numbers
            ' base call,
               ' @ call,
            ' 2dup call,
             ' mod call,
                48 literal, \ '0'
               ' + call,
            ' hold call, \ 0 n b
               ' / call, \ 0 m
            ' swap jump, \ TODO: support double numbers

\ #s ( ud -- ud ) convert all digits using # (appends at least 0)
0 header, #s
            ' swap call, \ TODO: support double numbers
                   begin,
            ' swap call, \ TODO: support double numbers
               ' # call, \ note: at least once even if zero
            ' swap call, \ TODO: support double numbers
             ' dup call,
             ' 0<> call,
                   while,
                   repeat,
            ' swap jump, \ TODO: support double numbers

\ sign ( n -- ) if negative, prepend '-' to pictured numeric output
0 header, sign
              ' 0< call,
                   if,
                45 literal, \ '-'
            ' hold call,
                   then,
                   ret,

\ #> ( xd -- addr len ) make pictured numeric output string available (np @ pad 64 + over -)
0 header, #>
           ' 2drop call,
              ' np call,
               ' @ call,
             ' pad call,
                64 literal, \ arbitrary distance away
               ' + call,
            ' over call,
               ' - jump,

\ s>d ( n -- d ) convert number to double-cell
0 header, s>d
                 0 literal,
                   ret,

\ . ( n -- ) display value in free field format (dup abs s>d <# #s rot sign #> type space)
0 header, .
             ' dup call,
             ' abs call,
             ' s>d call,
              ' <# call,
              ' #s call,
             ' rot call,
            ' sign call,
              ' #> call,
           ' space call,
            ' type jump,

\ u. ( u -- ) display unsigned value in free field format (0 <# #s #> type space)
0 header, u.
                 0 literal,
              ' <# call,
              ' #s call,
              ' #> call,
           ' space call,
            ' type jump,

\ .s ( -- ) display values on the stack non-destructively (depth dup 0 do dup i - pick . loop drop)
0 header, .s
           ' depth call,
             ' dup call,
                 0 literal,
                   do, \ TODO: ?do bug
             ' dup call,
               ' i call,
               ' - call,
            ' pick call,
               ' . call,
                   loop,
            ' drop call,
                   ret,

\ ? ( addr -- ) display value stored at address (@ .)
0 header, ?
               ' @ call,
               ' . jump,

( --- interpreter ------------------------------------------------------------ )

\ source ( -- c-addr u )
0 header, source
memory-size $500 - literal, \ $ff bytes above stacks
               $ff literal, \ size ($400 bytes for stacks/$ff elements each)
                   ret,

\ fill ( c-addr u char -- ) if u is greater than zero, store char in each of u consecutive characters of memory beginning at c-addr.
0 header, fill
            ' -rot call,
                 0 literal,
                   ?do,
            ' 2dup call,
              ' c! call,
              ' 1+ call,
                   loop,
           ' 2drop jump,

\ erase ( addr u -- ) if u is greater than zero, clear all bits in each of u consecutive address units of memory beginning at addr.
0 header, erase
                 0 literal,
            ' fill jump,

\ >in offset to parse area within input buffer
var, >in

\ refill ( -- flag ) fill input buffer from input (TODO: support evaluate strings)
0 header, refill
          ' source call,
            ' 2dup call,
           ' erase call,
            ' drop call, \ TODO: bounds check
                   begin,
             ' key call,
             ' dup call,
                13 literal,
              ' <> call,
                   while,
            ' over call,
               ' ! call,
              ' 1+ call,
                   repeat,
           ' 2drop call,
                 0 literal,
             ' >in call,
               ' ! call,
            ' true jump,

\ parse ( char "ccc<char>" -- c-addr u ) parse ccc delimited by char
0 header, parse
          ' source call, \ char c-addr u
             ' >in call, \ char c-addr u in
               ' @ call, \ char c-addr u in
             ' rot call, \ char u in c-addr
               ' + call, \ char u start
            ' tuck call, \ char start u start
               ' + call, \ char start end
            ' over call, \ char start end start
                   ?do,  \ char start
            ' over call, \ char start char
               ' i call, \ char start char i
              ' c@ call, \ char start char c
            ' over call, \ char start char c char
              ' bl call,
               ' = call, \ char start char c sp?
                   if,   \ char start char c
              ' 1- call,
               ' > call, \ char start c<=$20
                   else,
               ' = call, \ char start =?
                   then,
                   if,
             ' nip call, \ start
               ' i call, \ start i
            ' over call, \ start i start
               ' - call, \ start len
             ' dup call, \ start len len
              ' 1+ call, \ start len len+
             ' >in call, \ start len in
              ' +! call, \ start len
          ' unloop call,
            ' exit call,
                   then,
                   loop,
             ' nip call, \ start
          ' source call, \ start c-addr u
             ' nip call, \ start u
            ' over call, \ start u start
               ' + call, \ start end
            ' over call, \ start end start
               ' - jump, \ start len

\ (skip) ( char "<chars>..." -- "..." ) skip leading delimeter chars
0 header, (skip)
          ' source call, \ char c-addr u
             ' >in call, \ char c-addr u in
               ' @ call, \ char c-addr u in
             ' rot call, \ char c-addr u in c-addr
               ' + call, \ char c-addr u inaddr
            ' tuck call, \ char inaddr c-addr u
               ' + call, \ char inaddr end
            ' swap call, \ char end inaddr
                   ?do,  \ char
             ' dup call, \ char char
               ' i call, \ char char addr
              ' c@ call, \ char char c
            ' over call, \ char char c char
              ' bl call, \ char char c char $20
               ' = call, \ char char c sp?
                   if,   \ char char c
            ' swap call, \ char c char
               ' > call, \ char c>$20 (not delimiter)
                   else, \ char char c
              ' <> call, \ char <>? (not delimiter)
                   then,
                   if,   \ char
                   leave,
                   then,
                 1 literal, \ char 1
             ' >in call,    \ char 1 in
              ' +! call,    \ char 
                   loop,
            ' drop jump,    \ 

\ parse-name ( "<spaces>name<space>" -- c-addr u ) skip leading space and parse name delimited by space
0 header, parse-name
              ' bl call,
             ' dup call,
          ' (skip) call,
           ' parse jump,

\ cmove ( c-addr1 c-addr2 u -- ) copy characters (from lower to higher addresses)
0 header, cmove
                 0 literal,
                   ?do,
            ' over call,
              ' c@ call,
            ' over call,
              ' c! call,
              ' 1+ call,
            ' swap call,
              ' 1+ call,
            ' swap call,
                   loop,
           ' 2drop jump,

\ >counted ( c-addr u -- c-addr ) string address and length to counted string address (non-standard)
0 header, >counted
             ' dup call, \ c-addr u u
             ' pad call, \ c-addr u u pad
              ' c! call, \ c-addr u -- length
             ' pad call, \ c-addr u pad
              ' 1+ call, \ c-addr u pad+
            ' swap call, \ c-addr pad+ u
           ' cmove call, \ 
             ' pad jump, \ pad

\ word ( char "<chars>ccc<char>" -- c-addr ) skip leading delimeters, parse ccc delimited by char, return transient counted string
0 header, word
             ' dup call,
          ' (skip) call,
           ' parse call,
        ' >counted jump,

\ count ( c-addr1 -- c-addr2 u ) counted string to c-addr and length
0 header, count
             ' dup call,
              ' c@ call,
            ' swap call,
              ' 1+ call,
            ' swap jump,

var, latest \ common, but non-standard

\ (equal) ( c-addr1 u1 c-addr2 u2 -- flag ) compare strings for equality
0 header, (equal)
             ' rot call,
            ' over call,
              ' <> call, \ lengths unequal?
                   if,
            ' drop call,
           ' 2drop call,
           ' false call,
            ' exit call,
                   then,
                 0 literal,
                   do,
            ' 2dup call,
               ' i call,
               ' + call,
              ' c@ call,
            ' swap call,
               ' i call,
               ' + call,
              ' c@ call,
              ' <> call,
                   if,
           ' 2drop call,
           ' false call,
          ' unloop call,
            ' exit call,
                   then,
                   loop,
           ' 2drop call,
            ' true jump,

\ find ( c-addr -- c-addr 0 | xt 1 | xt -1 ) find named definition (0=not found, 1=immediate, -1=non-immediate)
0 header, find
          ' latest call,
               ' @ call,    \ find link
                   begin,
            ' 2dup call,    \ find link find link
                 3 literal,
               ' + call,    \ find link find link+3
             ' dup call,
              ' 1- call,    \ find link find link+3 link+2
              ' c@ call,    \ find link find link+3 len/flag
               $7f literal,
             ' and call,    \ find link find link+3 len
             ' rot call,    \ find link link+3 len find
           ' count call,    \ find link link+3 len find len
           ' 2over call,    \ find link link+3 len find len link+3 len
         ' (equal) call,    \ find link link+3 len eq?
                   if,      \ find link link+3 len
               ' + call,    \ find link xt
            ' -rot call,    \ xt find link
             ' nip call,    \ xt link
                 2 literal,
               ' + call,    \ xt link+2
               ' @ call,    \ xt len/flag
               $80 literal,
             ' and call,    \ xt flag
              ' 0= call,
                   if,
                -1 literal, \ xt -1 (non-immediate)
                   else,
                 1 literal, \ xt 1 (immediate)
                   then,
            ' exit call,    \ xt (names match!)
                   then,    \ find link link+3 len
           ' 2drop call,    \ find link
               ' @ call,    \ find link (follow link)
             ' dup call,
              ' 0= call,
                   if,
            ' drop call,    \ find
                 0 literal, \ find 0  (not found)
            ' exit call,
                   then,
                   again,
                   ret,

\ ' ( "<spaces>name" -- xt ) skip leading space, parse and find name
0 header, '
              ' bl call, \ $20
            ' word call, \ c-addr
            ' find call, \ c-addr 0 | xt 1 | xt -1
              ' 0= call, \ c-addr -1 | xt 0
                   if,   \ c-addr
            ' drop call, \
                 0 literal, \ 0 \ TODO: abort with message?
                   then,
                   ret, \ 0 | xt

\ >number ( ud1 c-addr1 u1 -- ud2 c-addr2 u2 ) ud2 is the unsigned result of converting the characters
\         within the string specified by c-addr1 u1 into digits, using the number in BASE, and adding
\         each into ud1 after multiplying ud1 by the number in BASE. Conversion continues left-to-right
\         until a character that is not convertible, including any "+" or "-", is encountered or the string
\         is entirely converted. c-addr2 is the location of the first unconverted character or the first
\         character past the end of the string if the string was entirely converted. u2 is the number of
\         unconverted characters in the string. An ambiguous condition exists if ud2 overflows.
0 header, >number
                   begin,  \ n a c
             ' dup call,   \ n a c c
              ' 0> call,   \ n a c c0>
                   while,  \ n a c
            ' -rot call,   \ c n a
             ' dup call,   \ c n a a
              ' c@ call,   \ c n a d
               '0' literal,
               ' - call,   \ c n a d
             ' dup call,   \ c n a d d
                10 literal, \ cn a d d 10
               ' < call,   \ c n a d <
                   if,     \ c n a d  valid?
             ' rot call,   \ c a d n
            ' base call,   \ c a d n b
               ' @ call,   \ c a d n b
               ' * call,   \ c a d n
               ' + call,   \ c a n
            ' swap call,   \ c n a
              ' 1+ call,   \ c n a
             ' rot call,   \ n a c
              ' 1- call,   \ n a c
                   else,   \ c n a d  invalid
            ' drop call,   \ c n a
             ' rot call,   \ n a c
            ' exit call,
                   then,   \ n a c
                   repeat,
                   ret,

( --- assembler -------------------------------------------------------------- )

\ pc ( -- pc )
0 header, pc
                pc literal,
                   ret,
\ zero ( -- zero )
0 header, zero
              zero literal,
                   ret,

\ one ( -- one )
0 header, one
               one literal,
                   ret,

\ two ( -- two )
0 header, two
               two literal,
                   ret,

\ four ( -- four )
0 header, four
              four literal,
                   ret,

\ eight ( -- eight )
0 header, eight
             eight literal,
                   ret,

\ twelve ( -- twelve )
0 header, twelve
            twelve literal,
                   ret,

\ #t ( -- #t )
0 header, #t
                #t literal,
                   ret,

\ #f ( -- #f )
0 header, #f
                #f literal,
                   ret,

\ x ( -- x )
0 header, x
                 x literal,
                   ret,

\ y ( -- y )
0 header, y
                 y literal,
                   ret,

\ z ( -- z )
0 header, z
                 z literal,
                   ret,

\ w ( -- w )
0 header, w
                 w literal,
                   ret,

\ d ( -- d )
0 header, d
                 d literal,
                   ret,

\ r ( -- r )
0 header, r
                 r literal,
                   ret,

\ 2nybbles ( x i -- )
0 header, 2nybbles,
                 4 literal,
          ' lshift call,
              ' or call,
              ' c, jump,

\ 4nybbles, ( z y x i -- )
0 header, 4nybbles,
       ' 2nybbles, call,
       ' 2nybbles, jump,
    
\ halt, ( x -- ) halt(x) (halt with exit code x)
0 header, halt,
                 0 literal,
       ' 2nybbles, jump,

\ ldc, ( v x -- ) x=v (load constant signed v into x)
0 header, ldc,
                 1 literal,
       ' 2nybbles, call,
              ' c, jump,

\ ld+, ( z y x -- ) z<-[y] y+=x (load from memory and inc/dec pointer)
0 header, ld+,
                 2 literal,
       ' 4nybbles, jump,

\ st+, ( z y x -- ) z->[y] y+=x (store to memory and inc/dec pointer)
0 header, st+,
                 3 literal,
       ' 4nybbles, jump,

\ cp?, ( z y x -- ) z=y if x=0 (conditional copy)
0 header, cp?,
                 4 literal,
       ' 4nybbles, jump,

\ add, ( z y x -- ) z=y+x (addition)
0 header, add,
                 5 literal,
       ' 4nybbles, jump,

\ sub, ( z y x -- ) z=y-x (subtraction)
0 header, sub,
                 6 literal,
       ' 4nybbles, jump,

\ mul, ( z y x -- ) z=y*x (multiplication)
0 header, mul,
                 7 literal,
       ' 4nybbles, jump,

\ div, ( z y x -- ) z=y/x (division)
0 header, div,
                 8 literal,
       ' 4nybbles, jump,

\ nand, ( z y x -- ) z=y nand x (not-and)
0 header, nand,
                 9 literal,
       ' 4nybbles, jump,

\ shl, ( z y x -- ) z=y<<x (bitwise shift-left)
0 header, shl,
                10 literal,
       ' 4nybbles, jump,

\ shr, ( z y x -- ) z=y>>x (bitwise shift-right)
0 header, shr,
                11 literal,
       ' 4nybbles, jump,

\ in, ( y x -- ) x=getc() (read from console)
0 header, in,
                12 literal,
       ' 2nybbles, jump,

\ out, ( y x -- ) putc(x) (write to console)
0 header, out,
                13 literal,
       ' 2nybbles, jump,

\ read, ( z y x -- ) read(z,y,x) (file z of size y -> address x)
0 header, read,
                14 literal,
       ' 4nybbles, jump,

\ write, ( z y x -- ) write(z,y,x) (file z of size y <- address x)
0 header, write,
                15 literal,
       ' 4nybbles, jump,

\ cp, ( y x -- ) y=x (unconditional copy)
0 header, cp,
            ' zero call,
            ' cp?, jump,

\ ld, ( y x -- ) y<-[x] (load from memory)
0 header, ld,
            ' zero call,
            ' ld+, jump,

\ st, ( y x -- ) y->[x] (store to memory)
0 header, st,
            ' zero call,
            ' st+, jump,

\ jump, ( addr -- ) unconditional jump to address (following cell)
0 header, jump,
              ' pc call,
              ' pc call,
             ' ld, call,
               ' , jump,

\ ldv, ( val reg -- )
0 header, ldv,
              ' pc call,
             ' two call,
            ' ld+, call,
               ' , jump,

\ push, ( reg ptr -- )
0 header, push,
             ' dup call,
             ' dup call,
            ' four call,
            ' sub, call,
             ' st, jump,

\ pop, ( reg ptr -- )
0 header, pop,
            ' four call,
            ' ld+, jump,

\ pushd, ( reg -- )
0 header, pushd,
               ' d call,
           ' push, jump,

\ popd, ( reg -- )
0 header, popd,
               ' d call,
            ' pop, jump,

\ literal,
0 header, literal,
               ' x call,
            ' ldv, call,
               ' x call,
          ' pushd, jump,

\ pushr, ( reg -- )
0 header, pushr,
               ' r call,
           ' push, jump,

\ popr, ( reg -- )
0 header, popr,
               ' r call,
            ' pop, jump,

\ call, ( addr -- )
0 header, call,
              ' pc call,
          ' pushr, call,
           ' jump, jump,

\ ret, ( -- )
0 header, ret,
               ' x call,
           ' popr, call,
               ' x call,
               ' x call,
            ' four call,
            ' add, call,
              ' pc call,
               ' x call,
             ' cp, jump,

( --- interpreter ------------------------------------------------------------ )

\ source-id ( -- 0 | -1 ) Identifies the input source (-1=string [evaluate], 0=input device)
var, source-id

\ state ( -- a-addr ) compilation-state flag (true=compiling)
var, state

\ interpret ( c-addr u -- ) (implementation defined)
0 header, interpret
( c )   ' >counted call,
( xn )      ' find call, \ c-addr 0 | xt 1 | xt -1
( xnn )      ' dup call,
( xnb )       ' 0= call,
( xn )             if, \ not found?
( x )       ' drop call,
( au )     ' count call,
( auau )    ' 2dup call,
( auau0 )        0 literal,
( au0au )   ' -rot call,
( aunau) ' >number call,
( aunauu )   ' dup call,
( aunaub )    ' 0= call,
( aunau )          if, \ proper number
( aunaus ) ' state call,
( aunaus )     ' @ call,
( aunau )          if,   \ compile?
( aun )    ' 2drop call,
               ' x call, ' pc call, ' two call, ' ld+, call, \ ld+ two pc x -- x=[pc] pc += 2 -- load and skip literal
( au )         ' , call, \ compile literal value
               ' d call, ' d call, ' four call, ' sub, call, \ sub four d d -- push literal
               ' x call, ' d call, ' st, call, \ st d x
( )        ' 2drop call,
( aunau )          else, \ interactive
( aun )    ' 2drop call,
( nau )     ' -rot call,
( n )      ' 2drop call, \ leave number
( n )              then,
( aunau )          else, \ error
( aun )    ' 2drop call,
( au )      ' drop call,
( )           ' cr call,
( )         ' type call,
( ? )       char ? literal,
( )         ' emit call,
( )                then,
( xn )             else, \ found
( xn1 )          1 literal,
( xb )         ' = call,
( x )              if,   \ immediate?
( )      ' execute call,
( x )              else, \ non-immediate
( xs )     ' state call,
( xs )         ' @ call,
( x )              if,   \ compile?
               ' r call, ' r call, ' four call, ' sub, call, \ sub four r r -- push pc
              ' pc call, ' r call, ' st, call, \ st r pc
              ' pc call, ' pc call, ' ld, call, \ ld pc pc -- pc=[pc] -- jump to following address
( )            ' , call, \ xt is address to call
( x )              else, \ interactive
( )      ' execute call,
                   then,
                   then,
                   then,
                   ret,

\ quit ( -- ) ( R: i * x -- ) Empty the return stack, store zero in SOURCE-ID if it is present, make
\      the user input device the input source, and enter interpretation state. Do not display a message.
\      Repeat the following: Accept a line from the input source into the input buffer, set >IN to zero,
\      and interpret. Display the system prompt if in interpretation state, all processing has been
\      completed, and no ambiguous condition exists.
0 header, quit
  ' (clear-return) call,
                 0 literal,
       ' source-id call,
               ' ! call,
           ' false call,
           ' state call,
               ' ! call,
                   begin,
          ' refill call,
                   while,
                   begin,
      ' parse-name call, \ addr len
             ' dup call,
              ' 0> call,
                   while,
       ' interpret call,
                   repeat,
           ' 2drop call,
              ' bl call,
            ' emit call,
            char o literal,
            ' emit call,
            char k literal,
            ' emit call,
              ' cr call,
                   repeat,
                   \ TODO: failed to refill
                   ret,

\ [ ( -- ) enter interpretation state (immediate word)
$80 header, [
           ' false call,
           ' state call,
               ' ! jump,

\ ] ( -- ) enter compilation state
0 header, ]
            ' true call,
           ' state call,
               ' ! jump,

\ ; ( -- ) end current definition, make visible in dictionary and enter interpretation state
$80 header, ;
               ' [ call,
               ' r call, ' r call, ' four call, ' add, call, \ add four r r -- r=r+4
               ' x call, ' r call, ' ld, call, \ ld r x -- x=[r]
               ' x call, ' x call, ' four call, ' add, call, \ add four x x -- x=x+4
              ' pc call, ' x call, ' cp, call, \ cp x pc -- pc=x
                   ret,

\ header, ( flag "<spaces>name -- ) append header to dictionary (non-standard)
0 header, header,
          ' latest call,
               ' @ call, \ link to current (soon to be previous) word
            ' here call,
          ' latest call,
               ' ! call, \ update latest to this word
               ' , call, \ append link
      ' parse-name call, ( flag addr len -- )
             ' rot call,
            ' over call,
              ' or call,
              ' c, call, \ append flag/len
            ' over call,
               ' + call,
            ' swap call, ( end start -- )
                   do,   \ append name
               ' i call,
              ' c@ call,
              ' c, call,
                   loop,
                   ret,  ( addr len )

\ create ( "<spaces>name -- ) make dictionary header and compile push PFA and return
0 header, create
                 0 literal,
         ' header, call,
              3137 literal, ' , call, \ 410C -> cp? zero pc x   x=pc
              3613 literal, ' , call, \ 1D0E -> ldc y 14        y=14
            -13219 literal, ' , call, \ 5DCC -> add y x x       x=y+x
            -14283 literal, ' , call, \ 35C8 -> st+ -four x d   push x
            -26282 literal, ' , call, \ 5699 -> add four r r    ret
            -27871 literal, ' , call, \ 2193 -> ld+ zero r t
             13142 literal, ' , call, \ 5633 -> add four t t
             12353 literal, ' , jump, \ 4130 -> cp? zero t pc

( --- end of dictionary ------------------------------------------------------ )
                   patch,
    ' (clear-data) call,
              here literal, \ update dictionary pointer to compile-time position
              ' dp call,
               ' ! call,
          latest @ literal, \ update latest to compile-time
          ' latest call,
               ' ! call,
         ' decimal call, \ default base
            ' quit jump,
\             zero halt,