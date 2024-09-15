( test turtle graphics )
( requires: prelude pixels turtle )

: angle 360 swap / ;
: draw -rot times 2dup move turn loop 2drop ;
: polygon dup angle draw ;

: triangle 3 polygon ;
: square   4 polygon ;
: pentagon 5 polygon ;
: hexagon  6 polygon ;
: circle  36 polygon ;

: shapes start 0 -70 go 50 hexagon 50 pentagon 50 square 50 triangle show ;

: star 5 144 draw ;

: spin dup angle swap times 2dup turn call loop 2drop ;
: stars start [: 80 star :] 3 spin show ;

: spiro start [: 4 circle :] 15 spin show ;

: burst start 60 times i 6 * head 0 0 go 80 move loop show ;

: squaral start -70 -35 go 20 times 140 move 126 turn loop show ;

: rose start 0 54 times 2 + dup move 84 turn loop show ;

: arc times 2dup turn move loop 2drop ;
: petal 2 times 4 6 16 arc 1 -6 16 arc 180 turn loop ;
: flower start ' petal 15 spin show ;

: spiral-rec 1 + dup move 92 turn dup 110 < if spiral-rec then ;
: spiral start 1 spiral-rec show ;

burst shapes squaral spiro stars rose flower spiral
