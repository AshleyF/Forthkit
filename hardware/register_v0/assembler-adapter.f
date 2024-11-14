20000 constant buffer
10000 constant bufsize

: c! buffer + c! ;
: ! buffer + ! ;

: write rot buffer + -rot write ;
