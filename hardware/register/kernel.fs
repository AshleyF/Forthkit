require assembler.fs

2 constant one     1   one ldc, \ literal  1
3 constant two     2   two ldc, \ literal  2
4 constant four    4  four ldc, \ literal  4
5 constant -four  -4 -four ldc, \ literal -4

: literal, ( val reg -- ) pc two ld+, , ;

( --- stacks ----------------------------------------------------------------- )

: push, ( reg ptr -- ) swap -four st+, ;
: pop,  ( reg ptr -- ) dup dup four add, ld, ;

6 constant d  32764 d literal, \ data stack pointer

: pushd, ( reg -- ) d push, ;
: popd,  ( reg -- ) d pop, ;

7 constant r  32766 r literal, \ return stack pointer

: pushr, ( reg -- ) r push, ;
: popr,  ( reg -- ) r pop, ;

8 constant x

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
  0 do dup i + c@ c, loop ; \ append name

true warnings ! \ intentionally redefining (latest, header,)