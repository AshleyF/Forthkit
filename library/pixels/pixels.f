( pixel graphics library using Unicode Braille characters )
( requires: prelude )

160 constant width
160 constant height

width 2 / constant columns
width height * 8 / constant size

( init dot masks )
: init-masks 8 0 do size i + b! loop ;
128 64 32 4 16 2 8 1 init-masks 

: clear size 0 do 0 i b! loop ;

: cell 4 / floor columns * swap 2 / floor + ;
: mask 4 mod 2 * swap 2 mod + size + b@ ;
: cell-mask 2dup cell -rot mask over b@ ;

: set cell-mask or swap b! ;
: reset cell-mask swap invert and swap b! ;

: show
  size 0 do
    i columns mod 0 = if 10 emit then  ( newline as appropriate )
    i b@ 10240 or emit
  loop ;
