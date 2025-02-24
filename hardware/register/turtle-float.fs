\ turtle graphics library

require pixels.fs

:noname ." Welcome to Forthkit" ; is bootmessage
false warnings !

fvariable x fvariable y fvariable theta \ initialized in start
fvariable dx fvariable dy

width  2/ constant hwidth
height 2/ constant hheight

: valid? ( x y -- b )
  0 height 1- within swap
  0 width  1- within and ;

pi 180e f/ fconstant rads
180e pi f/ fconstant degs ( needed? )

: deg2rad rads f* ;
: rad2deg degs f* ; ( needed? )

: go ( x y -- ) s>f y f! s>f x f! ;

: fhead ( t -- )
  fdup theta f! deg2rad fdup
  fcos dx f!
  fsin dy f! ;
: head ( t -- ) s>f fhead ;

: pose ( x y t -- ) head go ;
: home 0 0 0 pose ;

: start ( -- ) clear home ;
: turn ( a -- ) s>f theta f@ f+ fhead ;
: plot ( x y -- )
  fround f>s hwidth +
  fround f>s hheight + \ x y on *data* stack
  2dup valid? if set else 2drop then ;
: move ( d -- )
  dy f@ dx f@ y f@ x f@ 0 do \ note: on *floating point* stack
    fover fover plot
    2 fpick f+ fswap \ x+=dx
    3 fpick f+ fswap \ y+=dy
  loop
  x f! y f! fdrop fdrop ;

: f+! ( x addr -- ) dup f@ f+ f! ; ( note dup is address on *data* stack )
: jump ( d -- )
  s>f fdup
  dx f@ f* x f+!
  dy f@ f* y f+! ;

  ( drawing things! )

: angle ( sides -- angle ) 360 swap / ;
: draw ( len angle sides -- ) 0 do 2dup turn move loop 2drop ;
: polygon ( len sides -- ) dup angle swap draw ;

: triangle ( len -- )  3 polygon ;
: square   ( len -- )  4 polygon ;
: pentagon ( len -- )  5 polygon ;
: hexagon  ( len -- )  6 polygon ;
: circle   ( len -- ) 36 polygon ;

: shapes start 0 -70 go 50 hexagon 50 pentagon 50 square 50 triangle show ;

: star ( len -- ) 144 5 draw ;

: burst start 60 0 do i 6 * head 0 0 go 80 move loop show ;

: squaral start -70 -35 go 20 0 do 140 move 126 turn loop show ;

: rose start 0 54 0 do 2 + dup move 84 turn loop show ;

: spiral-rec 1 + dup move 92 turn dup 110 < if recurse then ;
: spiral start 1 spiral-rec show ;

\ shim for old version of gforth (e.g. apt install gforth gives 0.7.3)
\ : [: ( -- ) postpone ahead :noname ; immediate compile-only
\ : ;] ( -- xt ) postpone ; ] postpone then latestxt postpone literal ; immediate compile-only

: spin dup angle swap 0 do 2dup turn execute loop 2drop ;
: stars start [: 80 star ;] 3 spin show ;

: spiro start [: 6 circle ;] 20 spin show ;

: arc 0 do 2dup turn move loop 2drop ;
: petal 2 0 do 4 6 16 arc 1 -6 16 arc 180 turn loop ;
: flower start [: petal ;] 15 spin show ;

: demo burst shapes squaral spiro stars rose flower spiral ;
demo

( Kock curve experiment )

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
