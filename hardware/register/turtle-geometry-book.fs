\ --- turtle geometry adapter ---

variable pen true pen !
: pendown true pen ! ;
: penup false pen ! ;

: start clear 0 0 -90 pose pendown ;

: forward pen @ if move else jump then ;
: back 180 turn move 180 turn ;
: right turn ;
: left -1 * turn ;

: iterate 0 postpone literal postpone do ; immediate

\ --- turtle geometry examples ---

: turtle 14
  plot dup 2/ jump
  166 turn dup move
  104 turn dup 2/ move \ really 0.5176 *
  104 turn dup move
  -14 turn 2/ negate jump ;

: p4
  start
  50 forward turtle
  90 right 75 forward 45 left turtle
  50 back turtle
  45 left penup 50 forward turtle
  show ;

: square0 
  50 forward 90 right
  50 forward 90 right
  50 forward 90 right
  50 forward 90 right ;

: square1 
  4 iterate
    50 forward 90 right
  loop ;

: square ( size -- )
  4 iterate
    dup forward 90 right
  loop drop ;

: p5 start -80 -30 go square0 -40 0 go square1 20 20 go 60 0 do i square 15 +loop show ;

: square-piece ( size -- )
  forward 90 right ;

: square ( size -- )
  4 iterate
    dup square-piece
  loop drop ;

: rectangle ( side1 side2 -- )
  2 iterate
    2dup square-piece square-piece
  loop 2drop ;

: rectangle ( side1 side2 -- )
  2dup 2 iterate
    square-piece square-piece
  loop ;

: p6 start -80 -40 go 10 square 20 0 go 10 20 rectangle show ;

: try-angle ( size -- )
  3 iterate
    dup forward 60 right
  loop drop ;

: triangle ( size -- )
  3 iterate
    dup forward 120 right
  loop drop ;

: p7 start -80 0 go 30 try-angle turtle home 30 triangle show ;

: house0 ( size -- )
  dup square triangle ;

: house ( size -- )
  dup square dup forward 30 right triangle ;

: p8 start -80 0 go 30 house0 0 0 -90 pose 30 house show ;

: thing
  30 forward 90 right
  30 forward 90 right
  10 forward 90 right
  10 forward 90 right
  30 forward 90 right
   8 forward 90 right
   8 forward 90 right
  10 forward ;

: things0 4 iterate thing loop ;

: things1 9 iterate thing 10 right 10 forward loop ;

: things2 8 iterate thing 45 left 30 forward loop ;

: p9 start thing show
     start things0 show
     start things1 show
     start things2 show ;

: circle 360 iterate 1 forward 1 right loop ;

: arcr ( r deg -- ) 6 / iterate dup forward 6 right loop drop ; \ 6x step, tighter
: arcl ( r deg -- ) 6 / iterate dup forward 6 left loop drop ;

: p10 start -50 0 go circle home 4 90 arcr home 4 90 arcl show ;

: circles 9 iterate 4 360 arcr 40 right loop ;

: petal ( size -- )
  dup 60 arcr 120 right
      60 arcr 120 right ;

: flower ( size -- ) 
  6 iterate
    dup petal 60 right
  loop drop ;

: ray ( r -- )
  2 iterate
    dup 90 arcl
    dup 90 arcr
  loop drop ;

: sun ( size -- )
  9 iterate
    dup ray
    160 right
  loop drop ;

: p12 start circles show
      start 8 flower show
      start 45 65 go 3 sun show ;

: go demo
  p4 p5 p6 p7 p8 p9 p10 p12
; go
