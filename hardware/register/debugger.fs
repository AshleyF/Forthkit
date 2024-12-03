require machine.fs
require kernel.fs

: dump-registers ( -- ) registers 16 cells dump ;
: dump-memory ( -- ) here memory - dup . ." bytes" memory swap dump ; \ dump allocated memory (to here)
: dump-stacks ( -- ) d reg @ r reg @ min dup memory + memory-size rot - dump ; \ dump stacks

: run-word ( addr -- ) pc reg ! run ;
: rw run-word ;

here . ." byte kernel"

label 'dot
              x popd,
           48 y ldc,
          x x y add,
              x out,
                ret,

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

run