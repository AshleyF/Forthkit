require assembler.fs

    2 constant one       1   one ldc, \ literal 1
    3 constant two       2   two ldc, \ literal 2
    4 constant four      4  four ldc, \ literal 4
    5 constant eight     8 eight ldc, \ literal 8
    
    6 constant #t       -1    #t ldc, \ literal true (-1)
 zero constant #f                     \ literal false (0)
 
    7 constant x
    8 constant y
    9 constant z
   10 constant w \ TODO: c@ c! as secondaries and remove this

: literal, ( val reg -- ) pc two ld+, , ;

( --- stacks ----------------------------------------------------------------- )

: push, ( reg ptr -- ) dup dup four sub, st, ;
: pop,  ( reg ptr -- ) four ld+, ;

11 constant d  memory-size 2 + d literal, \ data stack pointer

: pushd, ( reg -- ) d push, ;
: popd,  ( reg -- ) d pop, ;

12 constant r  memory-size r literal, \ return stack pointer

: pushr, ( reg -- ) r push, ;
: popr,  ( reg -- ) r pop, ;

: call, ( addr -- ) pc pushr, jump, ;   \ 6 bytes
: ret, x popr, x x four add, pc x cp, ; \ 8 bytes (pc popr, would complicate calls)

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

                skip, \ skip to interpreter

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
         $ff y literal, 
         x x y and,
             x pushd,
               ret,

\ c! ( val addr -- ) store 8-bit value
0 header, c!  label 'c-store
             x popd,
             y popd,
         $ff z literal, 
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

\ dup ( x -- x x ) duplicate top stack value
0 header, dup  label 'dup
           x d ld,
             x pushd,
               ret,

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

\ swap ( y x -- x y ) swap top two stack values
0 header, swap  label 'swap
           x d ld,
      z d four add,
           y z ld,
           y d st, \ swap in-place
           x z st,
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

\ depth ( -- depth ) data stack depth
0 header, depth  label 'depth
 memory-size x literal,
         x x d sub,
      x x four div,
       x x one add,
             x pushd,
               ret,

\ >r ( x -- ) ( R: -- x ) move x to return stack
0 header, >r  label 'to-r
             x popd,
           y r ld,  \ this return address
           x r st,  \ replace
      y y four add, \ ret,
          pc y cp,
                
\ >r ( -- x ) ( R: x -- ) move x from return stack
0 header, r>  label 'r-from
             x popr, \ this return address
             y popr, \ top value before call
             y pushd,
      x x four add,  \ ret,
          pc x cp,

\ r@ ( -- x ) ( R: x -- x ) copy x from return stack
0 header, r@  label 'r-fetch
             x popr, \ this return address
           y r ld,   \ top value before call
             y pushd,
      x x four add,  \ ret,
          pc x cp,

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

\ < ( y x -- b ) true if y less than x (- 15 rshift negate 1+)
0 header, <  label 'less-than
             x popd,
           y d ld,
         y y x sub, \ negative if less
          15 x ldc,
         y y x shr, \ sign bit to 1s place
           y y not, \ negate
       y y one add,
           y d st,
               ret,

( --- secondaries ------------------------------------------------------------ )

\ \ xor ( y x -- result ) logical/bitwise exclusive or
\ 0 header, xor  label 'xor
\           '2dup call,
\             'or call,
\           '-rot call,
\            'and call,
\         'invert call,
\            'and jump,

( --- interpreter ------------------------------------------------------------ )

               then,
          zero halt,