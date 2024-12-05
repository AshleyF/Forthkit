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
             'here call,
               104 literal, \ h
          'c-comma call,
               101 literal, \ e
          'c-comma call,
               108 literal, \ l
          'c-comma call,
               108 literal, \ l
          'c-comma call,
               111 literal, \ o
          'c-comma call,
                 5 literal, \ length
             'type call,
              'bye call,

label 'foo
-123 literal,
'dot call,
              'bye call,

label 'baz
              'pad call,
                64 literal, \ arbitrary distance away
             'plus call,
               'np call,
            'store call,
               104 literal, \ h
'dupe call,
 'dot call,
               'np call,
            'fetch call,
'dupe call,
 'dot call,
          'c-store call,
              'bye call,

label 'test
               123 literal,
                 0 literal,
'less-number-sign call,
               111 literal, \ o
             'hold call,
               108 literal, \ l
             'hold call,
                -7 literal,
             'sign call,
               108 literal, \ l
             'hold call,
               101 literal, \ e
             'hold call,
               104 literal, \ h
             'hold call,
                33 literal, \ !
             'hold call,
'number-sign-s call,
'number-sign-gtr call,

'two-dupe call,
'less-number-sign call,
            'holds call,
                32 literal,
             'hold call,
            'holds call,
'number-sign-gtr call,
             'type call,
              'bye call,

run