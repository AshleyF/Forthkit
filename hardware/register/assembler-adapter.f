20000 constant buffer
10000 constant bufsize

: b! buffer + c! ;
: m! buffer + ! ;

( : dump 123 . . . 456 . buffer + swap bufsize - swap . . dump ; )
: dump buffer + swap buffer + swap dump ;
