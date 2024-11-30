require machine.fs
require kernel.fs

: dump-registers ( -- ) registers 16 cells dump ;
: dump-memory ( -- ) here memory - dup . ." bytes" memory swap dump ; \ dump allocated memory (to here)
: dump-stacks ( -- ) d reg @ r reg @ min dup memory + memory-size rot - dump ; \ dump stacks

: run-word ( addr -- ) pc reg ! run ;
: rw run-word ;

here memory - . ." byte kernel"

label 'dot
              x popd,
           48 y ldc,
          x x y add,
              x out,
                ret,

label 'foo
             -7 literal,
        'negate call,
           'dot call,
           'bye call,

label 'test
              2 literal,
              3 literal,
              1 literal,
                if,
              4 literal,
              5 literal,
                else,
              6 literal,
              7 literal,
                then,
              8 literal,
              9 literal,
           'dot call,
           'dot call,
           'dot call,
           'dot call,
           'dot call,
           'dot call,
           'bye call,

run