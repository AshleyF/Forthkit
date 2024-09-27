( shared library )

: 2dup over over ;

: 2drop drop drop ;

: min 2dup > if swap then drop ;
: max 2dup < if swap then drop ;
: within rot swap over >= -rot <= and ;

: negate -1 * ;
: abs dup 0 < if negate then ;

: +! dup @ rot + swap ! ;

: /mod 2dup / -rot mod ;

: factorial dup 1 > if dup 1- factorial * then ;
