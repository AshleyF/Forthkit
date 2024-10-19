( VT100 terminal commands )

: esc 27 emit ;
: bracket char [ emit ;
: semi char ; emit ;

: vthome ( cr- ) esc bracket 1+ . semi 1+ . char H emit ; ( TODO: multi-digit )
: vtclear esc bracket char 2 emit char J emit ;
: vtreset esc char c emit ;

: vtattribs esc bracket dup 0 do swap num dup i 1+ > if semi then loop char m emit ;
: vtattrib 1 vtattribs ;

30 constant vtblack
31 constant vtred
32 constant vtgreen
33 constant vtyellow
34 constant vtblue
35 constant vtmagenta
36 constant vtcyan
37 constant vtwhite

0 constant vtreset
1 constant vtbright
2 constant vtdim
4 constant vtunderscore	
5 constant vtblink
7 constant vtreverse
8 constant vthidden

: vtfg ( f- ) vtattrib ;
: vtbg ( b- ) 10 + vtattrib ; ( background = foreground + 10 )
: vtcolors ( fb- ) 10 + 2 vtattribs ; ( background = foreground + 10 )

: vthide esc bracket char ? emit 25 num char l emit ;
: vtshow esc bracket char ? emit 25 num char h emit ;
