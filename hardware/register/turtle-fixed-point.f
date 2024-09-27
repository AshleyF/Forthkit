( turtle graphics )
( requires: pixels-adapter.f pixels.f )

: ,, swap dup 1+ -rot ! ;

here dup
91 allot
constant table
256 ,, 256 ,, 256 ,, 256 ,, 255 ,, 255 ,, 255 ,, 254 ,, 254 ,, 253 ,,
252 ,, 251 ,, 250 ,, 249 ,, 248 ,, 247 ,, 246 ,, 245 ,, 243 ,, 242 ,,
241 ,, 239 ,, 237 ,, 236 ,, 234 ,, 232 ,, 230 ,, 228 ,, 226 ,, 224 ,,
222 ,, 219 ,, 217 ,, 215 ,, 212 ,, 210 ,, 207 ,, 204 ,, 202 ,, 199 ,,
196 ,, 193 ,, 190 ,, 187 ,, 184 ,, 181 ,, 178 ,, 175 ,, 171 ,, 168 ,,
165 ,, 161 ,, 158 ,, 154 ,, 150 ,, 147 ,, 143 ,, 139 ,, 136 ,, 132 ,,
128 ,, 124 ,, 120 ,, 116 ,, 112 ,, 108 ,, 104 ,, 100 ,,  96 ,,  92 ,,
 88 ,,  83 ,,  79 ,,  75 ,,  71 ,,  66 ,,  62 ,,  58 ,,  53 ,,  49 ,,
 44 ,,  40 ,,  36 ,,  31 ,,  27 ,,  22 ,,  18 ,,  13 ,,   9 ,,   4 ,,   0 ,,

: cos
  abs 360 mod dup 180 >= if
    360 swap -
  then
  dup 90 >= if
    -1 180 rot -
  else
    1 swap
  then
  table + @ *
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
