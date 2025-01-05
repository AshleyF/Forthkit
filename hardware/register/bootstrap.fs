0 header, : ] 0 header, ] ;

: [char] parse-name drop c@ postpone literal ; immediate

: ( [char] ) parse 2drop ; immediate
: \ 10 ( newline ) parse 2drop ; immediate

: create
  0 header,
   x pc cp,  \ x=pc
   14 y ldc, \ y=14
  x x y add, \ x+=y
;

(                0 literal, )
(        ' header, call, )
(             3137 literal, ' , call, \ 410C -> cp? zero pc x   x=pc )
(             3613 literal, ' , call, \ 1D0E -> ldc y 14        y=14 )
(           -13219 literal, ' , call, \ 5DCC -> add y x x       x=y+x )
(           -14283 literal, ' , call, \ 35C8 -> st+ -four x d   push x )
(           -26282 literal, ' , call, \ 5699 -> add four r r    ret )
(           -27871 literal, ' , call, \ 2193 -> ld+ zero r t )
(            13142 literal, ' , call, \ 5633 -> add four t t )
(            12353 literal, ' , jump, \ 4130 -> cp? zero t pc )
