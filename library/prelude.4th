( shared library )

: 2dup dup dup ;
: 3dup 2dup dup ;

: 2drop drop drop ;
: 3drop 2drop drop ;

: min 2dup > if swap then drop ;
: max 2dup < if swap then drop ;
: between rot swap over >= -rot <= and ;

: repeat 0 do ; ( TODO: move to prelude )

: neg -1 * ;
: abs dup 0 < if neg then ;

: +! dup @ rot + swap ! ;

