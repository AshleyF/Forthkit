( turtle graphics )
( requires: pixels.4th )

var x var y var theta
var dx var dy

: point-x x @ width 2 / + 0.5 + floor ;
: point-y y @ height 2 / + 0.5 + floor ;
: valid-x? point-x 0 width 1 - between ;
: valid-y? point-y 0 height 1 - between ;
: valid? valid-x? valid-y? and ;
: plot valid? if point-x point-y set then ;

: go y ! x ! ;
: head dup theta ! deg2rad dup cos dx ! sin dy ! ;
: pose head go ;

: start clear 0 0 0 pose ;
: turn theta @ + head ;
: move times dx @ x +! dy @ y +! plot loop ;
: jump dup dx @ * x +! dy @ * y +! ;
