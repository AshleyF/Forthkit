20000 constant buffer
10000 constant bufsize

: b! buffer + c! ;
: m! buffer + ! ;

: write rot buffer + -rot write ;
