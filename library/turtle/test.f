( test turtle graphics )
( requires: prelude pixels turtle )

: angle 360 swap / ;
: draw -rot 0 do 2dup move turn loop 2drop ;
: polygon dup angle draw ;

: triangle 3 polygon ;
: square   4 polygon ;
: pentagon 5 polygon ;
: hexagon  6 polygon ;
: circle  36 polygon ;

: shapes start 0 -70 go 50 hexagon 50 pentagon 50 square 50 triangle show ;

: star 5 144 draw ;

: spin dup angle swap 0 do 2dup turn call loop 2drop ;
: stars start [: 80 star :] 3 spin show ;

: spiro start [: 4 circle :] 15 spin show ;

: burst start 60 0 do i 6 * head 0 0 go 80 move loop show ;

: squaral start -70 -35 go 20 0 do 140 move 126 turn loop show ;

: rose start 0 54 0 do 2 + dup move 84 turn loop show ;

: arc 0 do 2dup turn move loop 2drop ;
: petal 2 0 do 4 6 16 arc 1 -6 16 arc 180 turn loop ;
: flower start [: petal :] 15 spin show ;  ( TODO ' petal instead of [: petal :] )

: spiral-rec 1 + dup move 92 turn dup 110 < if recurse then ;
: spiral start 1 spiral-rec show ;

: demo burst shapes squaral spiro stars rose flower spiral ;
demo

(
variable 'koch
: curve dup 0 > if 2dup 1 - swap 3 / swap 'koch @ execute else drop move then ;
: koch 2dup curve -60 turn 2dup curve 120 turn 2dup curve -60 turn 2dup curve 2drop ;
' koch 'koch !

start
-80 0 go
50 1 curve
show

start
-80 0 go
100 2 curve
show

start
-80 0 go
200 3 curve
show

start
-80 0 go
400 4 curve
show

: snowflake 3 0 do 2dup curve 120 turn loop 2drop ;

start
-80 0 go
80 0 snowflake
show

start
-80 0 go
50 1 snowflake
show

start
-80 0 go
50 2 snowflake
show
)