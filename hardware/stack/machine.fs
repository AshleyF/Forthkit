$8000 constant memory-size
memory-size buffer: memory  memory memory-size erase
variable h  memory h !

16 constant stack-size

: wrap over - stack-size mod + ;

stack-size cells buffer: dstack
variable sp  dstack sp !

: dpush sp dstack push ;
: dpop sp dstack pop ;

stack-size cells buffer: rstack
variable rsp  rstack rsp !

: rpush rsp rstack push ;
: rpop rsp rstack pop ;

variable p  -1 p !

: fetch p c@ 1 p +! ;
: run
  begin
    fetch dup $80 and =0 if
      8 lshift fetch or
      p c@ $ff <> if p rpush then
      p !
    else
      case
        $80 of quit endof
        $81 of endof
        $82 of endof
        $82 of endof
        $84 of endof
        $85 of endof
        $ff of endof
      endcase
    then
  p 0< until
;