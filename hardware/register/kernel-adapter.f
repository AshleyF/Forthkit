
( similar to in pixels-adapter.f but builds on stack rather than compiling )
: sym -1 begin 1+ key dup >r 32 = until r> drop dup begin r> -rot 1- dup 0 = until drop ;
