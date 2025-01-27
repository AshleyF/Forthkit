65536 constant memory-size

0 dp ! \ begin image

 1    one ldc, \ literal 1
 2    two ldc, \ literal 2
 4   four ldc, \ literal 4
 8  eight ldc, \ literal 8
12 twelve ldc, \ literal 12
-1     #t ldc, \ literal true (-1)

memory-size r ldv, \ return stack pointer

branch, \ skip dictionary

: (clear-data) [ memory-size 2 + d ldv, ] ;
: (clear-return) [
                 x popr,
     memory-size r ldv,
          x x four add, \ ret,
              pc x cp,

: decimal 10 base ! ;

: quit ;

patch,

] (clear-data) decimal quit ( TODO jump ) [

\ here     ' dp     16 + s! \ update dictionary pointer to compile-time position
\ latest @ ' latest 16 + s! \ update latest to compile-time

here .
0 here 0 write-block
\ bye