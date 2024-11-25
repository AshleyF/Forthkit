require machine.fs
require kernel.fs

: memory-dump ( -- ) here memory - dup . ." bytes" memory swap dump ; \ dump allocated memory (to here)
: stacks-dump ( -- ) d reg @ r reg @ min dup $8000 swap - swap dump ; \ dump stacks