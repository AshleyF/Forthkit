require assembler.fs

2 constant one     1   one ldc, \ literal  1
3 constant two     2   two ldc, \ literal  2
4 constant four    4  four ldc, \ literal  4
5 constant -four  -4 -four ldc, \ literal -4

6 constant x
7 constant y
8 constant z
9 constant t

: literal, ( val reg -- ) pc two ld+, , ;

( --- stacks ----------------------------------------------------------------- )

: push, ( reg ptr -- ) swap -four st+, ;
: pop,  ( reg ptr -- ) dup dup four add, ld, ;

10 constant d  $8000 2 - d literal, \ data stack pointer

: pushd, ( reg -- ) d push, ;
: popd,  ( reg -- ) d pop, ;

11 constant r  $8000 4 - r literal, \ return stack pointer

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

\ bye ( -- ) halt machine
0 header, bye
              0 halt,

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
            t x ld,   \ existing value
          t t z and,  \ mask to upper
          y y t or,   \ combine
            t y st,
                ret,

( --- interpreter ------------------------------------------------------------ )

               then,
          zero halt,