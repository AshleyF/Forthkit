require assembler.fs

    2 constant one       1     one ldc, \ literal 1
    3 constant two       2     two ldc, \ literal 2
    4 constant four      4    four ldc, \ literal 4
    5 constant eight     8   eight ldc, \ literal 8
    6 constant twelve   12  twelve ldc, \ literal 12
    7 constant fifteen  15 fifteen ldc, \ literal 15
    
    8 constant #t       -1     #t ldc, \ literal true (-1)
 zero constant #f                      \ literal false (0)
 
    9 constant x
   10 constant y
   11 constant z
   12 constant w

: ldv, ( val reg -- ) pc two ld+, , ;

( --- stacks ----------------------------------------------------------------- )

: push, ( reg ptr -- ) dup dup four sub,  st, ;
: pop,  ( reg ptr -- ) four ld+, ;

13 constant d \ data stack pointer (initialized after dictionary below)

: pushd, ( reg -- ) d push, ;
: popd,  ( reg -- ) d pop, ;

: literal, x ldv,  x pushd, ; \ 8 bytes

14 constant r  memory-size r ldv, \ return stack pointer

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
: else, ( C: orig1 -- orig2 ) branch, swap then, ; \ patch previous branch to here, dummy unconditionally branch over false block

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

variable latest  $ffff latest !

: header, ( flag "<spaces>name" -- )
  latest @          \ link to current (soon to be previous) word
  here latest !     \ update latest to this word
  ,                 \ append link
  parse-name        \ ( flag addr len -- )
  rot over or c,    \ append flag/len
  over + swap       \ ( end start -- )
  do i c@ c, loop ; \ append name

: find-word ( addr -- )
  latest @ \ ( find link -- )
  begin
    2dup memory + 3 + dup 1- c@ $7f and \ get name/len
    rot count 2over str= ( compare 0= ) if + memory - -rot 2drop exit then \ return xt if names match
    2drop s@ dup 0= if ." Word not found: " drop count type cr exit then \ follow link, until end
  again ;

: ' bl word find-word ;

: ['] ' postpone literal ; immediate

: def 
  0 header,
  begin
    bl word dup count s>number? if
      drop literal, drop \ compile literal number
    else
      2drop dup 1+ c@ [char] ; = if
        drop ret, exit \ compile return
      else
        find-word call, \ compile call
      then
    then
  again
;

true warnings ! \ intentionally redefining (latest, header, ')

( --- primitives ------------------------------------------------------------- )

               branch, \ skip dictionary

\ leave empty space for new image
\ memory $8000 + h !

\ (clear-data) empty return stack (non-standard)
0 header, (clear-data)
 memory-size 2 + d ldv,
                   ret,

\ (clear-return) empty return stack (non-standard)
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
     y one fifteen shl,
             y y x and, \ get sign bit
           x x one shr,
           x x y or, \ replace sign bit
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

\ read-block ( file addr size -- ) block file of size -> address
0 header, read-block
                 x popd,
                 y popd,
                 z popd,
             z y x read,
                   ret,

\ write-block ( file addr size -- ) block file of size -> address
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
       x x fifteen shr, \ sign bit to 1s place
           x x one and,
               x x not, \ negate
           x x one add,
               x d st,
                   ret,

\ < ( y x -- b ) true if y less than x (- 0<) TODO: handle overflow (see bootstrap)!
0 header, <
                 x popd,
               y d ld,
             z y x sub, \ negative if y less than x
       z z fifteen shr, \ sign bit to 1s place
           z z one and,
               z z not, \ negate
           z z one add,
               z d st,
                   ret,

\ 0> ( x -- b ) true if x greater than zero (1- 15 rshift 1-)
0 header, 0>
               x d ld,
           x x one sub, \ negative if not greater than zero
       x x fifteen shr, \ sign bit to 1s place
           x x one and,
               x x not, \ negate
           x x one add,
               x x not, \ invert
               x d st,
                   ret,

\ > ( y x -- b ) true if y greater than x (- 0>) TODO: handle overflow (see bootstrap)!
0 header, >
                 x popd,
               y d ld,
             x y x sub, \ negative if y less than x
           x x one sub, \ negative if y is equal to x
       x x fifteen shr, \ sign bit to 1s place
           x x one and,
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
\ 0 header, here
\               ' dp call,
\                ' @ jump,
def here dp @ ;

\ unused ( -- remaining ) dictionary space remaining
0 header, unused
       memory-size literal, \ can't be done in def
            ' here call,
               ' - jump, \ TODO: consider stack space?

\ allot ( n -- ) advance dictionary pointer (dp +!)
\ 0 header, allot
\               ' dp call,
\               ' +! jump,
def allot dp +! ;

\ , ( x -- ) append x in newly reserved cell (here ! 1 cells allot)
\ 0 header, ,
\             ' here call,
\                ' ! call,
\                  1 literal,
\            ' cells call,
\            ' allot jump,
def , here ! 1 cells allot ;

\ c, ( x -- ) append x chars in newly reserved space (here c! 1 chars allot)
\ 0 header, c,
\             ' here call,
\               ' c! call,
\                  1 literal,
\            ' chars call,
\            ' allot jump,
def c, here c! 1 chars allot ;

( --- secondaries ------------------------------------------------------------ )

\ 2! ( y x addr -- ) store x y at consecutive addresses (tuck ! cell+ !)
\ 0 header, 2!
\             ' tuck call,
\                ' ! call,
\            ' cell+ call,
\                ' ! jump,
def 2! tuck ! cell+ ! ;

\ 2@ ( addr -- y x ) fetch pair of consecutive addresses (dup cell+ @ swap @)
\ 0 header, 2@
\              ' dup call,
\            ' cell+ call,
\                ' @ call,
\             ' swap call,
\                ' @ jump,
def 2@ dup cell+ @ swap @ ;

\ xor ( y x -- result ) logical/bitwise exclusive or (2dup or -rot and invert and)
\ 0 header, xor
\             ' 2dup call,
\               ' or call,
\             ' -rot call,
\              ' and call,
\           ' invert call,
\              ' and jump,
def xor 2dup or -rot and invert and ;

\ abs ( x -- |x| ) absolute value (dup 0< if negate then)
0 header, abs
             ' dup call,
              ' 0< call,
                   if, \ can't use in def
          ' negate call,
                   then, \ can't use in def
                   ret,

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
\ 0 header, i
\              ' 2r@ call, \ including return from here
\             ' drop jump,
def i 2r@ drop ;

\ j ( -- x ) ( R: x -- x ) copy next outer loop index (2r> 2r@ drop -rot 2>r) (x r twelve add, x x ld, x pushd, ret,)
\ 0 header, j
\              ' 2r> call, \ i this
\              ' 2r@ call, \ i this j limit
\             ' drop call, \ i this j
\             ' -rot call, \ j i this
\              ' 2>r call, \ tricky, don't jump here!
\                    ret,
def j 2r> 2r@ drop -rot 2>r ;

\ unloop ( -- ) ( R: y x -- ) remove loop parameters (r> 2r> rot >r 2drop)
\ 0 header, unloop
\               ' r> call, \ this return address
\              ' 2r> call, \ loop parameters
\              ' rot call,
\               ' >r call, \ replace this return address
\            ' 2drop jump,
def unloop r> 2r> rot >r 2drop ;

( --- interpreter ------------------------------------------------------------ )

\ true ( -- true ) return true flag (-1 constant true)
\ 0 header, true
\                 -1 literal,
\                    ret,
def true -1 ;

\ false ( -- false ) return false flag (0 constant false)
\ 0 header, false
\                  0 literal,
\                    ret,
def false 0 ;

\ bl ( -- c ) space character value (32 constant bl)
\ 0 header, bl
\                 32 literal,
\                    ret,
def bl 32 ;

\ space ( -- ) emit space character (bl emit)
\ 0 header, space
\               ' bl call,
\             ' emit jump,
def space bl emit ;

\ cr ( -- ) cause newline (10 emit)
\ 0 header, cr
\                 10 literal,
\             ' emit jump,
def cr 10 emit ;

\ type ( addr len -- ) display the character string (0 max 0 ?do dup c@ emit char+ loop drop)
0 header, type
             ' dup call, \ essentially 0 max, but we don't have max yet
              ' 0< call,
                   if,
            ' drop call,
                 0 literal,
                   then,
                 0 literal,
                   ?do, \ can't use in def
             ' dup call,
              ' c@ call,
            ' emit call,
           ' char+ call,
                   loop, \ can't use in def
            ' drop jump,

\ pad ( -- addr ) address of transient region for intermediate processing
\ 0 header, pad
\             ' here call,
\               1024 literal, \ arbitrary distance away
\                ' + call,
\          ' aligned jump,
def pad here 1024 + aligned ;

\ np ( -- addr ) return address of pictured numeric output pointer (non-standard)
var, np

\ <# ( -- ) initialize pictured numeric output (pad 64 + np !)
\ 0 header, <#
\              ' pad call,
\                 64 literal, \ arbitrary distance away
\                ' + call,
\               ' np call,
\                ' ! jump,
def <# pad 64 + np ! ;

\ hold ( char -- ) add char to beginning of pictured numeric output (np -1 over +! c!)
\ 0 header, hold
\               ' np call,
\                 -1 literal,
\             ' over call,
\               ' +! call,
\                ' @ call,
\               ' c! jump,
def hold np -1 over +! @ c! ;

\ holds ( addr len -- ) add string to beginning of pictured numeric output (begin dup while 1- 2dup + c@ hold repeat 2drop)
0 header, holds
                   begin, \ can't use in def
             ' dup call,
                   while,
              ' 1- call,
            ' 2dup call,
               ' + call,
              ' c@ call,
            ' hold call,
                   repeat, \ can't use in def
           ' 2drop jump,

\ base ( -- base ) address of current number-conversion radix (2..36, initially 10)
var, base

\ decimal ( -- ) set number-conversion radix to 10
\ 0 header, decimal
\                 10 literal,
\             ' base call,
\                ' ! jump,
def decimal 10 base ! ;

\ hex ( -- ) set number-conversion radix to 16
\ 0 header, hex
\                 16 literal,
\             ' base call,
\                ' ! jump,
def hex 16 base ! ;

\ octal ( -- ) set number-conversion radix to 8 (non-standard)
\ 0 header, octal
\                  8 literal,
\             ' base call,
\                ' ! jump,
def octal 8 base ! ;

\ binary ( -- ) set number-conversion radix to 2 (non-standard)
\ 0 header, binary
\                  2 literal,
\             ' base call,
\                ' ! jump,
def binary 2 base ! ;

\ # ( ud -- ud ) prepend least significant digit to pictured numeric output, return ud/base
\ 0 header, #
\             ' swap call, \ TODO: support double numbers
\             ' base call,
\                ' @ call,
\             ' 2dup call,
\              ' mod call,
\                 48 literal, \ '0'
\                ' + call,
\             ' hold call, \ 0 n b
\                ' / call, \ 0 m
\             ' swap jump, \ TODO: support double numbers
def # swap base @ 2dup mod 48 + hold / swap ; \ TODO: doesn't work with negative values!
\ base @ ud/mod rot dup #9 u> #7 and + #48 + hold ;

\ #s ( ud -- ud ) convert all digits using # (appends at least 0)
0 header, #s
            ' swap call, \ TODO: support double numbers
                   begin, \ can't do in def
            ' swap call, \ TODO: support double numbers
               ' # call, \ note: at least once even if zero
            ' swap call, \ TODO: support double numbers
             ' dup call,
             ' 0<> call,
                   while, \ can't do in def
                   repeat, \ can't do in def
            ' swap jump, \ TODO: support double numbers

\ sign ( n -- ) if negative, prepend '-' to pictured numeric output
0 header, sign
              ' 0< call,
                   if, \ can't do in def
                45 literal, \ '-'
            ' hold call,
                   then, \ can't do in def
                   ret,

\ #> ( xd -- addr len ) make pictured numeric output string available (np @ pad 64 + over -)
\ 0 header, #>
\            ' 2drop call,
\               ' np call,
\                ' @ call,
\              ' pad call,
\                 64 literal, \ arbitrary distance away
\                ' + call,
\             ' over call,
\                ' - jump,
def #> 2drop np @ pad 64 + over - ;

\ s>d ( n -- d ) convert number to double-cell
\ 0 header, s>d
\              ' dup call,
\               ' 0< jump,
def s>d dup 0< ;

\ . ( n -- ) display value in free field format (dup abs s>d <# #s rot sign #> type space)
\ 0 header, .
\              ' dup call,
\              ' abs call,
\              ' s>d call,
\               ' <# call,
\               ' #s call,
\              ' rot call,
\             ' sign call,
\               ' #> call,
\            ' space call,
\             ' type jump,
def . dup abs s>d <# #s rot sign #> space type ;

\ d. ( u -- ) display unsigned value in free field format (from double word set)
\ 0 header, d.
\               ' <# call,
\               ' #s call,
\               ' #> call,
\            ' space call,
\             ' type jump,
def d. <# #s #> space type ;

\ u. ( u -- ) display unsigned value in free field format (0 <# #s #> type space)
\ 0 header, u.
\                  0 literal,
\               ' d. jump,
def u. 0 d. ;

\ .s ( -- ) display values on the stack non-destructively (depth dup 0 do dup i - pick . loop drop)
0 header, .s
           ' depth call,
             ' dup call,
                 0 literal,
                   do, \ TODO: ?do bug \ can't use in def
             ' dup call,
               ' i call,
               ' - call,
            ' pick call,
               ' . call,
                   loop, \ can't use in def
            ' drop call,
                   ret,

\ ? ( addr -- ) display value stored at address (@ .)
\ 0 header, ?
\                ' @ call,
\                ' . jump,
def ? @ . ;

( --- interpreter ------------------------------------------------------------ )

\ source-id ( -- addr ) source buffer address (initialized below)
var, source-addr

\ source-len ( -- len ) source buffer length (initialized below)
var, source-len

\ source ( -- c-addr u )
\ 0 header, source
\     ' source-addr call,
\               ' @ call,
\      ' source-len call,
\               ' @ jump,
def source source-addr @ source-len @ ;

\ fill ( c-addr u char -- ) if u is greater than zero, store char in each of u consecutive characters of memory beginning at c-addr.
0 header, fill
            ' -rot call,
                 0 literal,
                   ?do, \ can't use in def
            ' 2dup call,
              ' c! call,
              ' 1+ call,
                   loop, \ can't use in def
           ' 2drop jump,

\ erase ( addr u -- ) if u is greater than zero, clear all bits in each of u consecutive address units of memory beginning at addr.
\ 0 header, erase
\                  0 literal,
\             ' fill jump,
def erase 0 fill ;

\ >in offset to parse area within input buffer
var, >in

\ accept ( c-addr +n1 -- +n2 ) receive string of, at most, n1 chars into c-addr, returning number of chars (n2)
0 header, accept
                 0 literal, \ c-addr n1 0
                   begin,   \ c-addr n1 n2
              ' 1+ call,    \ c-addr n1 n2+                  increment n2
            ' 2dup call,    \ c-addr n1 n2+ n1 n2+
               ' < call,    \ c-addr n1 n2+ <
                   if,      \ c-addr n1 n2+                  bounds check  \ can't use in def
            ' drop call,    \ c-addr n1
             ' nip call,    \ n1
            ' exit call,    \ n1
                   then,    \ c-addr n1 n2+   \ can't use in def
             ' rot call,    \ n1 n2+ c-addr
             ' key call,    \ n1 n2+ c-addr key
            ' swap call,    \ n1 n2+ key c-addr
            ' 2dup call,    \ n1 n2+ key c-addr key c-addr
               ' ! call,    \ n1 n2+ key c-addr              store key
              ' 1+ call,    \ n1 n2+ key c-addr+
            ' swap call,    \ n1 n2+ c=addr+ key
             ' dup call,    \ n1 n2+ c-addr+ key key 
                13 literal, \ n1 n2+ c-addr+ key key 13
              ' <> call,    \ n1 n2+ c-addr+ key <>13
            ' swap call,    \ n1 n2+ c-addr+ <>13 key
                10 literal, \ n1 n2+ c-addr+ <>13 key 10
              ' <> call,    \ n1 n2+ c-addr+ <>13 <>10
             ' and call,    \ n1 n2+ c-addr+ <>CRLF
                   while,   \ n1 n2+ c-addr+   \ can't use in def
            ' -rot call,    \ c-addr+ n1 n2+
                   repeat,  \ c-addr+ n1 n2+   \ can't use in def
            ' drop call,    \ n1 n2+
             ' nip jump,    \ n2+

\ source-id ( -- 0 | -1 ) Identifies the input source (-1=string [evaluate], 0=input device)
var, source-id

\ refill ( -- flag ) fill input buffer from input (do nothing and return false when evaluating strings)
0 header, refill
       ' source-id call,
               ' @ call,
              ' 0= call,
             ' dup call, \ return value
                   if,   \ can't use in def
          ' source call,
            ' 2dup call, \ TODO REMOVE
           ' erase call, \ TODO REMOVE
          ' accept call,
            ' drop call, \ TODO SET LEN
                 0 literal,
             ' >in call,
               ' ! call,
                   then, \ can't use in def
                   ret,

\ parse ( char "ccc<char>" -- c-addr u ) parse ccc delimited by char
0 header, parse
          ' source call, \ char c-addr u
            ' over call, \ char c-addr u c-addr
               ' + call, \ char c-addr end
              ' 1+ call, \ char c-addr end+
            ' swap call, \ char end c-addr
             ' >in call, \ char end c-addr in-addr
               ' @ call, \ char end c-addr in
               ' + call, \ char end start
            ' tuck call, \ char start end start
                   ?do,  \ char start    \ can't use in def
            ' over call, \ char start char
               ' i call, \ char start char i
              ' c@ call, \ char start char c
            ' over call, \ char start char c char
              ' bl call,
               ' = call, \ char start char c sp?
                   if,   \ char start char c    \ can't use in def
              ' 1- call,
               ' > call, \ char start c<=$20
                   else,    \ can't use in def
               ' = call, \ char start =?
                   then,
                   if,    \ can't use in def
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
                   then,    \ can't use in def
                   loop,    \ can't use in def
             ' nip call, \ start
          ' source call, \ start c-addr u
               ' + call, \ start end
            ' over call, \ start end start
               ' - call, \ start len
             ' dup call, \ start len len
              ' 1+ call, \ start len len+
             ' >in call, \ start len in
              ' +! jump, \ start len

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
                   ?do,  \ char    \ can't use in def
             ' dup call, \ char char
               ' i call, \ char char addr
              ' c@ call, \ char char c
            ' over call, \ char char c char
              ' bl call, \ char char c char $20
               ' = call, \ char char c sp?
                   if,   \ char char c    \ can't use in def
            ' swap call, \ char c char
               ' > call, \ char c>$20 (not delimiter)
                   else, \ char char c
              ' <> call, \ char <>? (not delimiter)
                   then,
                   if,   \ char    \ can't use in def
                   leave,
                   then,    \ can't use in def
                 1 literal, \ char 1
             ' >in call,    \ char 1 in
              ' +! call,    \ char 
                   loop,    \ can't use in def
            ' drop jump,    \ 

\ parse-name ( "<spaces>name<space>" -- c-addr u ) skip leading space and parse name delimited by space
\ 0 header, parse-name
\               ' bl call,
\              ' dup call,
\           ' (skip) call,
\            ' parse jump,
def parse-name bl dup (skip) parse ;

\ cmove ( c-addr1 c-addr2 u -- ) copy characters (from lower to higher addresses)
0 header, cmove
                 0 literal,
                   ?do,    \ can't use in def
            ' over call,
              ' c@ call,
            ' over call,
              ' c! call,
           ' char+ call,
            ' swap call,
           ' char+ call,
            ' swap call,
                   loop,    \ can't use in def
           ' 2drop jump,

\ >counted ( c-addr u -- c-addr ) string address and length to counted string address (non-standard)
\ 0 header, >counted
\              ' dup call, \ c-addr u u
\              ' pad call, \ c-addr u u pad
\               ' c! call, \ c-addr u -- length
\              ' pad call, \ c-addr u pad
\               ' 1+ call, \ c-addr u pad+
\             ' swap call, \ c-addr pad+ u
\            ' cmove call, \ 
\              ' pad jump, \ pad
def >counted dup pad c! pad 1+ swap cmove pad ;

\ word ( char "<chars>ccc<char>" -- c-addr ) skip leading delimeters, parse ccc delimited by char, return transient counted string
\ 0 header, word
\              ' dup call,
\           ' (skip) call,
\            ' parse call,
\         ' >counted jump,
def word dup (skip) parse >counted ;

\ count ( c-addr1 -- c-addr2 u ) counted string to c-addr and length
\ 0 header, count
\              ' dup call,
\               ' c@ call,
\             ' swap call,
\               ' 1+ call,
\             ' swap jump,
def count dup c@ swap 1+ swap ;

var, latest \ common, but non-standard

0 header, (tolower) \ non-standard
             ' dup call,
         char A 1- literal,  \ can't use char A in def
               ' > call,
            ' over call,
         char Z 1+ literal,  \ can't use char Z in def
               ' < call,
             ' and call,
                   if,
                32 literal,
               ' + call,
                   then,
                   ret,

: tolower dup [char] A [char] Z within if 32 + then ;

\ (equal) ( c-addr1 u1 c-addr2 u2 -- flag ) compare strings for equality (non-standard)
0 header, (equal)
             ' rot call,
            ' over call,
              ' <> call, \ lengths unequal?
                   if,   \ can't use in def
            ' drop call,
           ' 2drop call,
           ' false call,
            ' exit call,
                   then,   \ can't use in def
                 0 literal,
                   do,   \ can't use in def
            ' 2dup call,
               ' i call,
               ' + call,
              ' c@ call,
       ' (tolower) call,
            ' swap call,
               ' i call,
               ' + call,
              ' c@ call,
       ' (tolower) call,
              ' <> call,
                   if,   \ can't use in def
           ' 2drop call,
           ' false call,
          ' unloop call,
            ' exit call,
                   then,   \ can't use in def
                   loop,   \ can't use in def
           ' 2drop call,
            ' true jump,

\ find ( c-addr -- c-addr 0 | xt 1 | xt -1 ) find named definition (0=not found, 1=immediate, -1=non-immediate)
0 header, find
          ' latest call,
               ' @ call,    \ find link
                   begin,   \ can't use in def
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
                   if,      \ find link link+3 len    \ can't use in def
               ' + call,    \ find link xt
            ' -rot call,    \ xt find link
             ' nip call,    \ xt link
                 2 literal,
               ' + call,    \ xt link+2
               ' @ call,    \ xt len/flag
               $80 literal,
             ' and call,    \ xt flag
              ' 0= call,
                   if,    \ can't use in def
                -1 literal, \ xt -1 (non-immediate)
                   else,
                 1 literal, \ xt 1 (immediate)
                   then,    \ can't use in def
            ' exit call,    \ xt (names match!)
                   then,    \ find link link+3 len
           ' 2drop call,    \ find link
               ' @ call,    \ find link (follow link)
             ' dup call,
\            $ffff literal, \ want 0000 to be a valid link
\              ' = call,
              ' 0= call,
                   if,    \ can't use in def
            ' drop call,    \ find
                 0 literal, \ find 0  (not found)
            ' exit call,
                   then,    \ can't use in def
                   again,    \ can't use in def
                   ret,

\ ' ( "<spaces>name" -- xt ) skip leading space, parse and find name
0 header, '
              ' bl call, \ $20
            ' word call, \ c-addr
            ' find call, \ c-addr 0 | xt 1 | xt -1
              ' 0= call, \ c-addr -1 | xt 0
                   if,   \ c-addr    \ can't use in def
            ' drop call, \
                 0 literal, \ 0 \ TODO: abort with message?
                   then,    \ can't use in def
                   ret, \ 0 | xt

\ >number ( ud1 c-addr1 u1 -- ud2 c-addr2 u2 ) ud2 is the unsigned result of converting the characters
\         within the string specified by c-addr1 u1 into digits, using the number in BASE, and adding
\         each into ud1 after multiplying ud1 by the number in BASE. Conversion continues left-to-right
\         until a character that is not convertible, including any "+" or "-", is encountered or the string
\         is entirely converted. c-addr2 is the location of the first unconverted character or the first
\         character past the end of the string if the string was entirely converted. u2 is the number of
\         unconverted characters in the string. An ambiguous condition exists if ud2 overflows.
0 header, >number \ TODO: support double ud1/ud2?
                   begin,  \ n a c    \ can't use in def
             ' dup call,   \ n a c c
              ' 0> call,   \ n a c c0>
                   while,  \ n a c    \ can't use in def
            ' -rot call,   \ c n a
             ' dup call,   \ c n a a
              ' c@ call,   \ c n a d
               '0' literal,
               ' - call,   \ c n a d
             ' dup call,   \ c n a d d
                10 literal, \ cn a d d 10
               ' < call,   \ c n a d <
                   if,     \ c n a d  valid?    \ can't use in def
             ' rot call,   \ c a d n
            ' base call,   \ c a d n b
               ' @ call,   \ c a d n b
               ' * call,   \ c a d n
               ' + call,   \ c a n
            ' swap call,   \ c n a
              ' 1+ call,   \ c n a
             ' rot call,   \ n a c
              ' 1- call,   \ n a c
                   else,   \ c n a d  invalid    \ can't use in def
            ' drop call,   \ c n a
             ' rot call,   \ n a c
            ' exit call,
                   then,   \ n a c    \ can't use in def
                   repeat,    \ can't use in def
                   ret,

( --- assembler -------------------------------------------------------------- )

\ pc ( -- pc )
0 header, pc
                pc literal,   \ can't use in def
                   ret,
\ zero ( -- zero )
0 header, zero
              zero literal,   \ can't use in def
                   ret,

\ one ( -- one )
0 header, one
               one literal,   \ can't use in def
                   ret,

\ two ( -- two )
0 header, two
               two literal,   \ can't use in def
                   ret,

\ four ( -- four )
0 header, four
              four literal,   \ can't use in def
                   ret,

\ eight ( -- eight )
0 header, eight
             eight literal,   \ can't use in def
                   ret,

\ twelve ( -- twelve )
0 header, twelve
            twelve literal,   \ can't use in def
                   ret,

\ #t ( -- #t )
0 header, #t
                #t literal,   \ can't use in def
                   ret,

\ #f ( -- #f )
0 header, #f
                #f literal,   \ can't use in def
                   ret,

\ x ( -- x )
0 header, x
                 x literal,   \ can't use in def
                   ret,

\ y ( -- y )
0 header, y
                 y literal,   \ can't use in def
                   ret,

\ z ( -- z )
0 header, z
                 z literal,   \ can't use in def
                   ret,

\ w ( -- w )
0 header, w
                 w literal,   \ can't use in def
                   ret,

\ d ( -- d )
0 header, d
                 d literal,   \ can't use in def
                   ret,

\ r ( -- r )
0 header, r
                 r literal,   \ can't use in def
                   ret,

\ 2nybbles ( x i -- )
\ 0 header, 2nybbles,
\                  4 literal,
\           ' lshift call,
\               ' or call,
\               ' c, jump,
def 2nybbles, 4 lshift or c, ;

\ 4nybbles, ( z y x i -- )
\ 0 header, 4nybbles,
\        ' 2nybbles, call,
\        ' 2nybbles, jump,
def 4nybbles, 2nybbles, 2nybbles, ;
    
\ halt, ( x -- ) halt(x) (halt with exit code x)
\ 0 header, halt,
\                  0 literal,
\        ' 2nybbles, jump,
def halt, 0 2nybbles, ;

\ ldc, ( v x -- ) x=v (load constant signed v into x)
\ 0 header, ldc,
\                  1 literal,
\        ' 2nybbles, call,
\               ' c, jump,
def ldc, 1 2nybbles, c, ;

\ ld+, ( z y x -- ) z<-[y] y+=x (load from memory and inc/dec pointer)
\ 0 header, ld+,
\                  2 literal,
\        ' 4nybbles, jump,
def ld+, 2 4nybbles, ;

\ st+, ( z y x -- ) z->[y] y+=x (store to memory and inc/dec pointer)
\ 0 header, st+,
\                  3 literal,
\        ' 4nybbles, jump,
def st+, 3 4nybbles, ;

\ cp?, ( z y x -- ) z=y if x=0 (conditional copy)
\ 0 header, cp?,
\                  4 literal,
\        ' 4nybbles, jump,
def cp?, 4 4nybbles, ;

\ add, ( z y x -- ) z=y+x (addition)
\ 0 header, add,
\                  5 literal,
\        ' 4nybbles, jump,
def add, 5 4nybbles, ;

\ sub, ( z y x -- ) z=y-x (subtraction)
\ 0 header, sub,
\                  6 literal,
\        ' 4nybbles, jump,
def sub, 6 4nybbles, ;

\ mul, ( z y x -- ) z=y*x (multiplication)
\ 0 header, mul,
\                  7 literal,
\        ' 4nybbles, jump,
def mul, 7 4nybbles, ;

\ div, ( z y x -- ) z=y/x (division)
\ 0 header, div,
\                  8 literal,
\        ' 4nybbles, jump,
def div, 8 4nybbles, ;

\ nand, ( z y x -- ) z=y nand x (not-and)
\ 0 header, nand,
\                  9 literal,
\        ' 4nybbles, jump,
def nand, 9 4nybbles, ;

\ shl, ( z y x -- ) z=y<<x (bitwise shift-left)
\ 0 header, shl,
\                 10 literal,
\        ' 4nybbles, jump,
def shl, 10 4nybbles, ;

\ shr, ( z y x -- ) z=y>>x (bitwise shift-right)
\ 0 header, shr,
\                 11 literal,
\        ' 4nybbles, jump,
def shr, 11 4nybbles, ;

\ in, ( y x -- ) x=getc() (read from console)
\ 0 header, in,
\                 12 literal,
\        ' 2nybbles, jump,
def in, 12 2nybbles, ;

\ out, ( y x -- ) putc(x) (write to console)
\ 0 header, out,
\                 13 literal,
\        ' 2nybbles, jump,
def out, 13 2nybbles, ;

\ read, ( z y x -- ) read(z,y,x) (file z of size y -> address x)
\   0 header, read,
\                   14 literal,
\          ' 4nybbles, jump,
def read, 14 4nybbles, ;

\ write, ( z y x -- ) write(z,y,x) (file z of size y <- address x)
\   0 header, write,
\                   15 literal,
\          ' 4nybbles, jump,
def write, 15 4nybbles, ;

\ cp, ( y x -- ) y=x (unconditional copy)
\ 0 header, cp,
\             ' zero call,
\             ' cp?, jump,
def cp, zero cp?, ;

\ ld, ( y x -- ) y<-[x] (load from memory)
\   0 header, ld,
\               ' zero call,
\               ' ld+, jump,
def ld, zero ld+, ;

\ st, ( y x -- ) y->[x] (store to memory)
\   0 header, st,
\               ' zero call,
\               ' st+, jump,
def st, zero st+, ;

\ jump, ( addr -- ) unconditional jump to address (following cell)
\ 0 header, jump,
\               ' pc call,
\               ' pc call,
\              ' ld, call,
\                ' , jump,
def jump, pc pc ld, , ;

\ ldv, ( val reg -- )
\ 0 header, ldv,
\               ' pc call,
\              ' two call,
\             ' ld+, call,
\                ' , jump,
def ldv, pc two ld+, , ;

\ push, ( reg ptr -- )
\ 0 header, push,
\              ' dup call,
\              ' dup call,
\             ' four call,
\             ' sub, call,
\              ' st, jump,
def push, dup dup four sub, st, ;

\ pop, ( reg ptr -- )
\ 0 header, pop,
\             ' four call,
\             ' ld+, jump,
def pop, four ld+, ;

\ pushd, ( reg -- )
\   0 header, pushd,
\                  ' d call,
\              ' push, jump,
def pushd, d push, ;

\ popd, ( reg -- )
\   0 header, popd,
\                  ' d call,
\               ' pop, jump,
def popd, d pop, ;

\ literal, \ TODO CONTINUE WIP
0 header, literal,
               ' x call,
            ' ldv, call,
               ' x call,
          ' pushd, jump,
\ def literal, x ldv, x pushd, ;

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

\ state ( -- a-addr ) compilation-state flag (true=compiling)
var, state

\ interpret ( c-addr u -- ) (implementation defined)
0 header, interpret \ can't be def
        ' >counted call,
            ' find call, \ c-addr 0 | xt 1 | xt -1
             ' dup call,
              ' 0= call,
                   if, \ not found?
            ' drop call,
           ' count call,
            ' over call, \ determine whether prefixed with -
              ' c@ call,
            char - literal,
               ' = call,
                   if,
              ' 1- call, \ decrement length
            ' swap call,
              ' 1+ call, \ increment address
            ' swap call,
                -1 literal, \ multiplier
                   else,
                 1 literal, \ multiplier
                   then,
            ' -rot call,
            ' 2dup call,
                 0 literal,
            ' -rot call,
         ' >number call,
             ' dup call,
              ' 0= call,
                   if, \ proper number
           ' 2drop call,
            ' -rot call,
           ' 2drop call,
               ' * call,
           ' state call,
               ' @ call,
                   if,   \ compile?
        ' literal, call,
                   then,
                   else, \ error
           ' 2drop call,
            ' drop call,
              ' cr call,
            ' type call,
            char ? literal,
            ' emit call,
    ' (clear-data) call,
            ' exit call,
                   then,
                   else, \ found
                 1 literal,
               ' = call,
                   if,   \ immediate?
         ' execute call,
                   else, \ non-immediate
           ' state call,
               ' @ call,
                   if,   \ compile?
           ' call, call,
                   else, \ interactive
         ' execute call,
                   then,
                   then,
                   then,
                   ret,

0 header, (evaluate) \ internal (non-standard) \ can't be def
                   begin,
      ' parse-name call, \ addr len
             ' dup call,
              ' 0> call,
                   while,
       ' interpret call,
                   repeat,
                   ret,

\ quit ( -- ) ( R: i * x -- ) Empty the return stack, store zero in SOURCE-ID if it is present, make
\      the user input device the input source, and enter interpretation state. Do not display a message.
\      Repeat the following: Accept a line from the input source into the input buffer, set >IN to zero,
\      and interpret. Display the system prompt if in interpretation state, all processing has been
\      completed, and no ambiguous condition exists.
0 header, quit  \ can't be def
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
      ' (evaluate) call,
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

\ 0 header, abort
\     ' (clear-data) call,
\             ' quit jump,
def abort (clear-data) quit ;

\ [ ( -- ) enter interpretation state (immediate word)
$80 header, [  \ can't be def (immediate)
           ' false call,
           ' state call,
               ' ! jump,

\ ] ( -- ) enter compilation state
\ 0 header, ]
\             ' true call,
\            ' state call,
\                ' ! jump,
def ] true state ! ;

\ ; ( -- ) end current definition, make visible in dictionary and enter interpretation state
$80 header, ;  \ can't be def (immediate)
               ' [ call,
            ' ret, call,
                   ret,

\ header, ( "<spaces>name -- ) append header to dictionary (non-standard, note: no flag)
0 header, header,   \ can't be def
          ' latest call,
               ' @ call, \ link to current (soon to be previous) word
            ' here call,
          ' latest call,
               ' ! call, \ update latest to this word
               ' , call, \ append link
      ' parse-name call, ( addr len -- )
             ' dup call,
              ' c, call, \ append len
            ' over call,
               ' + call,
            ' swap call, ( end start -- )
                   do,   \ append name
               ' i call,
              ' c@ call,
              ' c, call,
                   loop,
                   ret,  ( addr len )

\ postpone ( "<spaces>name" -- ) parse and find name, append compilation semantics
$80 header, postpone  \ can't be def (immediate)
      ' parse-name call, \ addr len
        ' >counted call, \ caddr
            ' find call, \ c-addr 0 | xt 1 | xt -1
                 1 literal,
               ' = call, \ only works for immediate words -- TODO: error for not found or non-immediate
                   if,
           ' call, call,
                   then,
                   ret,

\ literal ( x -- ) append run-time semantics of pushing x
$80 header, literal  \ can't be def (immediate)
        ' literal, jump, \ TODO: simply replace literal, ?

\ immediate ( -- ) make most recent definition immediate
0 header, immediate
          ' latest call,
               ' @ call,
                 2 literal,
               ' + call,  \ latest @ cell+ or >name [non-standard]
             ' dup call,
               ' @ call,
               $80 literal,
              ' or call,  \ set immediate flag
            ' swap call,
               ' ! jump,
\ TODO doesn't work as a def for some reason: def immediate latest @ 2 + dup @ 128 or swap ! ;

( --- end of dictionary ------------------------------------------------------ )

                   patch,
    ' (clear-data) call,
         ' decimal call, \ default base
memory-size $500 - literal, \ $ff bytes above stacks
     ' source-addr call,
               ' ! call,
               $ff literal, \ size ($400 bytes for stacks/$ff elements each)
      ' source-len call,
               ' ! call,
\             here literal, \ update dictionary pointer to compile-time position
\             ' dp call,
\              ' ! call,
\         latest @ literal, \ update latest to compile-time
\         ' latest call,
\              ' ! call,
            ' quit jump,

here     ' dp     16 + s! \ update dictionary pointer to compile-time position
latest @ ' latest 16 + s! \ update latest to compile-time