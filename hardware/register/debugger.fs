require machine.fs
require kernel.fs

: dump-registers ( -- ) registers 16 cells dump ;
: dump-memory ( -- ) here memory - dup . ." bytes" memory swap dump ; \ dump allocated memory (to here)
: dump-stacks ( -- ) d reg @ r reg @ min dup memory + memory-size rot - dump ; \ dump stacks

: run-word ( addr -- ) pc reg ! run ;
: rw run-word ;

here . ." byte kernel"

\ label 'dot
\                  x popd,
\               48 y ldc,
\              x x y add,
\                  x out,
\                    ret,

label 'hello
            ' here call,
               104 literal, \ h
              ' c, call,
               101 literal, \ e
              ' c, call,
               108 literal, \ l
              ' c, call,
               108 literal, \ l
              ' c, call,
               111 literal, \ o
              ' c, call,
                 5 literal, \ length
            ' type call,
             ' bye call,

label 'test
              -123 literal,
               ' . call,
              -123 literal,
              ' u. call,
               682 literal,
          ' binary call,
               ' . call,
          ' unused call,
         ' decimal call,
               ' . call,
          ' refill call,
             \  $20 literal,
            \ char x literal,
           ' parse-name call,
' cr call,
' cr call,
' 2dup call,
' u. call,
' u. call,
' >in call,
' @ call,
' u. call,
' type call,
               \ $20 literal,
            \ char x literal,
           ' parse-name call,
' cr call,
' cr call,
' 2dup call,
' u. call,
' u. call,
' >in call,
' @ call,
' u. call,
' type call,
             ' bye call,

run