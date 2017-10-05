( shared library )

: 2dup over over ;

: 2drop drop drop ;
: 3drop 2drop drop ;

: min 2dup > if swap then drop ;
: max 2dup < if swap then drop ;
: between rot swap over >= -rot <= and ;

: times 0 do ;

: neg -1 * ;
: abs dup 0 < if neg then ;

: +! dup @ rot + swap ! ;

3.14159265359 const pi
: deg2rad pi 180.0 / * ;
: rad2deg 180.0 pi / * ;
