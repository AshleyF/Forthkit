( turtle graphics )
( requires: pixels.4th )

var x var y var theta var dx var dy

: point-x x @ width 2 / + 0.5 + int ;
: point-y y @ height 2 / + 0.5 + int ;
: valid-x point-x 0.5 width 1 - between ;
: valid-y point-y 0.5 height 1 - between ;
: valid valid-x valid-y and ;
: plot valid if point-x point-y set then ;

: go y ! x ! ;
: head dup theta ! deg2rad dup cos dx ! sin dy ! ;
: pose head go ;

: begin clear 0 0 0 pose ;
: turn theta @ + head ;
: move repeat dx @ x +! dy @ y +! plot loop ;
: jump repeat dx @ x +! dy @ y +! loop ;

