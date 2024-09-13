( pixel graphics library using Unicode Braille characters )
( requires: prelude )

160 const width
160 const height

width 2 / const columns
width height * 8 / const size

( init dot masks )
128 64 32 4 16 2 8 1  8 times size i + m! loop

: clear size times 10240 i m! loop ;

: cell 4 / floor columns * swap 2 / floor + ;
: mask 4 mod 2 * swap 2 mod + size + m@ ;
: cell-mask 2dup cell -rot mask over m@ ;

: set cell-mask or swap m! ;
: reset cell-mask swap not and swap m! ;

: show
  size times
    i columns mod 0 = if 10 emit then  ( newline as appropriate )
    i m@ emit
  loop ;
