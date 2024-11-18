make : ] make ] ;

: cell 2 ;
: cell+ cell + ;
: cells cell * ;

: variable create 1 cells allot ;
: constant create , does> @ ;

: point create , , does> dup cell+ @ swap @ ;

: [char] parse-name drop c@ postpone literal ; immediate
: ( [char] ) parse 2drop ; immediate

( now we can use comments! )
