$10000 constant memory-size
memory-size buffer: memory
memory memory-size erase
variable h  memory h !

: s! ( val addr -- ) memory + over 8 rshift over 1+ c! c! ;
: s@ ( addr -- val ) memory + dup c@ swap 1+ c@ 8 lshift or ;

: block-filename ( n -- addr len ) 
  s" block" rot 0 <# #s #> s+ s" .bin" s+ ;
: read-block ( addr size block# -- ) 
  block-filename r/o open-file 0= if 
    >r swap rot r@ read-file drop drop r> close-file drop 
  else 
    drop 2drop 
  then ;
: write-block ( addr size block# -- ) 
  block-filename w/o create-file 0= if 
    >r swap rot r@ write-file drop r> close-file drop 
  else 
    drop 2drop 
  then ;
: read-boot-block ( -- ) memory memory-size 0 read-block ;