( turtle graphics )
( requires: pixels-adapter.f pixels.f )

: c@ _c@ ; ( restore after pixels adapter remapping )
: c! _c! ;

here
255 c, 255 c, 255 c, 255 c, 254 c, 254 c, 254 c, 253 c, 253 c, 252 c,
251 c, 250 c, 249 c, 248 c, 247 c, 246 c, 245 c, 244 c, 242 c, 241 c,
240 c, 238 c, 236 c, 235 c, 233 c, 231 c, 229 c, 227 c, 225 c, 223 c,
221 c, 218 c, 216 c, 214 c, 211 c, 209 c, 206 c, 203 c, 201 c, 198 c,
195 c, 192 c, 189 c, 186 c, 183 c, 180 c, 177 c, 174 c, 170 c, 167 c,
164 c, 160 c, 157 c, 153 c, 149 c, 146 c, 142 c, 138 c, 135 c, 131 c,
127 c, 123 c, 119 c, 115 c, 111 c, 107 c, 103 c,  99 c,  95 c,  91 c,
 87 c,  82 c,  78 c,  74 c,  70 c,  65 c,  61 c,  57 c,  52 c,  48 c,
 43 c,  39 c,  35 c,  30 c,  26 c,  21 c,  17 c,  12 c,   8 c,   3 c,   0 c,  ( TODO <-- should be -1 )
constant table
table .
: cos
  abs 360 mod dup 180 >= if
    360 swap -
  then
  dup 90 >= if
    -1 180 rot -
  else
    1 swap
  then
  table + c@ 1 + *
;

: sin 90 - cos ;

variable x variable y variable theta
variable dx variable dy

: point-x x @ 256 / width 2 / + ;
: point-y y @ 256 / height 2 / + ;
: valid-x? point-x 0 width 1 - within ;
: valid-y? point-y 0 height 1 - within ;
: valid? valid-x? valid-y? and ;
: plot valid? if point-x point-y set then ;

: go 256 * y ! 256 * x ! ;
: head dup theta ! dup cos dx ! sin dy ! ;
: pose head go ;

: start clear 0 0 0 pose ;
: turn theta @ + head ;
: move 0 do dx @ x +! dy @ y +! plot loop ;
: jump dup dx @ * x +! dy @ * y +! ;
