( turtle graphics )
( requires: pixels.4th )

var x var y var theta
var dx var dy

: point-x x @ width 2 / + 0.5 + floor ;
: point-y y @ height 2 / + 0.5 + floor ;
: valid-x point-x 0.5 width 1.0 - between ;
: valid-y point-y 0.5 height 1.0 - between ;
: valid valid-x valid-y and ;
: plot valid if point-x point-y set then ;

: go y ! x ! ;
: head dup theta ! deg2rad dup cos dx ! sin dy ! ;
: pose head go ;

: begin clear 0 0 0 pose ;
: turn theta @ + head ;
: move times dx @ x +! dy @ y +! plot loop ;
: jump times dx @ x +! dy @ y +! loop ;
