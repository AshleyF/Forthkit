\ pixel graphics library using Unicode Braille characters

160 constant width
160 constant height

width 2 / constant columns
width height * 8 / constant size

create screen size allot
create mask-table 1 c, 8 c, 2 c, 16 c, 4 c, 32 c, 64 c, 128 c,

: clear ( -- )
  screen size + screen do 0 i c! loop ;

: char-cell ( x y -- cell )
  4 / columns * swap 2 / + ;

: mask ( x y -- mask )
  4 mod 2 * swap 2 mod + mask-table + c@ ;

: char-cell-mask ( x y --  )
  2dup char-cell -rot mask over screen + c@ ;

: set ( x y -- )
  char-cell-mask or swap screen + c! ;

: reset ( x y -- )
  char-cell-mask swap invert and swap screen + c! ;

: utf8-emit ( c -- )
    dup $80 < if emit exit then
    0 swap $3f begin 2dup > while 2/ >r dup $3f and $80 or swap 6 rshift r> repeat
    $7f xor 2* or begin dup $80 u>= while emit repeat drop ;

: show
  size 0 do
    i columns mod 0= if 10 emit then ( newline as appropriate )
    i screen + c@ 10240 or utf8-emit
  loop ;

: test ( -- )
  clear
  130 30 do
    i  30 set
    i 130 set
     30 i set
    130 i set
  loop
  show ;