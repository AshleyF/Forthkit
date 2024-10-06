( snake game )

variable x 40 x ! variable y 20 y !

: cursor x @ y @ vthome ;
: _vmove y @ + y ! cursor ;
: _hmove x @ + x ! cursor ;
: up -1 _vmove ;
: down 1 _vmove ;
: left -1 _hmove ;
: right 1 _hmove ;

: snake cursor 9632 emit ;

: input
  key
  dup ascii x = if halt else
  dup ascii w = if snake up vtcyan vtfg 9650 emit else
  dup ascii a = if snake left vtmagenta vtfg 9664 emit else
  dup ascii r ( s ) = if snake down vtyellow vtfg 9660 emit else
  dup ascii s ( d ) = if snake right vtred vtfg 9654 emit else
  dup 10 = if ( ignore ) else
  
  then then then then then drop
  recurse ;

vthide
vtblack vtbg vtclear 0 0 vthome
vtbright vtattrib
x @ y @ vthome
input
