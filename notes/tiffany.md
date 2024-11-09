# [Tiffany](https://github.com/bradleyeckert/Tiffany/tree/master)

 0 equ false                            \ 6.2.1485
-1 equ true                             \ 6.2.2298
32 equ bl                               \ 6.1.0770

: -             invert 1+ + ; macro     \ replace "-" opcode
: !                 !+ drop ; macro     \ 6.1.0010  x addr --
: c!               c!+ drop ; macro     \ 6.1.0850  c addr --
: w!               w!+ drop ; macro     \           w addr --
: +!        dup >r @ + r> ! ;           \ 6.1.0130  x addr --
: c+!     dup >r c@ + r> c! ;           \           c addr --
: negate          invert 1+ ; macro     \ 6.1.1910  n -- -n
: 1-       invert 1+ invert ; macro     \ 6.1.0300  n -- n-1
: cell- invert cell+ invert ; macro     \           n -- n-4
: cells               2* 2* ; macro     \ 6.1.0890  n -- n*4
: rot       >r swap r> swap ; macro     \ 6.1.2160  n m x -- m x n
: -rot      swap >r swap r> ; macro     \           n m x -- x n m
: tuck            swap over ; macro     \ 6.2.2300  ab -- bab
: nip             swap drop ; macro     \ 6.2.1930  ab -- b
: 2>r        swap r> swap >r swap >r >r \ 6.2.0340
; call-only
: 2r>        r> r> swap r> swap >r swap \ 6.2.0410
; call-only
: 2r@  r> r> r@ swap >r swap r@ swap >r \ 6.2.0415
; call-only
: 2drop           drop drop ; macro     \ 6.1.0370  d --
: 2dup            over over ; macro     \ 6.1.0380  d -- d d
: 2swap       rot >r rot r> ;           \ 6.1.0430  d1 d2 -- d2 d1
: 2over    >r >r 2dup r> r> 2swap ;     \ 6.1.0400  d1 d2 -- d1 d2 d1
: 3drop      drop drop drop ; macro     \ abc --
: ?dup  dup ifz: exit | dup ;           \ 6.1.0630  n -- n n | 0
: s>d   dup -if: dup xor invert exit |  \ 6.1.2170  n -- d
        dup xor ;
: not                    0= ; macro     \           x -- f
: =                  xor 0= ; macro     \ 6.1.0530  x y -- f
: <>              xor 0= 0= ; macro     \ 6.2.0500  x y -- f
: 0<>                 0= 0= ; macro     \ 6.2.0260  x y -- f
: 0>              negate 0< ; macro     \ 6.2.0280  n -- f
: aligned   1+ 1+ 1+ -4 and ;           \ 6.1.0706  n -- n'
: 2@         @+ swap @ swap ;           \ 6.1.0350  a -- n2 n1
: 2!             !+ !+ drop ; macro     \ 6.1.0310  n2 n1 a --
: abs         |-if negate | ;           \ 6.1.0690  n -- u
: or    invert swap invert and invert ; \ 6.1.1980  n m -- n|m
: execute                >r ;           \ 6.1.1370  xt --
: d+     >r >r swap r> +  swap r> c+ ;  \ 8.6.1.1040  d1 d2 -- d3
: dnegate                               \ 8.6.1.1230  d -- -d
   invert >r invert 1+ r> |
   over 0= -if: drop 1+ exit |
   drop
;
: dabs    |-if dnegate exit | ;         \ 8.6.1.1160  n -- u
: count   c@+ ; macro					\ 6.1.0980  a -- a+1 c
: third   4 sp @ ;
: fourth  8 sp @ ;
: depth   sp0 @ 8 sp - 2/ 2/ ;          \ 6.1.1200  -- n
: pick    dup cells sp @ swap drop ;    \ 6.2.2030  ? n -- m
: decimal 10 base ! ;                   \ 6.1.1170  --
: hex     16 base ! ;                   \ 6.2.1660  --
: link>   @ 16777215 and ;              \ a1 -- a2, mask off upper 8 bits
         \ do is  "swap negate >r >r"  macro
         \ run loop?             no    yes
: (?do)  \ limit i -- | R: RA -- RA | -limit i RA+4
   over over invert +   |-if 3drop exit |
   drop   swap negate                   \ i -limit
   r> cell+  swap >r swap >r  >r
; call-only
: (loop)  \ R: -limit i RA -- -limit i+1 RA | RA+4
   R> R> 1+ R>  over over +             \ RA i+1 ~limit sum
   |-if drop >R >R >R exit |            \ keep going
   drop drop drop cell+ >R              \ quit loop
; call-only
: (+loop)  \ x -- | R: -limit i RA -- -limit i+x RA / RA+4
   r> swap r> over +                    \ RA x i' | -limit
   r@ over +  swap >r                   \ RA x sign | -limit i'
   swap 0< xor                          \ RA sign | -limit i'
   |-if  drop >r  exit |                \ keep going
   drop  r> drop  r> drop  cell+ >r     \ quit loop
; call-only

: j       12 rp @ ; call-only           \ 6.1.1730  R: ~limit j ~limit i RA
: unloop  R> R> drop R> drop >R         \ 6.1.2380  R: x1 x2 RA -- RA
; call-only
        \ match?         no         yes
: (of)  \ x1 x2 R: RA -- x1 R: RA | R: RA+4
   over over xor 0= |ifz drop exit |
   drop drop  r> cell+ >r               \ eat x and skip branch
; call-only
         \ done?        no           yes
: (for)  \ R: cnt RA -- cnt-1 RA+4 | RA
   r> r> 1-                             \ faster than DO LOOP because all of the
   |-if drop >r exit |                  \ indexing is done here. NEXT just jumps.
   >r cell+ >r                          \ FOR NEXT does something 0 or more times.
; call-only                             \ It also skips if negative count.

: (string)  \ -- c-addr | R: RA -- RA'	\ skip over counted string
   r> dup c@+ + 						\ ra ra'
   3  dup >r  + 						\ aligned
   r> invert and >r
; call-only

: u<  - 2*c 1 and 1- ;                	\ 6.1.2340  u1 u2 -- flag
: u>  swap u< ;                         \ 6.2.2350  u1 u2 -- flag

\ Here's where I disagree with the ANS standard.

\ You should be able to use "- 0<" for "<", but ANS "<" assumes the
\ existence of overflow and other flags such as those in typical CPUs.
\ This means the simple definition will fail only in a special case
\ that never happens in practice. That case is included in the Hayes test suite.

options 8 and [if]                      \ loose ANS compliance
: < - 0< ; macro
[else]
: < 2dup xor 0<                         \ 6.1.0480  n1 n2 -- flag
    |ifz - 0< exit |
    drop 0<
;
[then]
: >  swap < ;                           \ 6.1.0540  n1 n2 -- flag

: umove  \ a1 a2 n --                   \ move cells, assume cell aligned
   1- +if
      negate  swap >r swap
      | @+ r> !+ >r -rept nop           \ n a1' | a2'
      r> 3drop exit
   then  3drop
;
: cmove  \ a1 a2 n --                   \ 17.6.1.0910
   1- +if
      negate  swap >r swap
      | c@+ r> c!+ >r -rept nop         \ n a1' | a2'
      r> 3drop exit
   then  3drop
;

: cmove>  \ a1 a2 n --                  \ 17.6.1.0920
   1- +if  1+
      dup >r +  swap r@ +  swap  r>     \ a1 a2 n
      negate begin >r
         1- swap 1- swap
         over c@ over c!
      r> 1+ +until
   then  3drop
;

: move  \ from to count --              \ 6.1.1900
   >r  2dup u< if  r> cmove>  else  r> cmove  then
;

: fill  \ a1 n c --                     \ 6.1.1540
   over if
      swap negate                       \ a c -n
      1+ swap >r swap                   \ -n a | c
      | r@ swap c!+ -rept nop           \ -n' a' | c
      r> 3drop exit
   then  3drop
;

\ Is a 4x speedup worth it? Maybe if large RAM needs erased.
\ A 100 MHz CPU erases 16 cells (64 bytes) per microsecond.
\ A large 64KB RAM would erase in 1 ms.
\ I made it big and ugly to speed up hardware simulation.
\ In real life, just use "0 fill".

: erase  \ a n --                       \ 6.2.1350  --
   dup ifz: 2drop exit |                \ no length
   2dup or 3 and if \ cell address and cell length
     0 fill  exit   \ no, do it a byte at a time
   then
   dup dup xor >r   \ more compact "0 >r"
   2/ 2/ negate 1+  swap                ( -n a )
   | r@ swap !+ -rept nop
   r> 3drop
;

\ Software versions of math functions
\ May be replaced by user functions.

: lshift  \ x count -- x'               \ 6.1.1805
   1- -if: drop exit |
   63 and   negate  swap
   | 2* -rept nop swap drop ;
;

: rshift  \ x count                     \ 6.1.2162
   1- -if: drop exit |
   63 and   negate  swap
   | u2/ -rept nop swap drop ;
;

options 1 and [if]
: um*  \ u1 u2 -- ud                    \ 6.1.2360
   5 user ( u1 low )  swap  3 user
;
: *  \ n1 n2 -- n3                      \ 6.1.0090
   5 user ( u1 low )  swap drop
;
: um/mod \ ud u -- ur uq                \ 6.1.2370
   3 user drop                          \ set divisor
   4 user  ( x quot ) swap
   3 user  swap
;
[else]
: um*  \ u1 u2 -- ud                    \ 6.1.2360
   dup dup xor -32
   begin >r
   2* >r 2*c                            \ u1 u2' | count x'
   |ifc over r> + >r |                  \ add u1 to x
   |ifc 1+ |                            \ carry into u2
   r> r> 1+ +until drop
   >r >r drop r> r> swap
;
: *       um* drop ;                    \ 6.1.0090  n1 n2 -- n1*n2
: um/mod \ ud u -- ur uq                \ 6.1.2370
    2dup - drop
    ifnc
        -32  u2/ 2*                     \ clear carry
        begin
            >r >r  swap 2*c swap 2*c    \ dividend64 | count divisor32
            ifnc
                dup r@  - drop          \ test subtraction
                |ifc r@ - |             \ keep it  (carry is inverted)
                dup 2*c invert 2/ drop
            else                        \ carry out of dividend, so subtract
                r@ -   0 2* drop        \ clear carry
            then
            r> r> 1+                    \ L' H' divisor count
        +until
        drop drop swap 2*c invert       \ finish quotient
        exit
    then
    drop drop dup xor  dup 1-           \ overflow = 0 -1
;
[then]

: sm/rem  \ d n -- rem quot             \ 6.1.2214
   2dup xor >r  over >r  abs >r dabs r> um/mod
   swap r> 0< if  negate  then
   swap r> 0< if  negate  then ;

: fm/mod  \ d n -- rem quot             \ 6.1.1561
   dup >r  2dup xor >r  dup >r  abs >r dabs r> um/mod
   swap r> 0< if  negate  then
   swap r> 0< if  negate  over if  r@ rot -  swap 1-  then then
   r> drop ;

\ eForth model
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
: /mod   over 0< swap m/mod ;           \ 6.1.0240
: mod    /mod drop ;                    \ 6.1.1890
: /      /mod nip ;                     \ 6.1.0230

: min    2dup < ifz: swap | drop ;      \ 6.1.1870
: max    2dup swap < |ifz swap | drop ; \ 6.1.1880
: umin   2dup u< ifz: swap | drop ;
: umax   2dup swap u< |ifz swap | drop ;

: /string >r swap r@ + swap r> - ;      \ 17.6.1.0245  a u -- a+1 u-1
: within  over - >r - r> u< ;           \ 6.2.2440  u ulo uhi -- flag
: m*                                    \ 6.1.1810  n1 n2 -- d
    2dup xor 0< >r
    abs swap abs um*
    r> if dnegate then
;
: */mod  >r m* r> m/mod ;               \ 6.1.0110  n1 n2 n3 -- remainder n1*n2/n3
: */     */mod swap drop ;              \ 6.1.0100  n1 n2 n3 -- n1*n2/n3
: bye    1 user begin again ;           \ 15.6.2.0830  exit to OS if there is one

hex
: crc32  \ c-addr u -- crc              \ CRC32 of string, about 160 cycles per byte
   swap -1 >r                           ( u addr | crc )
   begin  over  while
      swap 1- swap
      count r> xor                      ( u addr' crc' )
      -8 begin  1+ >r                   \ do 8 times
         dup  1 and negate              ( u addr crc mask )
         EDB88320 and
         swap u2/ xor
      r> +until  drop  >r
   repeat
   2drop r> invert
;
decimal

\ This version expects two registers for the top of the data stack

: catch  \ xt -- exception# | 0         \ 9.6.1.0875
    over >r                             \ save N
    4 sp >r          \ xt               \ save data stack pointer
    handler @ >r     \ xt               \ and previous handler
    0 rp handler !   \ xt               \ set current handler = ret N sp handler
    execute                             \ execute returns if no throw
    r> handler !                        \ restore previous handler
    r> drop
    r> dup xor       \ 0                \ discard saved stack ptr
; call-only

: throw  \ ??? exc# -- ??? exc#         \ 9.6.1.2275
    dup ifz: drop exit |                \ Don't throw 0
    handler @ rp!   \ exc#              \ restore prev return stack
    r> handler !    \ exc#              \ restore prev handler
    r> swap >r      \ saved-sp          \ exc# is on return stack
    sp! drop nip
    r> r> swap      \ exc#              \ Change stack pointer
;

: unused    \ -- n                      \ 6.2.2395
   c_scope c@
   case
   0 of  [ RAMsize ROMsize + ] literal  here -  endof
   1 of  [ hp0               ] literal  here -  endof
         hp @ hp0 -  swap
   endcase
;

: roll                                  \ 6.2.2150
\ xu xu-1 ... x0 u -- xu-1 ... x0 xu
    dup ifz: drop exit |
    swap >r  1- recurse  r> swap
;

\ Note (from Standard): [compile] is obsolescent and is included as a
\ concession to existing implementations.
: [compile]  ' compile, ; immediate     \ 6.2.2530

\ http://www.forth200x.org/documents/html/core.html
: HOLDS  \ addr u --                    \ 6.2.1675
   BEGIN DUP WHILE 1- 2DUP + C@ HOLD REPEAT 2DROP
;

: alignh    hp @ aligned hp ! ;         \ --
: alignc    cp @ aligned cp ! ;         \ --
: align     h @ aligned h ! ;           \ 6.1.0705 --

: ]  1 state ! ;              \ --      \ 6.1.2540  resume compilation
: [  0 state ! ; immediate    \ --      \ 6.1.2500  interpret

: here      h @ ;                       \ 6.1.1650  -- a
: allot     h +! ;                      \ 6.1.0710  n --
: /allot    h @ swap dup allot erase ;  \           n --
: buffer:   h @ equ  allot ;            \ 6.2.0825  u <name> --
: constant  equ ;                       \ 6.1.0950  x <name> --
: variable  align h @ equ  4 /allot ;   \ 6.1.2410  <name> --

: d>s                  drop ; macro     \ 8.6.1.1140
: 2rot  2>r 2swap 2r> 2swap ;           \ 8.6.2.0420
: -2rot           2rot 2rot ;
: 2nip          2swap 2drop ;
: 2tuck         2swap 2over ;
: d-             dnegate d+ ;           \ 8.6.1.1050
: d<                  d- 0< ;
: d=                  d- 0= ;           \ 8.6.1.1120
: d0=                 or 0= ; macro     \ 8.6.1.1080  d -- flag
: d0<          swap drop 0< ; macro     \ 8.6.1.1075  d -- flag
: d2*      swap 2* swap 2*c ;           \ 8.6.1.1090  d -- d'
hex
: d2/
   2/ swap ifnc  2/ swap exit then      \ 8.6.1.1100  d -- d'
   80  0 litx  or  swap
;
decimal

: 2literal  \ d --                      \ 8.6.1.0390
   swap  postpone literal
   postpone literal
; immediate

: 2variable  \ <name> --                \ 8.6.1.0440
    create 2 cells /allot
;

: 2constant  \ d <name> --              \ 8.6.1.0360
    create , , does> 2@
;

: d< ( d1 d2 -- flag )                  \ 8.6.1.1110
   rot  2dup = if  2drop u<  exit then  2nip >
;
: du<  \ ud1 ud2 -- flag                \ 8.6.2.1270
   rot  2dup = if  2drop u<  exit then  2nip swap u<
;
: dmax  \ d1 d2 -- d3                   \ 8.6.1.1210
   2over 2over d< if  2swap  then  2drop
;
: dmin ( d d2 -- d3 )                   \ 8.6.1.1220
   2over 2over d< 0= if  2swap  then  2drop
;

: m+    s>d d+ ;                        \ 8.6.1.1830

: m*                                    \ 8.6.1.1820
    2dup xor >r
    abs swap abs um*
    r> 0< if dnegate then
;

\ From Wil Baden's "FPH Popular Extensions"
\ http://www.wilbaden.com/neil_bawd/fphpop.txt

: tnegate                           ( t . . -- -t . . )
    >r  2dup or dup if drop  dnegate 1  then
    r> +  negate ;

: t*                                ( d . n -- t . . )
                                    ( d0 d1 n)
    2dup xor >r                     ( r: sign)
    >r dabs r> abs
    2>r                             ( d0)( r: sign d1 n)
    r@ um* 0                        ( t0 d1 0)
    2r> um*                         ( t0 d1 0 d1*n .)( r: sign)
    d+                              ( t0 t1 t2)
    r> 0< if tnegate then ;

: t/                                ( t . . u -- d . )
                                    ( t0 t1 t2 u)
    over >r >r                      ( t0 t1 t2)( r: t2 u)
    dup 0< if tnegate then
    r@ um/mod                       ( t0 rem d1)
    rot rot                         ( d1 t0 rem)
    r> um/mod                       ( d1 rem' d0)( r: t2)
    nip swap                        ( d0 d1)
    r> 0< if dnegate then ;

: m*/  ( d . n u -- d . )  >r t*  r> t/ ;

: toupper  \ c -- c'
   dup [char] a [char] { within \ }
   32 and -
;
\ convert char to digit, return `ok` flag
: digit?  \ c base -- n ok?
   >r  toupper  [char] 0 -
   dup 10 17 within 0= >r               \ check your blind spot
   dup 9 > 7 and -
   r> over r> u< and                    \ check the base
;
\ convert string to number, stopping at first non-digit
: >number       \ ud a u -- ud a u      \ 6.1.0570
   begin dup
   while >r  dup >r c@ base @ digit?
   while swap base @ um* drop rot base @ um* d+ r> char+ r> 1-
   repeat drop r> r> then
;

: source    tibs 2@ ;                   \ -- addr len  entire source string
: /source   source >in @ /string ;      \ -- addr len  remaining source string

: skip  \ c-addr1 u1 char -- c-addr2 u2
   >r begin
      dup ifz: r> drop exit |           \ zero length quits
      over c@ r@ xor 0=
      |ifz r> drop exit |               \ mismatch quits
      1 /string
   again
;
: scan  \ c-addr1 u1 char -- c-addr2 u2
   >r begin
      dup ifz: r> drop exit |           \ zero length quits
      over c@ r@ xor
      |ifz r> drop exit |               \ mismatch quits
      1 /string
   again
;
: parse  \ char "ccc<char>" -- c-addr u \ 6.2.2008
   /source  over >r  rot scan  r>  rot over -  rot 0<>
   1 and over + >in +!
;
\ Version of parse that skips leading delimiters
: _parse  \ char "<chars>ccc<char>" -- c-addr u
   >r  /source over  r@ swap >r         \ a u char | char a
   skip drop r> - >in +!  r> parse      \ a u
;
: parse-name                            \ 6.2.2020
   bl _parse  \ "<spaces>name" -- c-addr u
;
: word  \ c "<ccc>string<c>" -- c-addr  \ 6.1.2450
   _parse pad  dup >r c!
   r@ c@+ cmove  r>                     \ use pad as temporary
;

: .(    \ "string)" --                  \ 6.2.0200
    [char] ) parse type                 \ parse to output
;
: char  \ "char" -- n                   \ 6.1.0895
   parse-name drop c@
;

\ A version of FIND that accepts a counted string and returns a header token.
\ `toupper` converts a character to lowercase
\ `c_casesens` is the case-sensitive flag
\ `match` checks two strings for mismatch and keeps the first string
\ `_hfind` searches one wordlist for the string

: match  \ a1 n1 a2 n2 -- a1 n1 0 | nonzero
   third over xor 0=                    \ n2 <> n1
   |ifz drop dup xor exit |             \ a1 n1 0 | a1 n1 a2 n2
   drop over >r third >r  swap negate   \ a1 a2 -n | n1 a1
   begin >r
      c@+ >r swap c@+ r>                \ a2' a1' c1 c2 | n1 a1 -n
      c_casesens c@ 0= if
         toupper swap toupper           \ not case sensitive
      then
      xor if                            \ mismatch
         r> 3drop r> r>
         dup dup xor exit               \ 0
      then
   r> 1+ +until
   3drop r> drop r> 1+                  \ flag is n1+1
;
: _hfind  \ addr len wid -- addr len 0 | ht    Search one wordlist
   @ over 31 u> -19 and throw           \ name too long
   over ifz: dup xor exit |             \ addr 0 0   zero length string
   begin
      dup >r cell+ c@+  63 and          \ addr len 'name length | wid
      match if
         r> exit                        \ return only the ht
      then
      r> link>
   dup 0= until                         \ not found
;
: hfind  \ addr len -- addr len | 0 ht  \ search the search order
   c_wids c@ begin
      1- |-if 2drop exit |              \ finished, not found
      >r
      r@ cells context + @ _hfind       \ wid
      ?dup if
        r> dup xor swap exit
      then
      r> dup
   again
;
: CaseInsensitive  0 c_casesens c! ;
: CaseSensitive    1 c_casesens c! ;

\ Header space:     W len xtc xte link name
\ offset from ht: -16 -12  -8  -4    0 4

: h'  \ "name" -- ht
   parse-name  hfind  swap 0<>          \ len -1 | ht 0
   -13 and throw                        \ header not found
;
: '   \ "name" -- xte                   \ 6.1.0070
   h' cell- link>
;


\ Recognize a string as a number if possible.

\ Formats:
\ Leading minus applies to entire string.
\ A decimal anywhere means it's a 32-bit floating point number.
\ If string is bracketed by '' and inside is a valid utf-8 sequence, it's xchar.
\ $ prefix is hex.

\ get sign from start of string
: numbersign  \ addr u -- addr' u' sign
   over c@  [char] - =  dup >r
   if 1 /string then        r>
;
\ Attempt to convert to an integer in the current base
: tonumber  \ addr len -- n
   0 dup 2swap >number 0<> -13 and throw  2drop
;
hex
\ Attempt to convert utf-8 code point
: nextutf8  \ n a -- n' a'              \ add to utf-8 xchar
   >r 6 lshift r> count                 \ expect 80..BF
   dup 0C0 and 80 <> -0D and throw      \ n' a c
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
\ Attempt to convert string to a number
: isnumber  \ addr u -- n
   numbersign  >r                       \ accept leading '-'
   2dup [char] . scan nip if            \ is there a decimal?
      -40 throw                         \ floats not implemented
   then
   dup 2 > if                           \ possible 'c' or 0x
      2dup over + 1-  c@ swap c@        \ a u c1 c2
      over = swap [char] ' = and        \ a u  is enclosed by ''
      if  1 /string 1- isutf8           \ attempt character
         r> if negate then  exit
      then
      over c@ [char] $ = if             \ leading $
         base @ >r hex  1 /string  tonumber
         r> base !
         r> if negate then  exit
      then
   then
   tonumber  r> if negate then
;

\ These expect a Forth QUIT loop for THROW to work.

: interpret
   begin  \ >in @ w_>in w!              \ save position in case of error (remove)
      parse-name  dup                   \ next blank-delimited string
   while
      hfind                             \ addr len | 0 ht
      over if
         isnumber
         state @ if literal, then
      else
         nip
         dup head !                     \ save last found word's head
         state @ 0= 0= 4 and            \ get offset to the xt
         cell+ -  link>                 \ get xtc or xte
         dup 8388608 and 0<>            \ is it a C function?
         -21 and throw                  \ that's a problem
         execute                        \ execute the xt
      then
      depth 0< -4 and throw             \ stack underflow
   repeat  2drop
;

: emit    0 personality_exec ;          \ 6.1.1320  xchar --
: cr      1 personality_exec ;          \ 6.1.0990  --
: page    2 personality_exec ;          \  clear screen
: key?    3 personality_exec ;          \  check for key
: key     4 personality_exec ;          \ 6.1.1750  -- c

:noname \ addr len --                   \ 6.1.2310  send chars
   dup ifz: 2drop exit | negate
   begin 1+ >r  count emit  r> +until
   2drop
; is type

: space   bl emit ;                     \ 6.1.2220  --
: spaces  begin |-if drop exit |        \ 6.1.2230  n --
          space 1- again ;

\ Numeric conversion, from eForth mostly.
\ Output is built starting at the end of a fixed `pad` of `|pad|` bytes.
\ Many Forths use RAM ahead of HERE (RAM scope) for PAD.

: digit   dup -10 + 0< -7 and + [char] 7 + ;
: <#      [ pad |pad| + ] literal       \ 6.1.0490
          hld ! ;
: hold    hld dup >r @ 1- dup r> ! c! ; \ 6.1.1670

: #    \ ud -- ud/base
    dup  base @ >r  if
		0 r@ um/mod r> swap             \ 6.1.0030
          >r um/mod swap digit hold r> exit
	then  r> um/mod swap digit hold  	\ ud is 32-bit number
	dup dup xor
;

: #s      begin # 2dup or 0= until ;    \ 6.1.0050
: sign    0< if [char] - hold then ;    \ 6.1.2210
: #>      2drop hld @ [ pad |pad| + ] literal over - ; \ 6.1.0040
: s.r     over - spaces type ;
: d.r     >r dup >r dabs                \ 8.6.1.1070  d width --
          <# #s r> sign #> r> s.r ;
: u.r     0 swap d.r ;                  \ 6.2.2330  u width --
: .r      >r s>d r> d.r ;               \ 6.2.0210  n width --
: d.      0 d.r space ;                 \ 8.6.1.1060  d --
: u.      0 d. ;                        \ 6.1.2320  u --
: .       base @ 10 xor if              \ 6.1.0180  n|u
             u. exit                    \           unsigned if hex
          then  s>d d. ;                \           signed if decimal
: ?       @ . ;                         \ 15.6.1.0220  a --
: <#>     <# negate begin >r # r> 1+ +until drop #s #> ;  \ ud digits-1
: h.x     base @ >r hex  0 swap <#> r> base !  type space ;

: compare  \ c-addr1 u1 c-addr2 u2 -- n \ 17.6.1.0935
   rot swap  2dup max >r
   - r> swap >r >r 						\ a1 a2 | sign count
   rfor
      c@+ >r swap c@+ >r swap           \ a1' a2' | sign count c2 c1
	  r> r> -                           \ a1' a2' diff
	  dup if
	     r> drop  r> drop				\ unloop
	     >r 2drop r> 0< 2* 1+ exit
      then drop
   next  2drop
   r> dup if							\ got all the way through
      0< 2* 1+ exit
   then
;

: blank  bl fill ;  \ c-addr len        \ 17.6.1.0780

: -trailing  \ a u -- a u'              \ 17.6.1.0170
   begin  2dup + 1- c@ bl =  over and   while
   1- repeat
;

\ Search the string specified by c-addr1 u1 for the string
\ specified by c-addr2 u2. If flag is true, a match was found
\ at c-addr3 with u3 characters remaining. If flag is false
\ there was no match and c-addr3 is c-addr1 and u3 is u1.

: search  \ a1 u1 a2 u2 -- a3 u3 flag   \ 17.6.1.2191
   dup ifz: drop exit |   \ special-case zero-length search
   2>r 2dup
   begin
      dup
   while
      2dup 2r@            ( c-addr1 u1 c-addr2 u2 )
      rot over min -rot   ( c-addr1 min_u1_u2 c-addr2 u2 )
      compare 0= if
         2swap 2drop 2r> 2drop true exit
      then
      1 /string
   repeat
   2drop 2r> 2drop
   false
;

: \       tibs @ >in ! ; immediate      \ 6.2.2535  --
: literal literal, ; immediate          \ 6.1.1780  x --
: [char]  char literal, ; immediate     \ 6.1.2520  <xchar> --
: exit    ,exit ; immediate             \ 6.1.1380
: chars   ; immediate                   \ 6.1.0898
: (       [char] ) parse 2drop
; immediate

: [']   \ "name" --                     \ 6.1.2510
   ' literal,
; immediate

: xtextc  \ head -- xte xtc
   8 - dup  cell+ link>  swap link>
;
: xtflag  \ ht -- xt flag
   xtextc ['] do-immediate  =  invert 2* 1+
;
: search-wordlist                       \ 16.6.1.2192
\ c-addr u wid -- 0 | xt flag
   _hfind  dup if xtflag exit then  2drop
;

: postpone \ "name" --                  \ 6.1.2033
   h'  xtextc                           \ xte xtc
   ['] do-immediate =                   \ immediate?
   if   compile, exit   then            \ compile it
   literal,  ['] compile, compile,      \ postpone it
; immediate

: recurse  \ --                         \ 6.1.2120
   -4 last link> compile,
; immediate

\ ------------------------------------------------------------------------------
\ Control structures
\ see compiler.f

: NoExecute   \ --                      \ Must be compiling
   state @ 0= -14 and throw
;
: NeedSlot  \ slot -- addr
   NoExecute
   cp @ swap 64000 > if cell+ 1+ 1+ then \ large address, leave room for >16-bit address
   c_slot c@ > if NewGroup then
   cp @
;
hex
: _branch   \ again --
   op_jmp Explicit                      \ addr slot
;
: _jump     \ addr -- then
   c_slot c@  -1 over lshift invert     \ create a blank address field
   _branch  18 lshift +                 \ pack the slot and address
   60000000 +							\ tag = 3, forward reference
;
: then      \ C: then --                \ 6.1.2270
   NoExecute  NewGroup  				\ then -- addr slot tag
   dup FFFFFF and
   swap 18 rshift
   dup 1F and  swap 5 rshift			\ addr slot tag
   3 <> -16 and throw					\ bogus tag
   -1 swap lshift  cp @ u2/ u2/ +       \ addr field
   swap ROM!
; immediate

: begin     \ C: -- again               \ 6.1.0760
   NoExecute  NewGroup  cp @
   40000000 +							\ tag = 2, backward mark
; immediate

: again     \ C: again --               \ 6.2.0700
   NoExecute  2/ 2/
   dup 38000000
   and 10000000 <> -16 and throw		\ expect tag of 2
   3FFFFF and _branch
; immediate

decimal

: ahead     \ C: -- then                \ 15.6.2.0702
   14 NeedSlot  _jump
; immediate
: ifnc      \ C: -- then
   20 NeedSlot
   op_ifc: Implicit  _jump
; immediate
: if        \ C: -- then | E: flag --   \ 6.1.1700
   20 NeedSlot
   op_ifz: Implicit  _jump
; immediate
: +if       \ C: -- then | E: n -- n
   20 NeedSlot
   op_-if: Implicit  _jump
; immediate
: else      \ C: then1 -- then2         \ 6.1.1310
   postpone ahead  swap
   postpone then
; immediate
: while     \ C: again -- then again    \ 6.1.2430
   NoExecute  >r  postpone if  r>
; immediate \ E: flag --
: +while    \ C: again -- then again
   NoExecute  >r  postpone +if  r>
; immediate \ E: flag --
: repeat    \ C: then again --          \ 6.1.2140
   postpone again
   postpone then
; immediate
: +until    \ C: again -- | E: n -- n
   20 NeedSlot drop
   op_-if: Implicit
   postpone again
; immediate
: until    \ C: again -- | E: flag --   \ 6.1.2390
   20 NeedSlot drop
   op_ifz: Implicit
   postpone again
; immediate
: rfor   \ C: -- again then | E: cnt -- R: -- cnt
   postpone begin   postpone (for)
   postpone ahead
; immediate
: for     \ C: -- again then | E: cnt -- R: -- cnt
   op_>r Implicit
   postpone rfor
; immediate
: next    \ C: -- again then | E: R: -- cnt
   swap  postpone again  postpone then
; immediate

\ CREATE DOES>

hex
: _create  \ n "name" --
   cp @  ['] get-compile  header[
   dup  pad w!+  c!                 \ group count = n, tag byte = n
   0C0 flags!                           \ flags: jumpable, anon
   ]header   NewGroup
;
: defer  \ <name> --
   1 _create
   3FFFFFF op_jmp Explicit
;
: is  \ xt --
   2/ 2/  4000000 -  '  ROM!            \ resolve forward jump
;
: create  \ -- | -- addr                \ 6.1.1000
   2 _create  align
   here  c_scope c@ 1 = 8 and +  literal,  \ skip forward if in ROM
   op_exit  Implicit
   iracc @
   -40 c_slot c@ lshift invert +		\ last group is "exit" or "com exit"
   ,c  ClearIR
;

: iscom?  \ 'group -- flag				\ does the group have a COM in slot 0?
   @ [ -1   1A lshift ] literal  and
   [ op_com 1A lshift ] literal  xor 0=
;

: _>body  \ xt -- body link flag		\ flag = T if there is no DOES> field
   @+ FFFFF and  swap          			( literal 'group )
   dup @ >r  iscom? if
      invert  r> 3FFF
   else
      r> FFFFF							( body group mask )
   then
   dup >r and  dup r> =
;

: >body  \ xt -- body                   \ 6.1.0550
   _>body  2drop
;

decimal

\ DOES> ends the current thread and causes the last CREATE to pick it up.
\ The COM EXIT or EXIT are cleared to COM NOP or NOP so that a JMP is executed.

: does>  \ RA --                        \ 6.1.1250
   r> 2/ 2/                             \ jump here, jdest
   -4 last link> cell+  dup >r
   iscom? if
	  [ op_jmp 14 lshift  op_exit invert 20 lshift  + ] literal
   else \ slot0 = com, must be "com exit"
	  [ op_jmp 20 lshift  op_exit invert 26 lshift  + ] literal
   then
   +  r> ROM!           				\ resolve the does> field
;

\ Eaker CASE statement
\ Note: Number of cases is limited by the stack headspace.
\ For a depth of 64 cells, that's 32 items.

: case      \ C: -- 0                   \ 6.2.0873
   0
; immediate
: of        \ C: -- then                \ 6.2.1950
   postpone (of)  postpone ahead
; immediate \ E: x1 x2 -- | --
: endof     \ C: then -- again          \ 6.2.1343
   postpone else
; immediate
: endcase   \ C: 0 a1 a2 ... | E: x --  \ 6.2.1342
   postpone drop
   begin ?dup while
      postpone then
   repeat
; immediate

\ PAD is LEAVE stack

: do  \ -- then again                   / 6.1.1240
   NoExecute  NewGroup
   op_swap Implicit   op_com  Implicit
   op_1+   Implicit   op_>r   Implicit   op_>r   Implicit
   postpone begin  0 pad !
; immediate
: ?do  \ -- then again                  / 6.2.0620
   NoExecute
   postpone (?do)  postpone ahead  4 pad 2!
   postpone begin
; immediate

: pushLV  pad dup >r @+  dup cell+ r> ! + ! ;   \ n --
: popLV   pad dup dup @ + @ >r -4 swap +! r> ;  \ -- n

: leave  \ pad: ... -- ... <then>       \ 6.1.1760
   NoExecute  postpone unloop  postpone ahead  pushLV
; immediate

: _leaves  \ pad: <thens>...
   begin  pad @  while
      popLV  postpone then
   repeat
;
: loop  \ again --                      \ 6.1.1800
   NoExecute  postpone (loop)  postpone again  _leaves
; immediate
: +loop  \ again --                     \ 6.1.0140
   NoExecute  postpone (+loop)  postpone again  _leaves
; immediate
: i  \ index                            \ 6.1.1680
   NoExecute  op_r@ Implicit
; immediate

\ Embedded strings

: ,"   \ string" --
   [char] " parse  ( addr len )
   dup >r c,c                           \ store count
   cp @ r@ ROMmove                      \ and string in code space
   r> cp +!
;
: _x"  \ string" --
   postpone (string)  ," alignc			\ compile embedded string
;
: ."   \ string" --                     \ 6.1.0190
   _x"  op_c@+ Implicit
   postpone type
; immediate

: abort  -1 throw ;  \ --               \ 6.1.0670

: abort" \ ( string" -- | e: i * x x1 -- | i * x ) ( r: j * x -- | j * x )
   postpone if                          \ 6.1.0680
   postpone ."  \ "
   postpone abort
   postpone then
; immediate

