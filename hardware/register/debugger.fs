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

label 'test \ 78598698719
             7 literal,
          'dot call,
             5 literal,
             5 literal,
               do,            \ false addr
             8 literal,
          'dot call,
      'i-index call,
          'dot call,
      'i-index call,
             6 literal,
       'equals call,
               if,          \ false addr ifaddr
             1 literal,
          'dot call,
               leave,       \ false baddr true addr ifaddr
               then,
             9 literal,
          'dot call,
               loop,
             9 literal,
          'dot call,
          'bye call,

run