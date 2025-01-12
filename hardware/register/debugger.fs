require machine.fs
require kernel.fs

: dump-registers ( -- ) registers 16 cells dump ;
: dump-memory ( -- ) here memory - dup . ." bytes" memory swap dump ; \ dump allocated memory (to here)
: dump-stacks ( -- ) d reg @ r reg @ min dup memory + memory-size rot - dump ; \ dump stacks

: run-word ( addr -- ) pc reg ! run ;
: rw run-word ;

here . ." byte kernel" cr

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

label 'parse
          ' refill call,
            ' drop call,
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


label 'test
7 literal,
42 literal,
          ' refill call,
            ' drop call,
                 0 literal,
      ' parse-name call,
         ' >number call,
           ' 2drop call,
                 0 literal,
      ' parse-name call,
         ' >number call,
           ' 2drop call,
               ' ' call,
         ' execute call,
               ' ' call,
         ' execute call,
               ' ' call,
         ' execute call,
               ' ' call,
         ' execute call,
               ' ' call,
         ' execute call,
             ' bye call,

label 'go
            ' quit call,
             ' bye call,

label 'loop
                 0 literal,
                 3 literal,
                   ?do,
               ' i call,
               ' . call,
              ' cr call,
                   loop,
             ' bye call,

run
