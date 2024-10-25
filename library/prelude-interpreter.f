( shared library - interpreter prelude )

: 2dup over over ;
: 2drop drop drop ;

: dup 0 pick ;
: over 1 pick ;
: swap 1 roll ;
: rot 2 roll ;
: -rot rot rot ;

: invert dup nand ;
: and nand invert ;
: or invert swap invert nand ;
: xor 2dup and invert -rot or and ;
: nor or invert ;
: xnor xor invert ;

: min 2dup > if swap then drop ;
: max 2dup < if swap then drop ;
: within rot swap over >= -rot <= and ;

: negate -1 * ;
: abs dup 0 < if negate then ;

: +! dup @ rot + swap ! ;

: 0= 0 = ;
: 0<> 0 <> ;
: 0< 0 < ;
: 0> 0 > ;

: /mod 2dup / -rot mod ;

: factorial dup 1 > if dup 1- recurse * then ;
