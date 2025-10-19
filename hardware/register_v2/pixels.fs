\ pixel graphics library using Unicode Braille characters

\ : buffer: create allot ; \ TODO: not defined in gforth?!

160 constant width
160 constant height

width 2 / constant columns
width height * 8 / constant size

size buffer: screen
create mask-table 1 c, 8 c, 2 c, 16 c, 4 c, 32 c, 64 c, 128 c,

: clear ( -- )
  screen size + screen do 0 i c! loop ;

: char-cell ( x y -- cell )
  4 / columns * swap 2 / + ;

: mask ( x y -- mask )
  4 mod 2 * swap 2 mod + mask-table + c@ ;

: char-cell-mask ( x y -- cell mask char )
  2dup char-cell -rot mask over screen + c@ ;

: set ( x y -- )
  char-cell-mask or swap screen + c! ;

: get ( x y -- b )
  char-cell-mask and swap drop 0<> ;

: reset ( x y -- )
  char-cell-mask swap invert and swap screen + c! ;

: u>= ( u1 u2 -- flag )
  u< 0= ;

: utf8-emit ( c -- )
    dup 128 < if emit exit then
    0 swap 63 begin 2dup > while 2/ >r dup 63 and 128 or swap 6 rshift r> repeat
    127 xor 2* or begin dup 128 u>= while emit repeat drop ;

: show
  size 0 do
    i columns mod 0= if 10 emit then \ newline as appropriate
    i screen + c@ 10240 or utf8-emit
  loop ;
