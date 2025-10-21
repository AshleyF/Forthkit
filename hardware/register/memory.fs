$10000 constant memory-size
create memory memory-size allot \ memory-size buffer: memory (buffer: not in old gforth)
memory memory-size erase
variable h  memory h !

: s! ( val addr -- ) memory + over 8 rshift over 1+ c! c! ;
: s@ ( addr -- val ) memory + dup c@ swap 1+ c@ 8 lshift or ;

: block-file ( n -- addr len ) s" block" rot 0 <# #s #> s+ s" .bin" s+ ;

: read-block ( block addr len )
  rot block-file 2dup file-status 0<> if ." Block file not found " else drop then
  r/o open-file throw
  rot memory + -rot dup >r
  read-file throw drop r>
  close-file throw ;

: write-block ( block addr len )
  rot block-file w/o create-file throw
  rot memory + -rot dup >r
  write-file throw r>
  close-file throw ;

: read-boot-block ( -- ) 0 0 memory-size read-block ;
: write-boot-block ( -- ) 0 memory here write-block ;