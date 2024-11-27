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
              1 pushc,
              2 pushc,
              3 pushc,
              4 pushc,
              5 pushc,
              6 pushc,
              7 pushc,
              8 pushc,
          'drop call, \ 1 2 3 4 5 6 7
          'drop call, \ 1 2 3 4 5 6
           'nip call, \ 1 2 3 4 6
          'over call, \ 1 2 3 4 6 4
           'rot call, \ 1 2 3 6 4 4
          'drop call, \ 1 2 3 6 4
          '-rot call, \ 1 2 4 3 6
          'to-r call, \ 1 2 4 3
          'to-r call, \ 1 2 4
        'r-from call, \ 1 2 4 3
       'r-fetch call, \ 1 2 4 3 6
       'r-fetch call, \ 1 2 4 3 6 6
           'dot call,
           'dot call,
           'dot call,
           'dot call,
           'dot call,
           'dot call,
           'bye call,

label 'go
              1 pushc,
      'two-star call,
      'two-star call,
      'two-star call,
     'two-slash call,
           'dot call,
           'bye call,

run