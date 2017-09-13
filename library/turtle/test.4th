( test turtle graphics )
( requires: prelude pixels turtle )

: edge turn move ;

: polygon repeat 2dup edge loop 2drop ;
: slices 360 over / swap ;
: spin slices repeat 2dup turn call loop 2drop ;
: regpoly slices polygon ;

: triangle 3 regpoly ;
: square   4 regpoly ;
: pentagon 5 regpoly ;
: hexagon  6 regpoly ;
: circle  36 regpoly ;

: star 144 5 polygon ;

: shapes begin 0 -70 go 50 hexagon 50 pentagon 50 square 50 triangle show ;

: burst begin 60 repeat i 6 * head 0 0 go 80 move loop show ;

: squaril begin -70 -35 go 20 repeat 140 move 126 turn loop show ;

: sm-circle 4 circle ;
: spiro begin ' sm-circle 15 spin show ;

: sm-star 80 star ;
: stars begin ' sm-star 3 spin show ;

: rose begin 0 54 repeat 2 + dup move 84 turn loop show ;

: arc repeat 2dup turn move loop 2drop ;
: petal 2 repeat 4 6 16 arc 1 -6 16 arc 180 turn loop ;
: flower begin ' petal 15 spin show ;

: spiral-rec 1 + dup move 92 turn dup 110 < if spiral-rec then ;
: spiral begin 1 spiral-rec show ;

burst shapes squaril spiro stars rose flower spiral

