make : ] make ] ;

: cell 2 ;
: cell+ cell + ;
: cells cell * ;

: variable create 1 cells allot ;
: constant create , does> @ ;

: point create , , does> dup cell+ @ swap @ ;
