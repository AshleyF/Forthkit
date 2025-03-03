: buffer: create allot ; \ TODO: not defined in gforth?!

160 constant width
160 constant height

27 constant esc

: show-sixel
  esc emit ." P;1q"
  [char] " emit ." 1;1" \ 1:1 pad:pan ratio (square pixels)
  height 0 do \ TODO: missed bottom rows
    width 0 do
      0
      i j get if 7 or then
      j height 1 - < if
        i j 1 + get if 56 or then
      then
      63 + dup emit dup emit emit
    loop
    [char] - emit
  2 +loop
  esc emit [char] \ emit cr ;

: show-sixel-tiny
  esc emit ." P;1q" \ transparent zero pixel value (also, default 0 aspect ratio -- override below)
  [char] " emit ." 1;1" \ 1:1 pad:pan ratio (square pixels)
  height 0 do \ TODO: missed bottom rows
    width 0 do
      0
      i j     get if  1 or then
      j height 1 - < if
        i j 1 + get if  2 or then
        j height 2 - < if
          i j 2 + get if  4 or then
          j height 3 - < if
            i j 3 + get if  8 or then
            j height 4 - < if
              i j 4 + get if 16 or then
              j height 5 - < if
                i j 5 + get if 32 or then
              then
            then
          then
        then
      then
      63 + emit
    loop
    [char] - emit
  6 +loop
  esc emit [char] \ emit cr ;

\ : show show-sixel ; \ replace `show`
: show show-sixel-tiny ; \ replace `show`
