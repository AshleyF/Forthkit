require assembler.fs

2 constant one    1   one ldc, \ literal 1
3 constant two    2   two ldc, \ literal 2
4 constant four   4  four ldc, \ literal 4
5 constant eight  8 eight ldc, \ literal 8

6 constant x
7 constant y
8 constant z
9 constant w \ TODO: c@ c! as secondaries and remove this

: literal, ( val reg -- ) pc two ld+, , ;

( --- stacks ----------------------------------------------------------------- )

: push, ( reg ptr -- ) dup dup four sub, swap st, ;
: pop,  ( reg ptr -- ) four ld+, ;

10 constant d  memory-size 2 + d literal, \ data stack pointer

: pushd, ( reg -- ) d push, ;
: popd,  ( reg -- ) d pop, ;

11 constant r  memory-size r literal, \ return stack pointer

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
            x y st,
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
            x y st,
                ret,

\ + ( y x -- sum ) addition
0 header, +  label 'plus
             x popd,
             y popd,
         x x y add,
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

\ nand ( y x -- not-and ) not and (non-standard)
0 header, nand  label 'nand
             x popd,
             y popd,
         x y x nand,
             x pushd,
               ret,

\ invert ( x -- result ) invert bits
0 header, invert  label 'invert
              x popd,
            x x not,
              x pushd,
                ret,

\ negate ( x -- result ) arithetic inverse (not 1+)
0 header, negate  label 'negate
             x popd,
           x x not,
       x x one add,
             x pushd,
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
          x x y shl,
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
          x x y shr,
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
           d x st,
               ret,

\ over ( y x -- y x y ) copy second stack value to top
0 header, over  label 'over
      x d four add,
           x x ld,
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

\ >r ( x -- ) ( R: -- x ) move x to return stack
0 header, >r  label 'to-r
              x popd,
            y r ld,  \ this return address
            r x st,  \ replace
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