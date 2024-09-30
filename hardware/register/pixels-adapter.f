here
3208 allot ( 80×40+8 )
constant buffer

: c@ buffer + c@ ;
: c! buffer + c! ;

: floor ; ( integer math already )

: sym 0 begin key swap 1+ swap dup literal 32 = until literal ; immediate
