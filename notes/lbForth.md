# [lbForth](https://github.com/larsbrinkhoff/lbForth)

## [C Target Nucleus](https://github.com/larsbrinkhoff/lbForth/blob/master/targets/c/nucleus.fth)

`cold` `exit` `dodoes` `0branch` `(literal)` `!` `@` `+` `>r` `r>` `nand` `c!` `c@` `emit` `bye`
`close-file` `open-file` `read-file`

\ Possible, but slow, implementation of 0branch.
\ : select   0= dup invert swap rot nand invert rot rot nand invert + ;
\ : 0branch   r> dup cell+ swap @ rot select >r ;

\ This works, but is too slow.
\ create 'cell   cell ,
\ variable temp
\ : (literal)   r> temp ! temp @ temp @ 'cell @ + >r @ ;

\ Alternative (slow +)
\ : +   begin ?dup while 2dup xor -rot and 2* repeat ;

\ This works, but is too slow.
\ : >r   r@ rp@ -4 + rp! rp@ ! rp@ 4 + ! ;

\ This works, but is too slow.
\ : r>   rp@ 4 + @ r@ rp@ 4 + rp! rp@ ! ;

## [Kernel](https://github.com/larsbrinkhoff/lbForth/blob/master/src/kernel.fth)

Primitives expected: dodoes exit 0branch (literal) ! @ c! c@ + nand >r r> emit open-file read-file close-file

h: :   parse-name header, docol, ] ;
h: create   parse-name header, dovar, ;
h: variable   create cell allot ;
h: defer   parse-name header, dodef, compile abort ;
h: constant   parse-name header, docon, , ;
h: value   constant ;
h: immediate   latest >nfa dup c@ negate swap c! ;
h: to   ' >body ! ;
h: is   ' >body ! ;

h: if   compile 0branch >mark ;
h: ahead   compile branch >mark ;
h: then   >resolve ;

h: begin   <mark ;
h: again   compile branch <resolve ;
h: until   compile 0branch <resolve ;

h: else   [compile] ahead swap [compile] then ;
h: while    [compile] if swap ;
h: repeat   [compile] again [compile]  then ;

h: to   ' >body t-literal compile ! ;
h: is   [compile] to ;

h: do   0leaves  compile 2>r  [compile] begin ;
h: loop   compile (loop) [compile] until  here leaves@ chains!  compile 2rdrop ;
h: leave   compile branch  leaves chain, ;

h: abort"   [compile] if [compile] s" compile cr compile type
   compile cr compile abort [compile] then ;

: .(   [char] ) parse type ; immediate

: 0<>   if -1 else 0 then ;
: 0>    0 > ;

: 2r@   r> 2r> 2dup 2>r rot >r ;

: :noname   noheader, docol, ] latestxt !csp ;

: (?do)   r> 2r> 2dup > rot rot 2>r swap >r ;
: ?do   leaves @  0 leaves !
   postpone 2>r postpone begin postpone (?do) postpone if ; compile-only

: string+   count + aligned ;
: (c")   r> dup string+ >r ;
: string,   dup c, ", ;
: ,c"   parse" string, ;
: c"   postpone (c") ,c" ; compile-only

: convert   char+ 65535 >number drop ;

: case   0 ; compile-only
: of   postpone over  postpone =  postpone if  postpone drop ; compile-only
: endof   postpone else  swap 1+ ; compile-only
: endcase   postpone drop  0 ?do postpone then loop ; compile-only

: erase   0 fill ;

variable span
: expect   accept span ! ;

: true    -1 ;
: false   0 ;

: hex   16 base ! ;

: pick   ?dup if swap >r 1- recurse r> swap exit then dup ;
: roll   ?dup if swap >r 1- recurse r> swap then ;

: query   terminal-input  refill drop ;

: value   header docon, , ;
: to      ' >body ! ;
: to      ' >body postpone literal postpone ! ; compile-only
: +to     ' >body +! ;
: +to     ' >body postpone literal postpone +! ; compile-only

: @+ ( addr -- a' x )   dup cell+ swap @ ;

create voc-link  ' env-words ,

: current,   current @ , ;
: context,   context 9 cells move, ;
: latestxt,   latestxt , ;
: voc-link,   voc-link @ , ;
: vocs,   voc-link begin @ ?dup while >body dup @ , cell+ repeat ;
: marker,   current, context, latestxt, voc-link, vocs, ;

: here!   dup dp ! ;
: current!   @+ current ! ;
: context!   dup context 9 cells cmove  9 cells + ;
: latestxt!   @+ to latestxt ;
: voc-link!   @+ voc-link ! ;
: voc! ( a1 a2 -- a1' a2' ) >body >r @+ r@ ! r> ;
: vocs!   voc-link begin @ ?dup while voc! cell+ repeat ;
: marker!    here! current! context! latestxt! voc-link! vocs! drop ;

: marker   here marker, create ,  does> @ marker! ;

?: tuck    swap over ;

: (.r) ( n f u -- )   0 <# #s rot sign #> rot over - spaces type ;
: u.r   0 rot (.r) ;
: .r   swap s>d swap abs (.r) ;

: u>   swap u< ;

: unused   limit @ here - ;

: within   over - under - u< ;

: [compile]   ' compile, ; compile-only

\ ----------------------------------------------------------------------

( Forth2012 core extension words. )

: buffer:   create allot ;

: alias ( xt "name" -- ) header dodef, , ;

: defer   ['] abort alias ;
: defer!   >body ! ;
: defer@   >body @ ;
: is   ' defer! ;
: is   postpone ['] postpone defer! ; compile-only
: action-of   ' defer@ ;
: action-of   postpone ['] postpone defer@ ; compile-only
: +is ( xt "name" -- ) >r :noname r> compile, ' dup defer@ compile, >r postpone ; r> defer! ;

: holds   bounds swap begin 2dup < while 1- dup c@ hold repeat 2drop ;

parse-name : header, docol, ]   parse-name header, docol, ] !csp exit [ reveal

: immediate   latestxt >nfa dup c@ negate swap c! ;
: \   refill drop ; immediate

\ This is the first file to be loaded and compiled by the kernel.
\ Since the kernel only defines a bare minimum of words, we have to
\ build basic language constructs almost from scratch.

: >mark      here 0 , ;
: >resolve   here swap ! ;
: <mark      here ;
: <resolve   , ;

: char   parse-name drop c@ ;
: '   parse-name find-name 0= ?undef ;

: hide   latestxt >nextxt  current @ >body ! ;
: link   current @ >body @  latestxt >lfa ! ;
: relink   hide >current link reveal current> ;
: compile-only   [ ' compiler-words ] literal relink  immediate ;

: dodoes_code   [ ' dodoes >code @ ] literal ;
: does>     [ ' (does>) ] literal compile,  dodoes_code does, ; compile-only
: "create   header, dovar, reveal ;
: create    parse-name "create ;

: postpone,   [ ' literal compile, ' compile, ] literal compile, ;
\ Same as:    postpone literal  postpone compile, ;

: finders   create ' , ' , ' ,   does> >r find-name 1+ cells r> + @ execute ;

finders postpone-name   postpone, abort compile,

: postpone   parse-name postpone-name ; compile-only

: unresolved   postpone postpone  postpone >mark ; immediate
: ahead   unresolved branch ; compile-only
: if   unresolved 0branch ; compile-only
: then   >resolve ; compile-only

: resolving   postpone postpone  postpone <resolve ; immediate
: begin   <mark ; compile-only
: again   resolving branch ; compile-only
: until   resolving 0branch ; compile-only

: else   postpone ahead swap postpone then ; compile-only
: while   postpone if swap ; compile-only
: repeat   postpone again postpone then ; compile-only

: recurse   latestxt compile, ; compile-only

: 1-   -1 + ;

: parse   >r  source drop >in @ +
   0 begin source? while <source r@ <> while 1+ repeat then r> drop ;

: [char]   char postpone literal ; compile-only

: (   begin [char] ) parse 2drop
      source drop >in @ 1- + c@ [char] ) <> while
      refill while repeat then ; immediate

create squote   128 allot

: parse"   [char] " parse ;
: s,   dup , ", ;
: ,s"   parse" s, ;
: s"   parse" >r squote r@ cmove  squote r> ;
: s"   postpone (sliteral) ,s" ; compile-only
: ,s"  postpone s" postpone s, ; compile-only
: ,"   parse" move, ;
: ,"   postpone s" postpone move, ; compile-only

: (abort)   sp0 sp!  quit ;
' (abort) ' abort >body !

: ((abort"))   cr type cr abort ;
' ((abort")) ' (abort") >body !
: abort"   postpone if postpone s" postpone (abort") postpone then ;
   compile-only

: ?:   >in @ >r  parse-name find-name
   if r> 2drop  begin source 1- + c@ [char] ; = refill drop until
   else 2drop r> >in ! : then ;

?: and   nand invert ;

?: 2*    dup + ;

?: *   1 2>r 0 swap begin r@ while
          2r> 2dup 2* 2>r and if swap over + swap then 2*
       repeat 2r> 3drop ;

: under   postpone >r ' compile, postpone r> ; immediate

: bits/cell   0 1 begin ?dup while 2* under 1+ repeat
   postpone literal ; immediate

: rshift   >r 0 begin r> dup bits/cell < while 1+ >r
           2* over 0< if 1+ then under 2* repeat drop nip ;
\ Since "an ambiguous condition exists if u is greater than or equal
\ to the number of bits in a cell", this would be acceptable.
\ : rshift   0 bits/cell rot do 2* over 0< if 1+ then under 2* loop nip ;
: lshift   begin ?dup while 1- under 2* repeat ;

: >   swap < ;

: u/mod ( n d -- r q )
    ?dup 0= abort" Division by zero"
    0 >r 2>r		\ R: q n
    0 1 begin ?dup while dup 2* repeat
    r> 0 begin		\ S: [...1<<i...] d r
      2*		\ r <<= 1
      r@ 0< if 1+ then	\ if n&msb then r++
      r> 2* >r		\ n <<= 1
      2dup > if rot drop else \ if r>=d
        over -		      \ r -= d
        rot r> r> rot + >r >r \ q += 1<<i
      then
      2>r ?dup 2r> rot 0= until
    nip r> drop r> ;

: +-   0< if negate then ;
: abs   dup 0< if negate then ;
: /mod   2dup xor >r abs under abs u/mod r> +- ;

create base  10 ,

: header   parse-name header, reveal ;
: constant   header docon, , ;

32 constant bl
: space   bl emit ;
: ?.-     dup 0< if [char] - emit negate then ;
: digit   dup 9 > if [ char A 10 - ] literal else [char] 0 then + ;
: (.)     base @ u/mod  ?dup if recurse then  digit emit ;
: u.      (.) space ;
: .       ?.- u. ;

: ."   [char] " parse type ;
: ."   postpone s"  postpone type ; compile-only

: postpone-number   undef ;
' postpone-number  ' postpone-name >body cell+ !

: /     /mod nip ;
: mod   /mod drop ;
: 2/    dup [ 0 invert 1 rshift invert ] literal and swap 1 rshift or ;

: 2@      dup cell+ @ swap @ ;
: 2!      swap over ! cell+ ! ;
: 2over   >r >r 2dup r> rot rot r> rot rot ;
: 2swap   >r rot rot r> rot rot ;

: chars   ;
: char+   1 chars + ;

: decimal   10 base ! ;

: depth   sp0 sp@ - cell / 1- ;

: variable   create cell allot ;

variable leaves

\ TODO: merge with metacompiler forward reference resolution.
: >resolve@   @ begin ?dup while dup @ here rot ! repeat ;

: r+   r> r> rot + >r >r ;

: do   leaves @  0 leaves !  postpone 2>r  postpone begin  0 ; compile-only
: leave   postpone branch  here leaves chain, ; compile-only
: +loop   ?dup if swap postpone r+ postpone again postpone then
   else postpone (+loop) postpone until then
   leaves >resolve@  leaves !  postpone unloop ; compile-only
: loop   1 postpone literal postpone +loop ; compile-only

: j   rp@ 3 cells + @ ;

: search-wordlist ( ca u wl -- 0 | xt 1 | xt -1 )
   (find) ?dup 0= if 2drop 0 then ;

create env-words  0 , ' included-files ,
: env-query   dup if drop execute -1 then ;
: environment?   #name min [ ' env-words ] literal search-wordlist env-query ;
: environment   [ ' env-words ] literal relink ;

: core ; environment
: address-unit-bits 8 ; environment
: /counted-string 255 ; environment
: max-char 255 ; environment
: lbforth ; environment

: fill   rot rot ?dup if bounds do dup i c! loop drop else 2drop then ;

: max   2dup > if drop else nip then ;

: s>d   dup 0< ;

: pad   here 1024 + ;

?: um/mod   nip u/mod ;

: base/mod ( ud1 -- ud2 u2 ) 0 base @ um/mod >r base @ um/mod r> rot ;

variable hld
: <#     pad hld ! ;
: hold   -1 hld +!  hld @ c! ;
: #      base/mod digit hold ;
: #s     begin # 2dup or 0= until ;
: sign   0< if [char] - hold then ;
: #>     2drop hld @  pad hld @ - ;

: spaces   ?dup 0 > if 0 do space loop then ;

: u<   2dup xor 0< if nip 0< else - 0< then ;
: u+d ( u1 u2 -- d )   dup rot + dup rot u< negate ;
: d+   >r rot u+d rot + r> + ;
: d+-   0< if invert swap invert 1 u+d rot + then ;
: um*   1 2>r 0 0 rot 0 begin r@ while
           2r> 2dup 2* 2>r and if 2swap 2over d+ 2swap then 2dup d+
        repeat 2drop 2r> 2drop ;
: m*   2dup xor >r abs swap abs um* r> d+- ;

: dnegate   invert swap invert 1 u+d rot + ;
: dabs      dup 0< if dnegate then ;

: sm/rem   2dup xor >r under dabs abs um/mod r> +- ;

\ TODO: implement this stub
: fm/mod   drop ;

: */mod    under m* sm/rem ;
: */       */mod nip ;

\ : dum* ( ud u -- ud' )   dup >r um* drop swap r> um* rot + ;
\ : dum/mod ( ud1 u1 -- ud2 u2 )   dup under u/mod swap under um/mod ;

: [']   ' postpone literal ; compile-only

: string-refill   0 ;

create string-source   0 -1 ' string-refill ' noop source,

: string-input ( a u -- )   string-source input !  0 >in !
   #source !  'source ! ;

: n>r   r> over >r swap begin ?dup while rot r> 2>r 1 - repeat >r ;
: nr>   r> r@ begin ?dup while 2r> >r rot rot 1 - repeat r> swap >r ;

: evaluate   save-input n>r  string-input interpret
   nr> restore-input abort" Bad restore-input" ;

create tib   256 allot

: key   here dup 1 stdin read-file if bye then  0= if bye then  c@ ;

: terminal-refill   tib 256 bounds do
      key dup 10 = if drop leave then
      i c!  1 #source +!
   loop -1 ;

: ok   state @ 0= if ."  ok" cr then ;

create terminal-source   tib 0 ' terminal-refill ' ok source,

: terminal-input   terminal-source input ! ;

: rp\   s" rp!" find-name if drop postpone \ else 2drop then ;
rp\ : rp!   postpone (literal) RP , postpone ! ; immediate

: (quit)   rp0 rp!  0 csp !  postpone [  terminal-input interpreting  bye ;

' (quit) ' quit >body !

: accept   save-input n>r  terminal-input  refill 0= if 2drop 0 exit then
   2>r source r> min swap over r> swap cmove  nr> restore-input abort" ?" ;

: uncount   under 1- over c! ;
: skip   begin source? while <source over = while repeat -1 >in +! then drop ;
: word   dup skip parse uncount ;
: find   count find-name ?dup 0= if uncount 0 then ;

: base*+ ( d u -- d' )   >r >r base @ um* r> base @ * +  r> 0 d+ ;

: 1/string ( a u -- a' u' )   1- under 1+ ;

: c>number ( c -- u )   [char] 0 -
   dup 9 > if [ char A char 0 - 10 - ] literal - else exit then
   dup 10 < if drop 36 exit then
   dup 35 > if [ char a char A - ] literal - then
   dup 10 < if drop 36 exit then ;

: u>number ( d a u -- d' a' u' )
   2>r begin 2r> 2dup 2>r while
      c@ c>number dup -1 > while  dup base @ < while
      base*+  2r> 1/string 2>r
   repeat then then drop 2r> ;

: >number   dup 0= if exit then
   2dup 2>r
   over c@ [char] - = dup >r if 1/string then
   u>number 2swap r@ d+- 2swap
   dup r> r@ + = if 2drop 2r> else 2r> 2drop then ;

create str  0 , 0 ,
: !str   str 2! ;
: @str   str 2@ ;
: 4drop  2drop 2drop ;

: (number) ( a u -- ) !str 0 0 @str >number @str rot ?undef 4drop ?literal ;

' (number) ' number >body !

: noop ;

: cell    cell ; \ Metacompiler knows what to do.
: cell+   cell + ;

?: sp@   SP @ cell + ;
?: sp!   SP ! ;
?: rp@   RP @ cell + ;
\ rp! in core.fth

variable  temp
?: drop    temp ! ;
?: 2drop   drop drop ;
: 3drop   2drop drop ;

?: r@   rp@ cell+ @ ;

?: swap   >r temp ! r> temp @ ;
?: over   >r >r r@ r> temp ! r> temp @ ;
?: rot    >r swap r> swap ;

?: 2>r   r> swap rot >r >r >r ;
?: 2r>   r> r> r> rot >r swap ;

?: dup    sp@ @ ;
?: 2dup   over over ;
: 3dup   >r >r r@ over 2r> over >r rot swap r> ;
?: ?dup   dup if dup then ;

?: nip    swap drop ;

?: invert   -1 nand ;
?: negate   invert 1 + ;
?: -        negate + ;

?: branch    r> @ >r ;
forward: <
: (+loop)   r> swap r> + r@ over >r < invert swap >r ;
: unloop    r> 2r> 2drop >r ;

?: 1+   1 + ;
?: +!   swap over @ + swap ! ;
?: 0=   if 0 else -1 then ;
?: =    - 0= ;
?: <>   = 0= ;

: min   2dup < if drop else nip then ;

: bounds   over + swap ;
: count    dup 1+ swap c@ ;

: aligned   cell + 1 - cell negate nand invert ;
?: (sliteral)   r> dup @ swap cell+ 2dup + aligned >r swap ;

: i    r> r@ swap >r ;
?: cr   10 emit ;
: type   ?dup if bounds do i c@ emit loop else drop then ;

\ Put the xt inside the definition of EXECUTE, overwriting the last noop.
?: execute   [ here cell + ] ['] noop ! then noop ;
?: perform   @ execute ;

variable state

?: 0<   [ 1 cell 8 * 1 - lshift ] literal nand invert if -1 else 0 then ;
?: or   invert swap invert nand ;
?: xor   2dup nand 1+ dup + + + ;
?: <   2dup xor 0< if drop 0< else - 0< then ;

: cmove ( addr1 addr2 n -- )   ?dup if bounds do count i c! loop drop
   else 2drop then ;

: cabs   127 over < if 256 swap - then ;

0 value latest
0 value latestxt

variable dp
: here      dp @ ;
: allot     dp +! ;
: align     dp @ aligned dp ! ;

: ,       here !  cell allot ;
: c,      here c!  1 allot ;
: move,   here swap dup allot cmove ;
: ",      move, align ;

include jump.fth

: >lfa     TO_NEXT + ;
: >code    TO_CODE + ;
: >body    TO_BODY + ;
: >nextxt   >lfa @ ;

include threading.fth

variable current

\ Compile the contents of a, then store x in a.
: chain, ( x a -- )   dup @ , ! ; 

: latest! ( a1 a2 -- ) to latest to latestxt ;
: link,   dup latest!  current @ >body @ , ;
: reveal   latest ?dup if current @ >body ! then ;

: cells   [ cell 1 > ] [if] dup + [then]
   [ cell 2 > ] [if] dup + [then]
   [ cell 4 > ] [if] dup + [then] ;

: code,   code! cell allot ;
: (does>)   r> does! ;

0 value stdin

include target.fth

: lowercase? ( c -- flag )   dup [char] a < if drop 0 exit then [ char z 1+ ] literal < ;
: upcase ( c1 -- c2 )   dup lowercase? if [ char A char a - ] literal + then ;
: c<> ( c1 c2 -- flag )   upcase swap upcase <> ;

: name= ( ca1 u1 ca2 u2 -- flag )
   2>r r@ <> 2r> rot if 3drop 0 exit then
   bounds do
      dup c@ i c@ c<> if drop unloop 0 exit then
      1+
  loop drop -1 ;
: nt= ( ca u nt -- flag )   >name name= ;

: immediate?   >nfa c@ 127 swap < if 1 else -1 then ;

\ TODO: nt>string nt>interpret nt>compile
\ Forth83: >name >link body> name> link> n>link l>name

: traverse-wordlist ( wid xt -- ) ( xt: nt -- continue? )
   >r >body @ begin dup while
      r@ over >r execute r> swap
      while >nextxt
   repeat then r> 2drop ;

: ?nt>xt ( -1 ca u nt -- 0 xt i? 0 | -1 ca u -1 )
   3dup nt= if >r 3drop 0 r> dup immediate? 0
   else drop -1 then ;
: (find) ( ca u wl -- ca u 0 | xt 1 | xt -1 )
   2>r -1 swap 2r> ['] ?nt>xt traverse-wordlist rot if 0 then ;

defer abort
defer (abort")
: undef ( a u -- )   ." Undefined: " type cr abort ;
: ?undef ( a u x -- a u )   if undef then ;

: literal   compile (literal) , ; immediate
: ?literal ( x -- )   state @ if [compile] literal then ;

defer number

\ Sorry about the long definition, but I didn't want to leave many
\ useless factors lying around.
: (number) ( a u -- )
   over c@ [char] - = dup >r if swap 1+ swap 1 - then
   0 rot rot
   begin dup while
      over c@ [char] 0 - -1 over < while dup 10 < while
      2>r 1+ swap dup dup + dup + + dup +  r> + swap r> 1 -
   repeat then drop then
   ?dup ?undef drop r> if negate then  ?literal ;

variable >in
variable input
: input@ ( u -- a )   cells input @ + ;
: 'source   0 input@ ;
: #source   1 input@ ;
: source#   2 input@ ;
: 'refill   3 input@ ;
: 'prompt   4 input@ ;
: source>   5 input@ ;
6 cells constant /input-source

create forth  2 cells allot
create compiler-words  2 cells allot
create search-paths 2 cells allot
create included-files  2 cells allot
create context  9 cells allot


: r@+   r> r> dup cell+ >r @ swap >r ;
: search-context ( a u context -- a 0 | xt ? )   >r begin r@+ ?dup while
   (find) ?dup until else drop 0 then r> drop ;
: find-name ( a u -- a u 0 | xt ? )   swap over #name min context
   search-context ?dup if rot drop else swap 0 then ;

: source   'source @  #source @ ;
: source? ( -- flag )   >in @ source nip < ;
: <source ( -- char|-1 )   source >in @ dup rot = if
   2drop -1 else + c@  1 >in +! then ;

: blank?   33 < ;
: skip ( "<blanks>" -- )   begin source? while
   <source blank? 0= until -1 >in +! then ;
: parse-name ( "<blanks>name<blank>" -- a u )   skip  source drop >in @ +
   0 begin source? while 1+ <source blank? until 1 - then ;

: (previous)   ['] forth context ! ;

defer also
defer previous
defer catch

create interpreters  ' execute , ' number , ' execute ,
: ?exception   if cr ." Exception!" cr then ;
: interpret-xt   1+ cells  interpreters + @ catch ?exception ;

: [   0 state !  ['] execute interpreters !  previous ; immediate
: ]   1 state !  ['] compile, interpreters !
   also ['] compiler-words context ! ;

variable csp

: .latest   latestxt >name type ;
: ?bad   rot if type ."  definition: " .latest cr abort then 2drop ;
: !csp   csp @ s" Nested" ?bad  sp@ csp ! ;
: ?csp   sp@ csp @ <> s" Unbalanced" ?bad  0 csp ! ;

: ;   reveal compile exit [compile] [ ?csp ; immediate

\ ----------------------------------------------------------------------

( Core extension words. )

: refill   0 >in !  0 #source !  'refill perform ;
: ?prompt    'prompt perform ;
: source-id   source# @ ;

256 constant /file

: file-refill   'source @ /file bounds do
      i 1 source-id read-file if 0 unloop exit then
      0= if source nip unloop exit then
      i c@ 10 = if leave then
      1 #source +!
   loop -1 ;

0 value file-source

: save-input   >in @ input @ 2 ;
: restore-input   drop input ! >in ! 0 ;

defer backtrace

: sigint   cr backtrace abort ;

\ These will be set in COLD, or by the metacompiler.
0 constant sp0
0 constant rp0
0 constant dp0
variable limit
0 constant image0
0 constant latest0

defer parsed
: (parsed) ( a u -- )   find-name interpret-xt ;
: ?stack   sp0 sp@ cell+ < abort" Stack underflow" ;
: interpret   begin parse-name dup while parsed ?stack repeat 2drop ;
: interpreting   begin refill while interpret ?prompt repeat ;

: 0source   'prompt !  'refill !  source# !  'source !  0 source> ! ;
: source, ( 'source sourceid refill prompt -- )
   input @ >r  here input !  /input-source allot  0source  r> input ! ;
: file,   0 0 ['] file-refill ['] noop source,  /file allot ;
: +file   here source> !  file, ;
: file>   source> @  ?dup if input ! else +file then ;
: alloc-file   file-source input ! begin 'source @ while file> repeat ;
: file-input ( fileid -- )   alloc-file  source# !  6 input@ 'source ! ;

: include-file ( fileid -- )   save-input drop 2>r
   file-input interpreting  source-id close-file drop  0 'source !
   2r> 2 restore-input abort" Bad restore-input" ;

: +string   2dup 2>r + over >r swap cmove r> 2r> rot + ;
: pathname   >r 2dup r> >name here 0 +string +string ;
: ?include   if drop 1 else >r 2drop r> include-file 0 0 then ;
: ?open ( a u nt -- a u 1 | 0 0 ) pathname r/o open-file ?include ;
: ?error   abort" File not found" ;
: search-file   ['] search-paths ['] ?open traverse-wordlist ?error ;
: >current ( wl1 -- ) ( R: -- wl2 ) current @ r> 2>r  current ! ;
: current>   r> r> current ! >r ;
: +name ( a u wl -- ) >current header, 0 , reveal current> ;
: remember-file   ['] included-files +name ;
: included   2dup remember-file search-file ;
: searched ( a u -- ) ['] search-paths +name ;

: dummy-catch   execute 0 ;

defer quit

: warm
   io-init

   ." lbForth" cr

   dp0 dp !

   ['] noop dup is backtrace is also
   ['] dummy-catch is catch
   ['] (number) is number
   ['] (parsed) is parsed
   ['] (previous) is previous
   latest0 dup to latestxt forth !
   ['] forth current !
   here to file-source  file,

   0 forth cell+ !
   0 compiler-words !  ['] forth compiler-words cell+ !
   0 search-paths !    ['] compiler-words included-files cell+ !
   0 included-files !  ['] search-paths included-files cell+ !
   ['] forth dup context ! context cell+ ! 0 context 2 cells + !

   s" src/" searched \ UGLY HACK
   sysdir searched
   s" " searched

   [compile] [
   s" load.fth" included
   ." ok" cr
   quit ;

: 2constant   create , ,  does> 2@ ;
: 2literal    swap postpone literal postpone literal ; compile-only
: 2variable   create 2 cells allot ;

\ d.
\ d.r

: d0<   nip 0< ;
: d2*   swap s>d negate swap 2* rot 2* rot + ;
: d2/   dup 1 and if [ 0 invert 1 rshift invert ] literal else 0 then
   swap 2/ rot 1 rshift rot + swap ;

: d<    rot > if 2drop -1 else u< then ;
: du<   rot u> if 2drop -1 else u< then ;

: d0=   or 0= ;
: d=    rot = -rot = and ;
: d>s   drop ;

: 4dup   2>r 2>r 2r@ 2r> 2r@ 2swap 2r> ;
: 2nip   2>r 2drop 2r> ;
: dmax   4dup d< if 2drop else 2nip then ;
: dmin   4dup d< if 2nip else 2drop then ;

: d-        dnegate d+ ;
: m+        s>d d+ ;

\ m*/ ( d n1 n2 -- d*n1/n2 )

( Double-Number extension words. )

: 2rot   2>r 2swap 2r> 2swap ;

( Forth12 Double-Number extension words. )

: value    create ['] ! , ,  does> cell+ @ ;
: 2value   create ['] 2! , , ,  does> cell+ 2@ ;

: to   ' >body dup ['] 2value < if ! else dup cell+ swap @ execute then ;
: to   ' >body dup ['] 2value < if postpone literal postpone !
   else dup cell+ postpone literal @ compile, then ; compile-only

( ... )

: 2tuck   over >r rot >r rot over r> r> rot ;