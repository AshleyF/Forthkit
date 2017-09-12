( : over ; )
( : rot ; )
( : nip swap drop ; )
( : -rot rot rot ; )
( : tuck swap over ; )

( : sq dup * ; )

: 2dup over over ;
: 3dup 2dup dup ;
: 2drop drop drop ;
: 3drop 2drop drop ;

: min 2dup > if swap then drop ; ( not used? )
: max 2dup < if swap then drop ; ( not used? )
: between rot swap over >= -rot <= and ;
: +! dup @ rot + swap ! ;
3.14159265359 const pi
pi 2 * const 2pi ( not used? )
pi 180 / const d2r
: deg2rad d2r * ;

: repeat 0 do ;

: neg -1 * ;
: abs dup 0 < if neg then ;
