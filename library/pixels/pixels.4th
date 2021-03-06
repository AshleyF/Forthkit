( pixel graphics library using Unicode Braile characters )
( requires: prelude )

160 const width
160 const height

width 2 / const columns
width height * 8 / const canvas-size

: clear canvas-size times 10240 i m! loop ;

( init dot masks )
canvas-size const dots ( after canvas )
128 64 32 4 16 2 8 1  8 times dots i + m! loop

: cell 4 / columns * swap 2 / + ;
: mask 4 mod 2 * swap 2 mod + dots + m@ ;
: cell-mask-dots 2dup cell -rot mask over m@ ;

: set cell-mask-dots or swap m! ;
: reset cell-mask-dots swap not and swap m! ;

: newline-as-appropriate i 80 mod 0 = if 10 emit then ;
: show canvas-size times newline-as-appropriate i m@ emit loop flush ;

