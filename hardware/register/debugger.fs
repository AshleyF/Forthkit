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

: pushc, x ldc, x pushd, ;

label 'foo
             -7 pushc,
        'negate call,
           'dot call,
           'bye call,

label 'test
              3 pushc,
              4 pushc,
              5 pushc,
              6 pushc,
      'two-swap call,
           'dot call,
           'dot call,
           'dot call,
           'dot call,
           'bye call,

run