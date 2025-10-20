
false warnings ! \ redefining gforth words

: (bye) cr ." HALT" cr quit ; \ quit back to gforth REPL without exiting process

true warnings !

require machine.fs

: dump-register ( r -- ) reg @ . ;
: dump-all-registers ( -- ) registers 16 cells dump ;

: dump-memory ( -- ) memory here 1- dump ;
: dump-all-memory ( -- ) memory memory-size dump ;