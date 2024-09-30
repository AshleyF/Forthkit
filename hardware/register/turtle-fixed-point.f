( turtle graphics )
( requires: pixels-adapter.f pixels.f )

: ,, swap dup 1+ -rot c! ;

here dup
91 allot
constant table
255 ,, 255 ,, 255 ,, 255 ,, 254 ,, 254 ,, 254 ,, 253 ,, 253 ,, 252 ,,
251 ,, 250 ,, 249 ,, 248 ,, 247 ,, 246 ,, 245 ,, 244 ,, 242 ,, 241 ,,
240 ,, 238 ,, 236 ,, 235 ,, 233 ,, 231 ,, 229 ,, 227 ,, 225 ,, 223 ,,
221 ,, 218 ,, 216 ,, 214 ,, 211 ,, 209 ,, 206 ,, 203 ,, 201 ,, 198 ,,
195 ,, 192 ,, 189 ,, 186 ,, 183 ,, 180 ,, 177 ,, 174 ,, 170 ,, 167 ,,
164 ,, 160 ,, 157 ,, 153 ,, 149 ,, 146 ,, 142 ,, 138 ,, 135 ,, 131 ,,
127 ,, 123 ,, 119 ,, 115 ,, 111 ,, 107 ,, 103 ,,  99 ,,  95 ,,  91 ,,
 87 ,,  82 ,,  78 ,,  74 ,,  70 ,,  65 ,,  61 ,,  57 ,,  52 ,,  48 ,,
 43 ,,  39 ,,  35 ,,  30 ,,  26 ,,  21 ,,  17 ,,  12 ,,   8 ,,   3 ,,   0 ,,  ( TODO <-- should be -1 )

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
