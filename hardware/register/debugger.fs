require machine.fs
require kernel.fs

: dump-registers ( -- ) registers 16 cells dump ;
: dump-memory ( -- ) here memory - dup . ." bytes" memory swap dump ; \ dump allocated memory (to here)
: dump-stacks ( -- ) d reg @ r reg @ min dup memory + memory-size rot - dump ; \ dump stacks

: run-word ( addr -- ) pc reg ! run ;
: rw run-word ;

here . ." byte kernel"

label 'dot-old
              x popd,
           48 y ldc,
          x x y add,
              x out,
                ret,

12 constant v

label 'dot
              x popd,
           48 w ldc,
        10000 y ldv,
          z x y div,
          v z w add,
              v out,
          y y z mul,
          x x y sub,
         1000 y ldv,
          z x y div,
          v z w add,
              v out,
          y y z mul,
          x x y sub,
          100 y ldv,
          z x y div,
          v z w add,
              v out,
          y y z mul,
          x x y sub,
           10 y ldv,
          z x y div,
          v z w add,
              v out,
          y y z mul,
          x x y sub,
          v x w add,
              v out,
           'c-r jump,

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
12345 literal,
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
           108 literal, \ l
         'hold call,
           101 literal, \ e
         'hold call,
           104 literal, \ h
         'hold call,
'number-sign-greater call,

'two-dupe call,
'less-number-sign call,
        'holds call,
            32 literal,
         'hold call,
        'holds call,
'number-sign-greater call,
         'type call,
          'bye call,

run