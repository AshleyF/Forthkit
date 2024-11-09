# [CHAD](https://github.com/bradleyeckert/chad/tree/master)

: -     invert 1 + + ;                  \ 1.2090 n1 n2 -- n3

\ The I/O is allowed to drop CKE on the processor.
\ Allow for code memory to double-feed the instruction following the pause
\ by putting a nop there. Such an allowance is not made for data memory.
: io@   _io@ nop _io@_ ;                \ 2.0110 addr -- n
: io!   _io! nop drop ;                 \ 2.0120 n addr --

: =     xor 0= ;                        \ 2.0130 n1 n2 -- flag
: <>     xor 0= 0= ;                    \ 2.0135 n1 n2 -- flag
: <     - 0< ; macro                    \ 2.0140 n1 n2 -- flag
: >     swap < ;                        \ 2.0150 n1 n2 -- flag
: cell+ cell + ; macro                  \ 2.0160 a-addr1 -- a-addr2

cell 4 = [if]
    : cells 2* 2* ; macro               \ 2.0170 n1 -- n2
    : cell/ 2/ 2/ ; macro               \ 2.0171 n1 -- n2
    : _cw!  \ end a c! or w!            \ u mask addr
        >r  swap over and               \ m u' | addr
        swap invert                     \ u' mask | addr
        r@  _@ _@_  and  +
        r>  _! drop
    ;
    : c! ( u c-addr -- )                \ 2.0180 c c-addr --
        dup>r 2 and if
            r@ 1 and if  swapw  swapb  $FF000000
            else         swapw         $FF0000
            then
        else
            r@ 1 and if  swapb         $FF00
            else                       $FF
            then
        then
        r> _cw!
    ;
    : c@                                \ 2.0200 c-addr -- c
        _@ _dup@
        over 1 and if swapb then
        swap 2 and if swapw then
        $FF and
    ;

  check_alignment [if]
    : (ta)  ( a mask -- a )
          over and if  22 invert throw  then ;
    : @   3 (ta)  _@ _@_ ;              \ 2.0210 a-addr -- x
    : !   3 (ta)  _! drop ;             \ 2.0200 x a-addr --
    : w!  ( u w-addr -- )               \ 2.0190 w addr --
        1 (ta)
        dup>r 2 and if  swapw  $FFFF0000
        else  $FFFF  then
        r> _cw!
    ;
    : w@  ( w-addr -- u )               \ 2.0220 addr -- w
        1 (ta)
        _@ _dup@  swap 2 and if swapw then
        $FFFF and
    ;
  [else]
    : @   _@ _@_ ;  ( macro )           \ 2.0210 a-addr -- x
    : !   _! drop ; ( macro )           \ 2.0200 x a-addr --
    : w! ( u c-addr -- )                \ 2.0190 w w-addr --
        dup>r 2 and if  swapw  $FFFF0000
        else  $FFFF  then
        r> _cw!
    ;
    : w@                                \ 2.0220 w-addr -- w
        _@ _dup@  swap 2 and if swapw then
        $FFFF and
    ;
  [then]
[else] \ 16-bit or 18-bit cells
    : cells 2* ; macro                  \ 2.0170 n1 -- n2
    : cell/ 2/ ; macro                  \ 2.0171 n1 -- n2
  check_alignment [if]
    : (ta)  over and if  22 invert throw  then ;
	: w@  [ ;
    : @   1 (ta)  _@ _@_ ;              \ 2.0210 a-addr -- x
	: w!  [ ;
    : !   1 (ta)  _! drop ;             \ 2.0200 x a-addr --
  [else]
	: w@  [ ;
    : @   _@ _@_ ; ( macro )            \ 2.0210 a-addr -- x
	: w!  [ ;
    : !   _! drop ; ( macro )           \ 2.0200 x a-addr --
  [then]
    : c! ( u c-addr -- )                \ 2.0180 c c-addr --
        dup>r 1 and if  swapb  $FF00  else  $FF  then
        swap over and                   \ m u' | addr
        swap invert                     \ u' mask | addr
        r@ _@ _@_ and  +
        r> _! drop
    ;
    : c@                                \ 2.0200 c-addr -- c
        _@ _dup@  swap 1 and if swapb then
        $FF and
    ;
[then]

\ Your code can usually use + instead of OR, but if it's needed:
: or    invert swap invert and invert ; \ 2.0300 n m -- n|m
: rot   >r swap r> swap ;               \ 2.0310 x1 x2 x3 -- x2 x3 x1

: execute  >r ; no-tail-recursion       \ 2.0320 i*x xt -- j*x
: ?dup   dup if dup then ;              \ 2.0325 x -- 0 | x x
: 2dup   over over ; ( macro )          \ 2.0330 d -- d d
: 2drop  drop drop ;                    \ 2.0340 d --
: char+ [ ;                             \ 2.0350 c-addr1 -- c-addr2
: 1+     1 + ;  ( macro )               \ 2.0360 n -- n+1
: 1-    -1 + ;  ( macro )               \ 2.0370 n -- n-1
: negate invert 1+ ;                    \ 2.0380 n -- -n
: tuck   swap over ;  ( macro )         \ 2.0390 n1 n2 -- n2 n1 n2
: +!     tuck @ + swap ! ;              \ 2.0400 n a-addr --
: ?exit  if rdrop then ; no-tail-recursion \ flag --
: d2/    2/ swap 2/c swap ;             \ 2.1160 d1 -- d2

\ This really comes in handy, although there is a small (9T) time penalty.
: times                                 \ 2.0405 n xt --
   swap dup 1- 0< if  2drop exit  then  \ do 0 times
   for  dup>r execute r>  next          \ do 1 or more times
   drop
;

: cowait  ( -- )                        \ wait until coprocessor finishes
   [ 0 cotrig ] begin costat while noop repeat
;

\ Math iterations are subroutines to minimize the latency of lazy interrupts.
\ These interrupts modify the RET operation to service ISRs.
\ RET ends the scope of carry and W so that ISRs may trash them.
\ Latency is the maximum time between returns.

hwoptions 1 and [if]                    \ hardware multiply?
: um*                                   \ 2.0410 u1 u2 -- ud
   [ cellbits 1-  2* 2* 2* 2* 2*  $12 +  cotrig ]
   2drop  cowait                        \ um* is a special case of fractional *
   [ 3 cotrig ]  costat
   [ 2 cotrig ]  costat
;
[else]

\ Multiplication using shift-and-add, 160 to 256 cycles at 16-bit.
\ Latency = 17
: (um*)
   2* >r 2*c carry
   if  over r> +c >r carry +
   then  r>
;
: um*                                   \ 2.0410 u1 u2 -- ud
   0 [ cellbits 2/ ] literal            \ cell is an even number of bits
   for (um*) (um*) next
   >r nip r> swap
;
[then]

hwoptions 2 and [if]                    \ hardware divide?
: um/mod                                \ 2.0420 ud u -- ur uq
   a! [ $14 cotrig ]  2drop cowait
   [ 5 cotrig ]  costat
   [ 4 cotrig ]  costat
;
[else]
\ Long division takes about 340 cycles at 16-bit.
\ Latency = 25
: (um/mod)
   >r  swap 2*c swap 2*c                \ 2dividend | divisor
   carry if
      r@ -   0 >carry
   else
      dup r@  swap invert +c invert     \ test subtraction
      carry if drop else nip then
   then
   r> carry                             \ carry is safe on the stack
;

: um/mod                                \ 2.0420 ud u -- ur uq
   2dup swap invert +c drop carry 0=
   if  drop drop dup xor
       dup invert  exit                 \ overflow = 0 -1
   then
   [ cellbits 2/ ] literal
   for (um/mod) >carry
       (um/mod) >carry
   next
   drop swap 2*c invert                 \ finish quotient
;
[then]

hwoptions 4 and [if]                    \ hardware shifter?
: )dshift                               \ d1 -- d2
   2drop  cowait                        \ SL1_011x
   [ 7 cotrig ]  costat
   [ 6 cotrig ]  costat
;
: drshift                               \ d1 u -- d2
   a!  [ $56 cotrig ]  )dshift
;
: dlshift                               \ d1 u -- d2
   a!  [ $36 cotrig ]  )dshift
;
: lshift  over swap dlshift drop ;	\ 2.1110 x1 u -- x2
: rshift  0 swap drshift drop ;		\ 2.1120 x1 u -- x2

[else]
: lshift  				\ 2.1110 x1 u -- x2
    dup if  for  2*  next  exit
    then drop
;
: rshift  				\ 2.1120 x1 u -- x2
    dup if  for  0 >carry 2/c  next  exit
    then drop
;
: drshift  				\ 2.1130 d1 u -- d2
    dup if  for  d2/  next  exit
    then drop
;
[then]

: *     um* drop ;                      \ 2.0430 n1 n2 -- n3
: dnegate                               \ 2.0440 d -- -d
        invert swap invert 1 +c swap carry + ;
: abs   dup 0< if negate then ;         \ 2.0450 n -- u
: dabs  dup 0< if dnegate then ;        \ 2.0460 d -- ud

: m/mod
    dup 0< dup >r
    if negate  >r
       dnegate r>
    then >r dup 0<
    if r@ +
    then r> um/mod
    r> if
       swap negate swap
    then
;
: /mod   over 0< swap m/mod ;           \ 2.0470 n1 n2 -- rem quot
: mod    /mod drop ;                    \ 2.0480 n1 n2 -- rem
: /      /mod nip ;                     \ 2.0490 n1 n2 -- quot
: m*                                    \ 2.0500 n1 n2 -- d
    2dup xor 0< >r
    abs swap abs um*
    r> if dnegate then
;
: */mod  >r m* r> m/mod ;               \ 2.0510 n1 n2 n3 -- rem quot
: */     */mod swap drop ;              \ 2.0520 n1 n2 n3 -- n4

: aligned  [ cell 1- ] literal +        \ 1.1050 addr1 -- addr2
           [ cell negate ] literal and ;
: align    dp @ aligned dp ! ;          \ 1.1060 --
: allot    dp +! ;                      \ 2.0550 n --
: here     dp @ ;                       \ 2.0560 -- addr
: ,        align here !  cell allot ;   \ 2.0570 x --
: w,       here w!  2 allot ;           \ 2.0590 c --

\ Paul Bennett's recommended minimum word set is mostly present.
\ DO, I, J, and LOOP are not included. Use for next r@ instead.
\ CATCH is not included. THROW jumps directly to QUIT.
\ DOES> needs a compilable CREATE.

: u<  swap invert +c drop carry 0= 0= ; \ 2.0700 u1 u2 -- flag
: min    2dup - 0< if                   \ 2.0710 n1 n2 -- n3
         drop exit then  swap drop ;
: max    2dup - 0< if                   \ 2.0720 n1 n2 -- n3
         swap drop exit then  drop ;

0 equ false                             \ 2.1000 -- false
-1 equ true                             \ 2.1010 -- true

: within  over - >r - r> u< ;           \ 2.1020 x xlo xhi -- flag
: /string >r swap r@ + swap r> - ;      \ 2.1030 addr1 u1 n -- addr2 u2
: 0<>     0= 0= ; macro                 \ 2.1040 x y -- f
: 0>      negate 0< ;                   \ 2.1050 n -- f
: u>      swap u< ;                     \ 2.1060 u1 u2 -- flag
: 2>r     swap r> swap >r swap >r >r    \ 2.1070 d -- | -- d
; no-tail-recursion
: 2r>     r> r> swap r> swap >r swap    \ 2.1080 -- d | d --
; no-tail-recursion
: 2r@  r> r> r@ swap >r swap r@ swap >r \ 2.1090 -- d | d -- d
; no-tail-recursion
: third   >r >r dup r> swap r> swap ;   \ 2.1100 x1 x2 x3 -- x1 x2 x3 x1
: count   dup 1+ swap c@ ;              \ 2.1200 a -- a+1 c
: @+      dup cell+ swap @ ;            \ 2.1210 a -- a+cell u


: 2@   _@ _dup@ swap cell + @ swap ;    \ 2.1220 a-addr -- x1 x2
: 2!   _! cell + ! ;                    \ 2.1230 x1 x2 a-addr --

\ Add a cell to a double variable and carry into the upper part

: 2+!                                   \ 2.1240 n a-addr
    cell+  tuck @ +c over !
    carry if
        -cell +  1 swap +! exit
    then  drop
;

: d+   >r swap >r +c carry r> + r> + ;  \ 2.1130 d1 d2 -- d3
: d-   dnegate d+ ;                     \ 2.1140 d1 d2 -- d3
: d2*  swap 2* swap 2*c ;               \ 2.1150 d1 -- d2
: d=   d- or 0= ;                       \ 2.1170 d1 d2 -- flag

\ 2nip saves 1 inst by using w. Same trick isn't used with 2swap
\ because carry, a, and b are not safe across calls.

: 2swap  rot >r rot r> ;                \ 2.1190 abcd -- cdab
: 2nip   a! nip nip a ;
: 2over  >r >r 2dup r> r> 2swap ;
: 3drop  drop 2drop ;

: du<                                   \ 2.1180 ud1 ud2 -- flag
    rot  2dupxor
    if  2nip swap u< exit
    then    2drop u<                    \ hi part matches, test lo
;

: [        0 state ! ;  immediate       \ 2.6300 --
: ]        1 state ! ;                  \ 2.6310 --

\ Text input parsing uses an applet that fits in 512 bytes of flash.

applets [if] .(     Applet bytes: { )   \ }
paged applet  paged [then]

\ Number input, about 323 bytes of flash.
\ Assumes that string-to-number conversion isn't time-critical.

: toupper                               \ 2.6000 c -- C
   dup [char] a [char] { within \ }
   32 and -
;
: digit?                                \ 2.6010 c base -- n flag
   >r  toupper
   [ char 0 negate ] literal +          \ char to digit, return `ok` flag
   dup 10 17 within or
   dup 9 > 7 and -
   dup r> u<
;
: >number                        \ 2.6020 ud1 c-addr1 u1 -- ud2 c-addr2 u2
   begin  dup   while
      over c@  base @ digit? if         \ uL uH ca cu digit
         swap >r  2swap                 \ ca digit uL uH | cu
         base @ *  swap                 \ ca digit vH uL | cu
         base @ um* d+                  \ ca ((digit vH) + (wL wH)) | cu
         rot r>
      else
         drop  exit
      then
      1 /string
   repeat
;

\ swap >r  2swap  base @ *  swap base @ um* d+
\ uL uH ca cu digit
\ uL uH ca digit | cu
\ ca digit uL uH | cu
\ ca digit vH wL wH | cu

\ Number parsing inspired by swapforth with dpl added and some tidying up.
\ Prefixes accepted: $, #, %, '

: has?  ( caddr u ch -- caddr' u' f )
    over if
       >r over c@ r> =  over 0<> and
       dup>r negate /string r>
    exit then  dup xor                  \ don't test empty string
;
: (number)  ( c-addr u -- x 0 | x x -1 )
    0 0 2swap  0 dpl !
    [char] - has? >r  >number
    [char] . has? r> 2>r                \ 0 is single, -1 is double
    dup if  dup dpl !  >number  then    \ digits after the decimal
    nip if  2drop                       \ any chars remain: error
       -13 throw exit
    then
    r> if dnegate then                  \ is negative
    r> ?dup and                         \ if single, remove high cell
;
: base(number)  ( c-addr u radix -- x 0 | x x -1 )
    base @ >r base !  (number)  r> base !
;
: is'  ( f caddr -- f' )                \ f remains true if caddr is '
   c@ [char] ' = and
;
: (xnumber)  ( c-addr u -- x 0 | x x -1 )
    [char] $ has? if  16 base(number) exit  then
    [char] # has? if  10 base(number) exit  then
    [char] % has? if   2 base(number) exit  then
    2dup 3 = over is'  swap 2 + is'  if
       drop 1+ c@ false  exit
    then  (number)
;

applets [if] end-applet  paged swap - . [then]

\ Input parsing, about 235 bytes of flash
\ Getting the next blank-delimited string from the input stream involves loading
\ this applet (about 25us) if the caller is not an applet.

applets [if] paged applet  paged [then]

: source   #tib 2@ ;                    \ 2.6030 -- c-addr len
: /source  source >in @ /string ;       \ 2.6040 -- c-addr len
: source?  >in 2@ u> ;                  \ -- flag   in source?
: char?    /source drop c@ ;            \ -- c  get source char
: in++     1 >in +! ;                   \ --

: _parse  ( c -- addr1 addr2 )          \ parse token from input
   begin  dup char? =  source? and
   while  in++  repeat                  \ skip leading delimiters
   /source drop swap over               ( addr1 c addr1 )
   begin  source?  while
      over char? =  in++
      if  nip exit  then  1+
   repeat nip                           \ end of input
;
: parse-name  bl [ ;                    \ 2.6060 <name> -- addr len
: parse       _parse over - ;           \ 2.6070 delimiter -- addr len

\ Cooked terminal input doesn't need echoing. It lets you edit the line before
\ sending it, at which point it sends the string all at once with an ending LF.
\ The EOL chars are stripped. If you fill the input buffer, `accept` terminates.

: key?   io'rxbusy io@ ;                \ 2.6100 -- n
: key  begin key? until  io'udata io@ ; \ 2.6120 -- c

: accept                                \ 2.6140 c-addr +n1 -- +n2
   >r dup dup r> + >r                   \ a a | limit
   begin  dup r@ xor  while  key
      dup 13 =  over 10 =  or           \ CR or LF = EOL
      if  drop
         r> drop swap - exit
      then                              ( a a' c | limit )
      dup bl < if  drop  else           \ ignore invalid chars
         echoing @ if
            dup emit                    \ echo if enabled
         then
         over c! 1+
      then
   repeat  r> drop swap -               \ filled
;

: refill                                \ 2.6150 -- okay?
  'tib @ |tib| accept  #tib !
  0 >in !  true
;

applets [if] end-applet  paged swap - . [then]

\ Search order words, about 173 bytes of flash.

applets [if] paged applet  paged [then]

\ The search order is implemented as a stack that grows upward, with #order the
\ offset into the orders array as well as the number of WIDs in the list.

: 'context  ( -- a )                    \ point to top of order
   #order @ cells [ orders cell - ] literal +
;
: context                               \ 2.6160 -- f-addr
   'context @ wid
;

:noname @+ swap ;
: get-order                             \ 2.6170 -- widn ... wid1 n
   orders  #order @  literal times
   drop    #order @
;
: set-order                             \ 2.6180 widn .. wid1 n --
   dup 0< if  drop root forth-wordlist 2  then
   dup 8 > -49 and throw                \ search order overflow
   dup #order !  orders over cells +    \ ... wid n dest
   begin over while
      cell -  swap 1- >r  tuck !  r> swap
   repeat  2drop
;
: +order ( wid -- )
   >r get-order r> swap 1+ set-order
;
: set-current  current ! ;              \ 2.6200 wid --
: get-current  current @ ;              \ 2.6210 -- wid
: also         get-order over swap 1+   \ 2.6230 --
               set-order
;
: previous     get-order nip 1-         \ 2.6240 --
               set-order
;
: definitions                           \ 2.6250 --
   get-order  over set-current  set-order
;
: forth        get-order nip            \ 2.6260 --
               forth-wordlist swap set-order
;

applets [if] end-applet  paged swap - . [then]

\ Wordlist display, about 229 bytes of applet flash

applets [if] paged applet  paged [then]

\ Header structures are in flash. Up to 8 wordlists may be in the
\ search order. WIDs are indices into `wids`, an array in data space.
\ The name of the wordlist is just before the first link.
\ The count is after the name instead of before it so it's stored as plaintext.

: (.wid)  \ f-addr --                   \ f-addr points to the link that's 0
   1-  dup /text c@f  tuck -  swap      \ f-addr len
   31 > if drop [char] ? emit exit then \ no name
   1-  f$type
;

: .wid   \ wid --                       \ display WID identifier (for order)
   wid    dup                           \ get the pointer
   begin  nip dup /text @f              \ skip to beginning
   dup 0= until  drop
   (.wid) space
;

: order                                 \ 2.6270 --
   ."  Context: " #order @ ?dup if
      for r@ cells [ orders 1 cells - ] literal + @ .wid
      next
   else ." <empty> " then
   cr ."  Current: " current @ .wid  cr
;

\ `words` overrides the host version, which is in `root`.
\ Use `Words` for the original version.

: _words  ( wordlist -- )
   1 stack(
   begin dup while
      dup /text @f  swap                ( link ht )
      [ cellbits 8 / ] literal +
      0 fcount ftype space
   repeat drop
   )stack
;
: words                                 \ 2.6280 --
   context _words                       \ list words in the top wordlist
;

\ Given a message table, look up the message number and print it.
\ `msg_seek` looks up the message in the list and returns 0 if not found.
\ `msg` prints the message. If not found, it defaults to message 0.
\ The list of standard FORTH errors is ~2KB, which is nothing for flash.

: msg_seek  ( idx f-addr -- f-addr' | 0 )
   begin  over  while  swap 1- swap
      /text fcount nip tuck 0= if       \ idx offset f-addr
         2drop dup xor exit
      then  +
   repeat  nip
;
: msg  \ 2.4120 idx f-addr --
   dup>r msg_seek
   ?dup if rdrop else r> then  f$type
;

fhere equ errorMsgs \ starting at -2 and going negative
           ,"  "
   (  -3 ) ," Stack overflow"
   (  -4 ) ," Stack underflow"
   (  -5 ) ," Return stack overflow"
   (  -6 ) ," Return stack underflow"
   (  -7 ) ," Do-loops nested too deeply during execution"
   (  -8 ) ," Dictionary overflow"
   (  -9 ) ," Invalid memory address"
   ( -10 ) ," Division by zero"
   ( -11 ) ," Result out of range"
   ( -12 ) ," Argument type mismatch"
   ( -13 ) ," Word not found"
   ( -14 ) ," Interpreting a compile-only word"
   ( -15 ) ," Invalid FORGET"
   ( -16 ) ," Attempt to use zero-length string as a name"
   ( -17 ) ," Pictured numeric output string overflow"
   ( -18 ) ," Parsed string overflow"
   ( -19 ) ," Definition name too long"
   ( -20 ) ," Write to a read-only location"
   ( -21 ) ," Unsupported operation"
   ( -22 ) ," Control structure mismatch"
   ( -23 ) ," Address alignment exception"
   ( -24 ) ," Invalid numeric argument"
   ( -25 ) ," Return stack imbalance"
   ( -26 ) ," Loop parameters unavailable"
   ( -27 ) ," Invalid recursion"
   ( -28 ) ," User interrupt"
   ( -29 ) ," Compiler nesting"
   ( -30 ) ," Obsolescent feature"
   ( -31 ) ," >BODY used on non-CREATEd definition"
   ( -32 ) ," Invalid name argument (e.g., TO xxx)"
   ( -33 ) ," Block read exception"
   ( -34 ) ," Block write exception"
   ( -35 ) ," Invalid block number"
   ( -36 ) ," Invalid file position"
   ( -37 ) ," File I/O exception"
   ( -38 ) ," File not found"
   ( -39 ) ," Unexpected end of file"
   ( -40 ) ," Invalid BASE for floating point conversion"
   ( -41 ) ," Loss of precision"
   ( -42 ) ," Floating-point divide by zero"
   ( -43 ) ," Floating-point result out of range"
   ( -44 ) ," Floating-point stack overflow"
   ( -45 ) ," Floating-point stack underflow"
   ( -46 ) ," Floating-point invalid argument"
   ( -47 ) ," Compilation wordlist deleted"
   ( -48 ) ," Invalid POSTPONE"
   ( -49 ) ," Search-order overflow"
   ( -50 ) ," Search-order underflow"
   ( -51 ) ," Compilation wordlist changed"
   ( -52 ) ," Control-flow stack overflow"
   ( -53 ) ," Exception stack overflow"
   ( -54 ) ," Floating-point underflow"
   ( -55 ) ," Floating-point unidentified fault"
   ( -56 ) ," QUIT"
   ( -57 ) ," Exception in sending or receiving a character"
   ( -58 ) ," [IF], [ELSE], or [THEN] exception"
   ( -59 ) ," Missing literal before opcode"
   ( -60 ) ," Attempt to write to non-blank flash memory"
   ( -61 ) ," Macro expansion failure"
   ( -62 ) ," Input buffer overflow, line too long"
   ( -63 ) ," Bad arguments to RESTORE-INPUT"
   ( -64 ) ," Write to non-existent data memory"
   ( -65 ) ," Read from non-existent data memory"
   ( -66 ) ," PC is in non-existent code memory"
   ( -67 ) ," Write to non-existent code memory"
   ( -68 ) ," Test failure"
   ( -69 ) ," Page fault writing flash memory"
   ( -70 ) ," Bad I/O address"
   ( -71 ) ," Writing to flash without issuing WREN first"
   ( -72 ) ," Invalid ALU opcode"
   ( -73 ) ," Bitfield is 0 or too wide for a cell"
   ( -74 ) ," Resolving a word that's not a DEFER"
   ( -75 ) ," Too many WORDLISTs used"
   ( -76 ) ," Internal API calls are blocked"
   ( -77 ) ," Invalid CREATE DOES> usage"
   ( -78 ) ," Nesting overflow during include"
   ( -79 ) ," Compiling an execute-only word"
   ( -80 ) ," Dictionary full"
   ( -81 ) ," Writing to invalid flash sector"
   ( -82 ) ," Flash string space overflow"
   ( -83 ) ," Invalid SPI flash address"
   ( -84 ) ," Invalid coprocessor field"
   ( -85 ) ," Can't postpone an applet word"
   ," " \ empty string = end of list

applets [if] end-applet  paged swap - . [then]

\ Quit messages, about 123 bytes of applet flash

applets [if] paged applet  paged [then]

: .error  ( error -- )
   dup if
      cr ." Error "  dup .
      invert 1-  dup 1 84 within if
         errorMsgs msg space
      else
         drop
      then
      cr source type
      cr >in @ $7F and spaces ." ^-- >in"
      exit
   then  drop
;

: _.s  \ ? -- ?
   depth  begin dup while               \ if negative depth,
      dup pick .  1-                    \ depth rolls back around to 0
   repeat drop
;

: prompt  ( ? -- ? )                    \ "ok>" includes stack dump
   depth if ." \ " _.s then
   ." ok>"
;

applets [if] end-applet  paged swap - . [then]

\ ------------------------------------------------------------------------------

\ `interpret` wants to be outside the applet.

: interpret  ( -- ? )
   begin  parse-name  dup while
      hfind over if                     \ not found
         2 stack(  (xnumber)  )stack
         if
            state @ if swap lit, lit, then
         else
            state @ if lit, then
         then
      else
         2drop
         @f> @f> @f>
         @f> api !  )@f                 ( w xte xtc )
         state @ if swap then drop execute
      then
   repeat 2drop
;

\ The QUIT loop avoids CATCH and THROW for stack depth reasons.
\ If an error occurs, `throw` restarts `quit`.
\ The stacks are cleared. One item is allowed on the return stack
\ to keep the simulator happy.

\ Since `throw` targets it, `quit` can't be in an applet.

: quit                                  \ 2.6320 error_id --
   state !  decimal
   \ If stacks are more than 8 deep, limit them to 8
   begin spstat $18 and while drop repeat
   begin spstat swapb $18 and while rdrop repeat
   state @  postpone [  .error cr
   depth if  ." Data stack -> "
      begin spstat 7 and while . repeat  cr
   then
   spstat swapb $1F and  ( rdepth )
   dup 1 > if  ." Return stack -> "         hex
      begin 1- dup while  r> .  repeat  cr  decimal
   then drop
   0 api !
   fpclear
   [ dm-size |tib| - ] literal 'tib !
   begin
      prompt
      refill drop
      interpret
   again
;

\ `only` can't be in the applet because set-current can't execute
\ until end-applet writes the applet to the flash memory image.

root set-current                        \ escape hatch from root
: only         -1 set-order ;           \ 2.6220 --
forth-wordlist set-current

0 [if]
\ Text input uses shared data (chad.c and the Forth) so >in can be manipulated
\ here. However, if `\` is defined here the host version will stop compiling
\ HTML code after \. So, don't define it until you need it.

: \   #tib @ >in ! ; immediate          \ 2.6080 ccc<EOL> --

[then]

.( }; ) there swap - . .( instructions used by interpret) cr

: -rot  swap >r swap r> ;

: sm/rem  \ d n -- rem quot             \ 6.1.2214
   2dup xor >r  over >r  abs >r dabs r> um/mod
   swap r> 0< if  negate  then
   swap r> 0< if  negate  then ;

: fm/mod  \ d n -- rem quot             \ 6.1.1561
   dup >r  2dup xor >r  dup >r  abs >r dabs r> um/mod
   swap r> 0< if  negate  then
   swap r> 0< if  negate  over if  r@ rot -  swap 1-  then then
   r> drop ;

: (umin)  over over- drop carry ;
: umin   (umin) if swap drop exit then  drop ;
: umax   (umin) if drop exit then  swap drop ;

: roll                                  \ 2.2980 xu..x0 u -- xu-1..x0 xu
    ?dup if
        1+  frp @  ds>mem  mem>
        1-  over cell -                 ( a u' 'xu )
        dup @ >r  !
        mem>ds drop  r>
    then
;

hex
\ Attempt to convert utf-8 code point
: nextutf8  \ n a -- n' a'              \ add to utf-8 xchar
   >r 6 lshift r> count                 \ expect 80..BF
   dup 0C0 and 80 <> -0D and throw  \ n' a c
   3F and  swap >r  +  r>
;
: isutf8  \ addr len -- xchar
   over c@ 0F0 <  over 1 = and  if      \ plain ASCII
      drop c@ exit
   then
   over c@ 0E0 <  over 2 = and  if      \ 2-byte utf-8
      drop count 1F and  swap  nextutf8
      drop exit
   then
   over c@ 0F0 <  over 3 = and  if      \ 3-byte utf-8
      drop count 1F and  swap  nextutf8  nextutf8
      drop exit
   then
   -0D throw
;
decimal

: decimal 10 base ! ;
: hex     16 base ! ;
:noname   count emit ;  ( xt )          \ send string to output device
: type    literal times drop ;          \ 2.3140 c-addr u --
: s>d     dup 0< ;                      \ 2.3150 n -- d
: space   bl emit ;                     \ 2.3160 --
: spaces  ['] space times ;             \ 2.3170 n --

\ Numeric conversion. `d.r` uses frame stack protection to prevent overflow
\ when the stacks have significant content. Since `d.r` ia typically at the
\ end of a definition, its tail call doesn't increase the stack.

: digit   dup -10 + 0< -7 and           \ 2.3180 n -- char
          + [char] 7 + ;
: <#      numbuf  hld ! ;               \ 2.3190 ud1 -- ud1
: hold    hld dup >r @ 1- dup r> ! c! ; \ 2.3200 char --
: _#_     um/mod swap digit hold ;      \ ud base -- u/base
: #       dup  base @ >r  if            \ 2.3210 ud1 -- ud2
              0 r@ um/mod r> swap
              >r _#_ r> exit
          then  r> _#_ 0
;
: #s      begin # 2dup or 0= until ;    \ 2.3220 ud1 -- ud2
: sign    0< if [char] - hold then ;    \ 2.3230 n --
: #>      2drop hld @ numbuf  over - ;  \ 2.3240 ud -- c-addr u
: s.r     over - spaces type ;          \ length width --
: d.r     3 stack(  >r dup >r dabs      \ 2.3250 d width --
          <# #s r> sign #> r> s.r )stack ;
: u.r     0 swap d.r ;                  \ 2.3260 u width --
: .r      >r s>d r> d.r ;               \ 2.3270 n width --
: d.      0 d.r space ;                 \ 2.3280 d --
: u.      0 d. ;                        \ 2.3290 u --
: ?       @ [ ;                         \ 2.3310 a --
: .       s>d d. ;                      \ 2.3300 n --
: <#>     >r  <# begin # next #s #> ;   \ ud digits-1
: h.2     1 [ ;
: h.x     base @ >r hex  0 swap <#> r>  \ 2.3320 u n --
          base !  type space ;

there swap - . .( instructions used by numeric output) cr

