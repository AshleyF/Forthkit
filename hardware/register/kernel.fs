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
: ret, x popr,  x x four add,  pc x cp, ; \ 8 bytes (pc popr, would complicate calls)

( --- control-flow ----------------------------------------------------------- )

: if, ( C: -- addr ) x popd,  0 y ldv,  here 2 -  pc y x cp?, ; \ dummy branch on 0, push pointer to address
: else, ( C: addr -- addr ) branch, swap then, ;  \ patch previous branch to here, dummy unconditionally branch over false block

: begin, ( C: -- addr ) here memory - ;
: again, ( C: addr -- ) jump, ;
: until, ( C: addr -- ) if, s! ; \ branch on 0 to address

( --- dictionary ------------------------------------------------------------- )

false warnings ! \ intentionally redefining (latest header,)

variable latest  0 latest !

: header, ( flag "<spaces>name" -- )
  latest @ \ link to current (soon to be previous) word
  here latest ! \ update latest to this word
  memory - , \ append link, relative to memory buffer
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
 memory-size x ldv,
         x x d sub,
      x x four div,
       x x one add,
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

( --- secondaries ------------------------------------------------------------ )

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

: do, ( C: -- addr ) \ begin do-loop (2>r)
     'two-to-r call,
               begin, ;

: loop, ( C: addr -- ) \ end do-loop (r> 1+ r@ over >r < if again then 2r> 2drop)
       'r-from call, ( start )
     'one-plus call,
      'r-fetch call,
         'over call,
         'to-r call,
    'less-than call,
               if,
               swap again, \ swap if address
               then,
   'two-r-from call,
     'two-drop call, ;

( --- memory abstractions ---------------------------------------------------- )

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
0 header, cell+  label 'cell+
             2 literal,
         'plus jump,

\ chars ( x -- n-chars ) size in address units of n-chars (no-op)
0 header, chars  label 'chars
               ret, \ no-op

\ char+ ( addr -- addr ) add size of char to address (1+)
0 header, char+  label 'char-plus
     'one-plus jump,

( --- interpreter ------------------------------------------------------------ )

\ bl ( -- c ) space character value (32 constant bl)
0 header, bl  label 'bl
          32 x ldc,
             x pushd,
               ret,

\ space ( -- ) emit space character (bl emit)
0 header, space  label 'space
           'bl call,
         'emit jump,

\ cr ( -- ) cause newline (10 emit)
0 header, cr  label 'c-r
            10 literal,
         'emit jump,

( --- end of dictionary ------------------------------------------------------ )

               then,
          zero halt,