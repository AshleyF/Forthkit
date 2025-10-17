$8000 constant memory-size
memory-size buffer: memory  memory memory-size erase
variable h  memory h !

false warnings ! \ redefining (here c, , !)

: here h @ memory - ;
: c, ( c -- ) h @ c! 1 h +! ;
: , ( cc -- ) dup 8 rshift c, c, ; \ 16-bit big endian
: s! ( v a --) memory + over 8 rshift over c! 1+ c! ; \ 16-bit big endian relative to memory

true warnings !

: halt,  (     n --     )  $80 c, ;
: lit8,  (     n --     )  $81 c, c, ;
: lit16, (     n --     )  $82 c, , ;
: in,    (       -- x   )  $83 c, ;
: out,   (     x --     )  $84 c, ;
: dup,   (     x -- x x )  $85 c, ;
: ret,   (       --     )  $ff c, ;

( --- secondary instructions ------------------------------------------------- )

: call,  (     a --   ) $8000 or , ;
: jump,  (     a --   ) call, ret, ;

( --- assembler tools -------------------------------------------------------- )

: label ( -- addr ) here constant ; \ current address within memory

: segment, here 0 , ; \ leave dummy header address
: pack, here over - swap s! ;
: execute, here over - $8000 or swap s! ;


: write-image
  s" image.bin" w/o create-file throw >r
  memory here r@ write-file throw
  r> close-file throw ;