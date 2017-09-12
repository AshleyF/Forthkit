( pixel graphics library using Unicode Braile characters )

160 const width
160 const height

width 2 / const columns
width height * 8 / const canvas-size

: clear canvas-size repeat 10240 i m! loop ;

( init dot masks )
canvas-size const dots ( after canvas )
128 64 32 4 16 2 8 1
8 repeat dots i + m! loop

: cell 4 / columns * swap 2 / + ;
: mask 4 mod 2 * swap 2 mod + dots + m@ ;
: cell-mask-dots 2dup cell -rot mask over m@ ;

: set cell-mask-dots or swap m! ;
: reset cell-mask-dots swap not and swap m! ;

: <cr> 10 emit ; ( *nix style -- change as needed )
: newline-as-appropriate i 80 mod 0 = if <cr> then ;
: show canvas-size 0 do newline-as-appropriate i m@ emit loop flush ;