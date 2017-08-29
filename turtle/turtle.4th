( turtle graphics )

3.14159265359 const pi
pi 180 / const d2r
: deg2rad d2r * ;

var x var y

: go y ! x ! ;

width  2 / const width/2
height 2 / const height/2
width  1 - const width-1
height 1 - const height-1

: point-x x @ width/2 + 0.5 + int ;
: point-y y @ height/2 + 0.5 + int ;

: valid-x point-x 0.5 width-1 between ;
: valid-y point-y 0.5 height-1 between ;
: valid valid-x valid-y and ;
: plot valid if point-x point-y set then ;

var theta var dx var dy
: theta-rad theta @ deg2rad ;
: update-dx theta-rad cos dx ! ;
: update-dy theta-rad sin dy ! ;
: update update-dx update-dy ;
update

: head theta ! update ;
: pose head go ;

: begin clear 0 0 0 pose ;

: turn theta +! update ;

: step-x dx @ x +! ;
: step-y dy @ y +! ;
: step step-x step-y ;

: move2 repeat step plot loop ;

: seg ( err ) dup 0.5 >= if plot 1 y +! 1 - seg then ;

: bresenham ( y' x' )
    dup x @ -   ( y' x' diffx )
    dup 0.0 > if
        rot y @ -   ( x' diffx diffy )
        swap /           ( x' derror TODO: handle zero )
    else drop 160.0
    then
    0.0 rot     ( derror err x' )
    1 + x @     ( derror err x' x )
    do          ( derror err )
        i x ! plot
        over    ( derror err derror )
        +       ( derror err' )
        seg
    loop
    ;

: swapvar ( v x ) dup @ -rot ! ;

: line2 ( y' x' )
    dup x @ < if x swapvar swap y swapvar swap then
    bresenham ;

: move3 ( n ) dx @ over * x @ + swap dy @ * y @ + line2 ;

: move move2 ;

: draw                      ( m bv av a' a )
    plot
    1 + swap 1 + swap
    do
    i                       ( m bv av i )
    over !                  ( m bv av )
    -rot                    ( av m bv )
    2dup                    ( av m bv m bv )
    +!                      ( av m bv )
    rot                     ( m bv av )
    plot
    loop 3drop ;

: switchx                   ( x' y' m dx )
    0 < if                  ( x' y' m )
        -rot                ( m x' y' )
        y !                 ( m x' )
        x @ swap x !        ( m x )
    else                    ( x' y' m )
        -rot drop           ( m x' )
    then
    ;

: switchy                   ( x' y' m dy )
    0 < if                  ( x' y' m )
        -rot                ( m x' y' )
        swap x !            ( m y' )
        y @ swap y !        ( m y )
    else                    ( x' y' m )
        -rot swap drop      ( m y' )
    then
    ;

: line ( x' y' )
    2dup y @ - swap x @ -   ( x' y' dy dx )
    2dup abs swap abs       ( x' y' dy dx adx ady )
    > if                    ( x' y' dy dx )
        swap over           ( x' y' dx dy dx )
        float /             ( x' y' dx m )
        swap                ( x' y' m dx )
        switchx             ( m x' )
        y swap              ( m yv x' )
        x swap              ( m yv xv x' )
        x @                 ( m yv xv x' x )
    else                    ( x' y' dy dx )
        over                ( x' y' dy dx dy )
        float /             ( x' y' dy m )
        swap                ( x' y' m dy )
        switchy             ( m y' )
        x swap              ( m xv y' )
        y swap              ( m xv yv y' )
        y @                 ( m xv yv y' y )
    then
    draw
    ;

: move ( s )
    dup ( s s )
    dx @ * x @ + ( s sx )
    swap dy @ * y @ + ( x' y' )
    2dup
    line
    y ! x !
    ;

: move move2 ;

