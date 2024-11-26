require machine.fs
require kernel.fs

: dump-registers ( -- ) registers 16 cells dump ;
: dump-memory ( -- ) here memory - dup . ." bytes" memory swap dump ; \ dump allocated memory (to here)
: dump-stacks ( -- ) d reg @ r reg @ min dup memory + memory-size rot - dump ; \ dump stacks

: run-word ( addr -- ) pc reg ! run ;

0 header, test
label 'test
           42 x ldc,
              x pushd,
         1000 x ldc,
              x pushd,
         'store call,
            7 x ldc,
              x pushd,
           'bye call,
