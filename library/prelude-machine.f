( shared library - machine prelude )

: +! dup @ rot + swap ! ;

: 2drop drop drop ;

: within rot swap over >= -rot <= and ;
