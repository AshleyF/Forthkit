$10000 constant memory-size
memory-size buffer: memory
memory memory-size erase
variable h  memory h !

: s! ( val addr -- ) memory + over 8 rshift over 1+ c! c! ;
: s@ ( addr -- val ) memory + dup c@ swap 1+ c@ 8 lshift or ;

: block-filename ( n -- addr len ) 
  s" block" rot 0 <# #s #> s+ s" .bin" s+ ;
: read-block ( block# offset size -- ) 
  rot block-filename r/o open-file throw
  rot memory + -rot dup >r
  read-file throw drop r>
  close-file throw ;
: write-block ( block# addr size -- ) 
  rot block-filename w/o create-file 0= if 
    >r r@ write-file drop r> close-file drop 
  else 
    drop 2drop 
  then ;
: read-boot-block ( -- ) 0 0 memory-size read-block ;