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
            10 literal,
             0 literal,
               do,
             9 literal,
             3 literal,
               do,
      'i-index call,
          'dot call,
      'j-index call,
          'dot call,
          'c-r call,
            1 literal,
              +loop,
              loop,
          'bye call,

label 'test
         'here call,
          'dot call,
             7 literal,
        'allot call,
         'here call,
          'dot call,
          'bye call,

run