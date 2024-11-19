make : ] make ] ;

: constant create , does> @ ;

32 constant bl
2 constant cell

: cell+ cell + ;
: cells cell * ;

: variable create 1 cells allot ;

: point create , , does> dup cell+ @ swap @ ;

: [char] parse-name drop c@ postpone literal ; immediate

: ( [char] ) parse 2drop ; immediate
: \ 10 ( newline ) parse  2drop ; immediate

( now we can use comments like this! )
\ or like this to the end of the line!
