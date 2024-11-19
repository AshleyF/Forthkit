( turtle graphics )
( requires: pixels.f )

variable x variable y variable theta ( initialized in start )
variable dx variable dy

: point-x x @ width 2 / + 0.5 + floor ;
: point-y y @ height 2 / + 0.5 + floor ;
: valid-x? point-x 0 width 1 - within ;
: valid-y? point-y 0 height 1 - within ;
: valid? valid-x? valid-y? and ;
: plot valid? if point-x point-y set then ;

3.14159265359 constant pi
pi 180.0 / constant rads
180.0 pi / constant degs
: deg2rad rads * ;
: rad2deg degs * ;

: go y ! x ! ;
: head dup theta ! deg2rad dup cos dx ! sin dy ! ;
: pose head go ;

: start clear 0 0 0 pose ;
: turn theta @ + head ;
: move 0 do dx @ x +! dy @ y +! plot loop ;
: jump dup dx @ * x +! dy @ * y +! ;
