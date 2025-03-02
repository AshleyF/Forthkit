: buffer: create allot ; \ TODO: not defined in gforth?!

160 constant width
160 constant height

27 constant esc

variable r  100 r !
variable g    0 g !
variable b    0 b !
: next-color
       r @ 100 =  b @   0 =  and  g @ 100 <  and if  1 g +!
  else g @ 100 =  b @   0 =  and  r @   0 >  and if -1 r +!
  else r @    0=  g @ 100 =  and  b @ 100 <  and if  1 b +!
  else r @    0=  b @ 100 =  and  g @   0 >  and if -1 g +!
  else g @    0=  b @ 100 =  and  r @ 100 <  and if  1 r +!
  else r @ 100 =  g @   0 =  and  b @   0 >  and if -1 b +!
  then then then then then then ;
: emit-num s>d <# #s #> type ;
: set-color 
  ." #0;2;"
  r @ emit-num [char] ; emit
  g @ emit-num [char] ; emit
  b @ emit-num
  next-color ;

: home-cursor esc emit ." [H" ;
: clear esc emit ." [40m" esc emit ." [2J" home-cursor ;
: set ( x y -- )
  home-cursor
  esc emit ." P;1q"
  [char] " emit ." 1;1" \ 1:1 pad:pan ratio (square pixels)
  set-color
  dup 6 / 0 ?do [char] - emit loop \ to line
  swap [char] ! emit emit-num 63 emit
  \ swap 0 ?do 63 emit loop \ to column \ TODO: use repeat protocol
  6 mod 2 swap power 63 + \ sixel
  emit
  esc emit [char] \ emit
  ; \ 10000 0 do loop ;

: show 100 0 do 10000 0 do loop loop ; \ pause
\ : show ;
