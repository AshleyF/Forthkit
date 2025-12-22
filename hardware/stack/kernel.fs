require assembler.fs

( --- primitive control-flow ------------------------------------------------- )

: 0branch, ( -- dest ) here 0jump, here 1- ; \ dummy relative jump if 0 to address, push pointer to patch
: branch, ( -- dest ) zero, 0branch, ; \ dummy unconditional relative jump, push pointer to patch
: patch, ( orig -- ) initslot here over - verify-sbyte swap memory + c! ; \ patch relative branch to here \ TODO: why 2 - ?!

\ ... if ... then | ... if ... else ... then
: if, ( C: -- orig ) 0branch, ; \ dummy branch on 0, push pointer to address
: else, ( C: orig1 -- orig2 ) branch, swap patch, ; \ patch previous branch to here, dummy unconditionally branch over false block
: then, ( orig -- ) patch, ; \ patch if/else to continue here

\ begin ... again | begin ... until | begin ... while ... repeat  (note: not begin ... while ... again!)
: begin, ( C: -- dest ) initslot here ; \ begin loop
: again, ( C: dest -- ) zero, 0jump, ; \ jump back to beginning
: while, ( C: dest -- orig dest ) 0branch, swap ; \ continue while condition met (0= if), 
: repeat, ( C: orig dest -- ) again, patch, ; \ jump back to beginning, patch while to here

( --- dictionary ------------------------------------------------------------- )

false warnings ! \ intentionally redefining (latest header, ' ['])

variable latest  $ffff latest !

\ [LINK][FLAG/LEN]NAM (only first three letters--6 bytes)

: header, ( flag "<spaces>name" -- )
  align,            \ aligned header + 6 bytes, code is even-aligned
  latest @          \ link to current (soon to be previous) word
  here latest !     \ update latest to this word
  ,                 \ append link
  parse-name        \ ( flag addr len -- )
  rot over or c,    \ append flag/len ( addr len -- )
  3 min swap over   \ first 3 letters only ( len addr len -- )
  over + swap       \ ( len end start -- )
  do i c@ c, loop   \ append name
  3 swap - 0 ?do 0 c, loop ; \ pad

: find-word ( addr -- )
  \ ." FIND: " dup count type cr
  latest @ \ ( find link -- )
  begin
    2dup memory + 3 + dup 1- c@ $7f and \ get name/len ( find link find name len -- )
    rot count 2over ( find link name len find len name len -- )
    rot over = if 3 min rot over str= nip else 2drop 2drop false then ( find link name eq? -- )
    if 3 + memory - -rot 2drop exit then \ return xt if names match
    drop s@ dup 0= if ." Word not found: " drop count type cr exit then \ follow link, until end
  again ;

: ' bl word find-word ;

: ['] ' postpone literal ; immediate

true warnings ! \ intentionally redefining (latest header, ' ['])

( --- primitives ------------------------------------------------------------- )

skip, \ skip dictionary

\ poor man's . ( n -- ) print decimal number \ TODO: remove
0 header, .
    dup, 10000 literal, div, dup, 48 literal, add, emit, 10000 literal, mul, sub,
    dup, 1000  literal, div, dup, 48 literal, add, emit,  1000 literal, mul, sub,
    dup, 100   literal, div, dup, 48 literal, add, emit,   100 literal, mul, sub,
    dup, 10    literal, div, dup, 48 literal, add, emit,    10 literal, mul, sub,
                                  48 literal, add, emit, ret,

\ @ ( addr -- ) fetch 16-bit value
0 header, @
  ld16+, nip, ret,

\ c@ ( addr -- ) fetch 8-bit value
0 header, c@
  ld8+, nip, ret,

\ ! ( val addr -- ) store 16-bit value
0 header, !
  swap, st16+, drop, ret,

\ +! ( val addr -- ) store 16-bit value (dup @ rot + swap !)
0 header, +!
  swap, over, ld16+, nip, add, st16+, drop, ret,

\ c! ( val addr -- ) store 8-bit value
0 header, c!
  swap, st8+, drop, ret,

\ 0= ( y x -- b ) true if equal to zero
0 header, 0=
  if, false literal, else, true literal, then, ret, \ TODO: seems "brute force"

\ <> ( y x -- b ) true if not equal
0 header, <>
  sub, ( zero if equal ) if, true literal, else, false literal, then, ret, \ TODO: seems "brute force"

0 header, sign-bit \ sign bit of top of stack to 1s place (non-standard)
  15 literal, shr, ret,

\ < ( y x -- b ) true if y less than x (- 0<) TODO: handle overflow (see bootstrap)!
0 header, <
  sub, ( negative if y < x ) ' sign-bit call, 1 literal, and, not, 1 literal, add, ret,

\ > ( y x -- b ) true if y greater than x (- 0>) TODO: handle overflow (see bootstrap)!
0 header, >
  sub, ( negative if y < x ) 1 literal, sub, ' sign-bit call, 1 literal, and, not, 1 literal, add, not, ret,

\ 0> ( x -- b ) true if x greater than zero (1- 15 rshift 1-)
0 header, 0>
  1 literal, sub, \ negative if not greater than zero
  ' sign-bit call,
  1 literal, and, not, 1 literal, add, not, ret,

\ 0< ( x -- b ) true if x less than zero (15 rshift negate 1+)
0 header, 0<
  ' sign-bit call,
  1 literal, and, not, 1 literal, add, ret,

\ = ( y x -- b ) true if equal
0 header, =
  sub, ( zero if equal ) if, false literal, else, true literal, then, ret, \ TODO: seems "brute force"

\ 1+ ( x -- inc ) increment (1 +)
0 header, 1+
  1 literal, add, ret,

\ 1- ( x -- dec ) decrement (1 -)
0 header, 1-
  1 literal, sub, ret,

\ exit ( -- ) ( R: addr -- ) return from call
0 header, exit
  pop, drop, ret, \ discard this call

\ bl ( -- c ) space character value (32 constant bl)
0 header, bl
  32 literal, ret,

\ cr ( -- ) cause newline (10 emit)
0 header, cr
  10 literal, emit, ret,

( --- memory ----------------------------------------------------------------- )

\ create variable ([header] [lit16 ret .] <ADDR> <VALUE>)
: var, 0 header, here 4 + lit16, ret, 0 , ;

( --- secondary control-flow ------------------------------------------------- )

\ <limit> <start> do ... loop
\ <limit> <start> do ... <n> +loop
\ <limit> <start> do ... unloop exit ... loop
\ <limit> <start> do ... if ... leave then ... loop
: do, ( limit start -- ) ( C: -- false addr ) \ begin do-loop (immediate 2>r begin false)
  2>r,
  false \ no addresses to patch (initially)
  begin, ;

: ?do, ( limit start -- ) ( C: -- false addr true addr )
         2dup,
         2>r,
  ['] <> call,
         false \ terminator for patching
         0branch,
         true  \ patch branch to loop
         begin, ;

: leave, ( C: -- addr true )
  branch, -rot true -rot ; \ patch to loop (swap under if address)

: loop, ( C: addr -- )
     1 literal,
       r>,
       add,
       r@,
       over,
       >r,
 ['] < call,
       if,
       swap again,
       then,
       begin while
       patch,
       repeat
       2r>,
       2drop, ;

( --- interpreter ------------------------------------------------------------ )

\ base ( -- base ) address of current number-conversion radix (2..36, initially 10)
var, base

\ decimal ( -- ) set number-conversion radix to 10
0 header, decimal
      10 literal,
  ' base call,
     ' ! call,
         ret,

\ source-id ( -- 0 | -1 ) Identifies the input source (-1=string [evaluate], 0=input device)
var, source-id

\ true ( -- true ) return true flag (-1 constant true)
0 header, true
  -1 literal, ret,

\ false ( -- false ) return false flag (0 constant false)
0 header, false
  0 literal, ret,

\ state ( -- a-addr ) compilation-state flag (true=compiling)
var, state

\ source-addr ( -- addr ) source buffer address (initialized below)
var, source-addr

\ source-len ( -- len ) source buffer length (initialized below)
var, source-len

\ source ( -- c-addr u )
0 header, source
  ' source-addr call,
            ' @ call,
   ' source-len call,
            ' @ jump,

\ rot ( z y x -- y x z ) rotate top three stack values
0 header, rot
  push, swap, pop, swap, ret,

\ -rot ( z y x -- x z y ) reverse rotate top three stack values (non-standard - rot rot)
0 header, -rot
  swap, push, swap, push, pop, pop, ret,

\ fill ( c-addr u char -- ) if u is greater than zero, store char in each of u consecutive characters of memory beginning at c-addr.
0 header, fill
  ' -rot call,
       0 literal,
         ?do,
         2dup,
    ' c! call,
    ' 1+ call,
         loop,
         2drop,
         ret,

\ erase ( addr u -- ) if u is greater than zero, clear all bits in each of u consecutive address units of memory beginning at addr.
0 header, erase
       0 literal,
  ' fill jump,

\ accept ( c-addr +n1 -- +n2 ) receive string of, at most, n1 chars into c-addr, returning number of chars (n2)
0 header, accept
       0 literal, \ c-addr n1 0
         begin,   \ c-addr n1 n2
    ' 1+ call,    \ c-addr n1 n2+                  increment n2
         2dup,    \ c-addr n1 n2+ n1 n2+
     ' < call,    \ c-addr n1 n2+ <
         if,      \ c-addr n1 n2+                  bounds check
         drop,    \ c-addr n1
         nip,     \ n1
  ' exit call,    \ n1
         then,    \ c-addr n1 n2+
   ' rot call,    \ n1 n2+ c-addr
         key,     \ n1 n2+ c-addr key
         swap,    \ n1 n2+ key c-addr
         2dup,    \ n1 n2+ key c-addr key c-addr
     ' ! call,    \ n1 n2+ key c-addr              store key
    ' 1+ call,    \ n1 n2+ key c-addr+
         swap,    \ n1 n2+ c=addr+ key
         dup,     \ n1 n2+ c-addr+ key key 
      13 literal, \ n1 n2+ c-addr+ key key 13
    ' <> call,    \ n1 n2+ c-addr+ key <>13
         swap,    \ n1 n2+ c-addr+ <>13 key
      10 literal, \ n1 n2+ c-addr+ <>13 key 10
    ' <> call,    \ n1 n2+ c-addr+ <>13 <>10
         and,     \ n1 n2+ c-addr+ <>CRLF
         while,   \ n1 n2+ c-addr+
  ' -rot call,    \ c-addr+ n1 n2+
         repeat,  \ c-addr+ n1 n2+
         drop,    \ n1 n2+
         nip,     \ n2+
         ret,

\ >in offset to parse area within input buffer
var, >in

\ refill ( -- flag ) fill input buffer from input (do nothing and return false when evaluating strings)
0 header, refill
  ' source-id call,
          ' @ call,
         ' 0= call,
              dup, \ return value
              if,
     ' source call,
              2dup, \ TODO REMOVE
      ' erase call, \ TODO REMOVE
     ' accept call,
              drop, \ TODO SET LEN
            0 literal,
        ' >in call,
          ' ! call,
              then,
              ret,

\ i ( -- x ) ( R: x -- x ) copy innermost loop index (2r@ drop)
0 header, i
                  pop,  \ return from here
                  peek, \ count
                  swap,
                  push, \ restore return address
                  ret,

\ unloop ( -- ) ( R: y x -- ) remove loop parameters (r> 2r> rot >r 2drop)
0 header, unloop
        r>,  \ this return address
        2r>, \ loop parameters
  ' rot call,
        >r,  \ replace this return address
        2drop,
        ret,

\ (skip) ( char "<chars>..." -- "..." ) skip leading delimeter chars
0 header, (skip)
          ' source call, \ char c-addr u
             ' >in call, \ char c-addr u in
               ' @ call, \ char c-addr u in
             ' rot call, \ char c-addr u in c-addr
                   add,  \ char c-addr u inaddr
                   tuck, \ char inaddr c-addr u
                   add,  \ char inaddr end
                   swap, \ char end inaddr
                   ?do,  \ char
                   dup,  \ char char
               ' i call, \ char char addr
              ' c@ call, \ char char c
                   over, \ char char c char
              ' bl call, \ char char c char $20
               ' = call, \ char char c sp?
                   if,   \ char char c
                   swap, \ char c char
               ' > call, \ char c>$20 (not delimiter)
                   else, \ char char c
              ' <> call, \ char <>? (not delimiter)
                   then,
                   if,   \ char
                   leave,
                   then,
                 1 literal, \ char 1
             ' >in call,    \ char 1 in
              ' +! call,    \ char 
                   loop,
                   drop,
                   ret,

\ parse ( char "ccc<char>" -- c-addr u ) parse ccc delimited by char
0 header, parse
          ' source call, \ char c-addr u
                   over, \ char c-addr u c-addr
                   add,  \ char c-addr end
              ' 1+ call, \ char c-addr end+
                   swap, \ char end c-addr
             ' >in call, \ char end c-addr in-addr
               ' @ call, \ char end c-addr in
                   add,  \ char end start
                   tuck, \ char start end start
                   ?do,  \ char start
                   over, \ char start char
               ' i call, \ char start char i
              ' c@ call, \ char start char c
                   over, \ char start char c char
              ' bl call,
               ' = call, \ char start char c sp?
                   if,   \ char start char c
              ' 1- call,
               ' > call, \ char start c<=$20
                   else,
               ' = call, \ char start =?
                   then,
                   if,
                   nip,  \ start
               ' i call, \ start i
                   over, \ start i start
                   sub,  \ start len
                   dup,  \ start len len
              ' 1+ call, \ start len len+
             ' >in call, \ start len in
              ' +! call, \ start len
          ' unloop call,
            ' exit call,
                   then,
                   loop,
                   nip,  \ start
          ' source call, \ start c-addr u
                   add,  \ start end
                   over, \ start end start
                   sub,  \ start len
                   dup,  \ start len len
              ' 1+ call, \ start len len+
             ' >in call, \ start len in
              ' +! jump, \ start len

\ parse-name ( "<spaces>name<space>" -- c-addr u ) skip leading space and parse name delimited by space
0 header, parse-name
              ' bl call,
                   dup,
          ' (skip) call,
           ' parse jump,

\ h ( -- addr ) return address of dictionary pointer (variable h) (non-standard, common internal)
var, h \ initialized after dictionary (below)

\ here ( -- addr ) current dictionary pointer address (h @)
0 header, here
  ' h call, ' @ jump,

\ pad ( -- addr ) address of transient region for intermediate processing
0 header, pad
            ' here call,
              1024 literal, \ arbitrary distance away
                   add,
\        ' aligned jump,
                   ret,

\ cmove ( c-addr1 c-addr2 u -- ) copy characters (from lower to higher addresses)
0 header, cmove \ TODO: rethink, using ld8+ and st8+
                 0 literal,
                   ?do,
                   over,
              ' c@ call,
                   over,
              ' c! call,
              ' 1+ call, \ TODO: char+
                   swap,
              ' 1+ call, \ TODO: char+
                   swap,
                   loop,
                   2drop,
                   ret,

\ >counted ( c-addr u -- c-addr ) string address and length to counted string address (non-standard)
0 header, >counted
                   dup,  \ c-addr u u
             ' pad call, \ c-addr u u pad
              ' c! call, \ c-addr u -- length
             ' pad call, \ c-addr u pad
              ' 1+ call, \ c-addr u pad+
                   swap, \ c-addr pad+ u
           ' cmove call,
             ' pad jump, \ pad

var, latest \ common, but non-standard (confusing name conflict)

\ count ( c-addr1 -- c-addr2 u ) counted string to c-addr and length
0 header, count
                   dup,
              ' c@ call,
                   swap,
              ' 1+ call,
                   swap,
                   ret,

\ 2over ( w z y x -- w z y x w z ) copy second pair of stack values to top
0 header, 2over
  push, push, 2dup,
  pop, ' -rot call,
  pop, ' -rot jump,

0 header, (tolower) \ non-standard
                   dup,
         char A 1- literal,
               ' > call,
                   over,
         char Z 1+ literal,
               ' < call,
                   and,
                   if,
                32 literal,
                   add,
                   then,
                   ret,

\ (equal) ( c-addr1 u1 c-addr2 u2 -- flag ) compare strings for equality (non-standard)
0 header, (equal)
             ' rot call,
                   over,
              ' <> call, \ lengths unequal?
                   if,
                   drop,
                   2drop,
           ' false call,
            ' exit call,
                   then,
                 0 literal,
                   do,
                   2dup,
               ' i call,
                   add,
              ' c@ call,
       ' (tolower) call,
                   swap,
               ' i call,
                   add,
              ' c@ call,
       ' (tolower) call,
              ' <> call,
                   if,
                   2drop,
           ' false call,
          ' unloop call,
            ' exit call,
                   then,
                   loop,
                   2drop,
            ' true jump,

\ min ( y x -- min ) return minimum of y and x
0 header, min
  2dup, ' < call, if, drop, else, nip, then, ret,

\ find ( c-addr -- c-addr 0 | xt 1 | xt -1 ) find named definition (0=not found, 1=immediate, -1=non-immediate)
0 header, find
          ' latest call,
               ' @ call,    \ find link
  begin,
                   2dup,    \ find link find link
                 3 literal,
                   add,     \ find link find link+3
                   dup,
              ' 1- call,    \ find link find link+3 link+2
              ' c@ call,    \ find link find link+3 len/flag
               $7f literal,
                   and,     \ find link find link+3 len
             ' rot call,    \ find link link+3 len find
           ' count call,    \ find link link+3 len find len
           ' 2over call,    \ find link link+3 len find len link+3 len
             ' rot call,    \ find link link+3 len find link+3 len len
                   over,    \ find link link+3 len find link+3 len len len
               ' = call,    \ find link link+3 len find link+3 len =
    if,                     \ find link link+3 len find link+3 len
                 3 literal, \ find link link+3 len find link+3 len 3
             ' min call,    \ find link link+3 len find link+3 minlen
             ' rot call,    \ find link link+3 len link+3 minlen find
                   over,    \ find link link+3 len link+3 minlen find minlen
         ' (equal) call,    \ find link link+3 len =?
                   nip,     \ find link link+3 =?
    else,                   \ find link link+3 len find link+3 len
                   2drop,   \ find link link+3 len find
                   2drop,   \ find link link+3
           ' false call,    \ find link link+3 !=
    then,                   \ find link link+3 =?

    if,                     \ find link link+3
                 3 literal, \ find link link+3 3
                   add,     \ find link xt
            ' -rot call,    \ xt find link
                   nip,     \ xt link
                 2 literal, \ xt link 2
                   add,     \ xt link+2
               ' @ call,    \ xt len/flag
               $80 literal, \ xt len/flag $80
                   and,     \ xt flag
              ' 0= call,    \ xt flag=0?
      if,                   \ xt
                -1 literal, \ xt -1 (non-immediate)
      else,
                 1 literal, \ xt 1 (immediate)
      then,
            ' exit call,    \ xt (names match!)
    then,                   \ find link link+3
                   drop,    \ find link
               ' @ call,    \ find link@
                   dup,     \ find link@ link@
              ' 0= call,    \ find link@ 0=?
    if,                     \ find link@
                   drop,    \ find
                 0 literal, \ find 0  (not found)
            ' exit call,
    then,
  again,
                   ret,

\ type ( addr len -- ) display the character string (0 max 0 ?do dup c@ emit char+ loop drop)
0 header, type
                   dup, \ essentially 0 max, but we don't have max yet
              ' 0< call,
                   if,
                   drop,
                 0 literal,
                   then,
                 0 literal,
                   ?do,
                   dup,
              ' c@ call,
                   emit,
              ' 1+ call, \ TODO: char+;
                   loop,
                   drop,
                   ret,

\ >number ( ud1 c-addr1 u1 -- ud2 c-addr2 u2 ) ud2 is the unsigned result of converting the characters
\         within the string specified by c-addr1 u1 into digits, using the number in BASE, and adding
\         each into ud1 after multiplying ud1 by the number in BASE. Conversion continues left-to-right
\         until a character that is not convertible, including any "+" or "-", is encountered or the string
\         is entirely converted. c-addr2 is the location of the first unconverted character or the first
\         character past the end of the string if the string was entirely converted. u2 is the number of
\         unconverted characters in the string. An ambiguous condition exists if ud2 overflows.
0 header, >number \ TODO: support double ud1/ud2?
                   begin,  \ n a c
                   dup,    \ n a c c
              ' 0> call,   \ n a c c0>
                   while,  \ n a c
            ' -rot call,   \ c n a
                   dup,    \ c n a a
              ' c@ call,   \ c n a d
               '0' literal,
                   sub,    \ c n a d
                   dup,   \ c n a d d
                10 literal, \ cn a d d 10
               ' < call,   \ c n a d <
                   if,     \ c n a d  valid?
             ' rot call,   \ c a d n
            ' base call,   \ c a d n b
               ' @ call,   \ c a d n b
                   mul,    \ c a d n
                   add,    \ c a n
                   swap,   \ c n a
              ' 1+ call,   \ c n a
             ' rot call,   \ n a c
              ' 1- call,   \ n a c
                   else,   \ c n a d  invalid
                   drop,   \ c n a
             ' rot call,   \ n a c
            ' exit call,
                   then,   \ n a c
                   repeat,
                   ret,

\ execute ( i * x xt -- j * x ) perform the semantics identified by xt
0 header, execute
  pop, drop, \ discard return address from this call
  push, ret, \ instead, "return" to xt

( --- partial assembler ------------------------------------------------------ )

var, shiftbits
11 ' shiftbits 4 + s! \ initialize shiftbits
0 header, initslot \ word to initialize shiftbits
  11 literal, ' shiftbits call, ' ! jump,

0 header, c, \ ( c -- ) append byte to instruction slot
  ' h call, ' @ call, ' c! call, 1 literal, ' h call, ' +! jump,

0 header, , \ ( cc -- ) append 16-bit value to instruction slot
  dup, ' c, call, 8 literal, shr, ' c, jump,

0 header, call, \ ( addr -- ) call address (even-aligned)
  ' initslot call, ' , jump, \ TODO: verify even-aligned

var, h'
0 header, slot, ( i -- )
  ' shiftbits call,
          ' @ call,
           11 literal,
          ' = call, \ first slot?
              if,
        $ffff literal,
          ' h call,
          ' @ call,
              dup,
         ' h' call,
          ' ! call,
          ' ! call,
            2 literal,
          ' h call,
         ' +! call, \ h'=h, initialize no-ops, h+=2
              then,
  ' shiftbits call,
          ' @ call,
              shl, \ shift instruction to slot
         0x1f literal,
  ' shiftbits call,
          ' @ call,
              shl,
              not,
         ' h' call,
          ' @ call,
          ' @ call,
              and, \ fetch and mask off slot
              or,
         ' h' call,
          ' @ call,
          ' ! call, \ place instruction
           -5 literal,
  ' shiftbits call,
         ' +! call,
  ' shiftbits call,
          ' @ call,
         ' 0> call,
              not, \ TODO: really 0<= but not defined
              if,
           11 literal,
  ' shiftbits call,
          ' ! call,
              then,
              ret,

$80 header, lit16, \ ( n -- ) fetch literal next cell (used by interpreter)
           19 literal,
      ' slot, call,
          ' , jump,

$80 header, lit8, \ ( n -- ) fetch literal next byte (used by interpreter)
           20 literal,
      ' slot, call,
         ' c, jump, \ TODO: verify-sbyte

$80 header, ret, \ (used by interpreter)
           30 literal,
      ' slot, jump,

$80 header, literal, \ (used by interpreter)
              dup,
         -129 literal,
          ' > call,
              over,
          128 literal,
          ' < call,
              and,
              if,
      ' lit8, call,
              else,
     ' lit16, call,
              then,
              ret,

\ interpret ( c-addr u -- ) (implementation defined)
0 header, interpret
        ' >counted call,
            ' find call, \ c-addr 0 | xt 1 | xt -1
                   dup,
              ' 0= call,
  if, \ not found?
                   drop,
           ' count call,
                   over, \ determine whether prefixed with -
              ' c@ call,
            char - literal,
               ' = call,
    if,
              ' 1- call, \ decrement length
                   swap,
              ' 1+ call, \ increment address
                   swap,
                -1 literal, \ multiplier
    else,
                 1 literal, \ multiplier
    then,
            ' -rot call,
                   2dup,
                 0 literal,
            ' -rot call,
         ' >number call,
                   dup,
              ' 0= call,
    if, \ proper number
                   2drop,
            ' -rot call,
                   2drop,
                   mul,
           ' state call,
               ' @ call,
      if,   \ compile?
        ' literal, call,
      then,
    else, \ error
                   2drop,
                   drop,
              ' cr call,
            ' type call,
            char ? literal,
                   emit,
\    ' (clear-data) call, \ TODO: no such thing
            ' exit call,
    then,
  else, \ found
                 1 literal,
               ' = call,
    if,   \ immediate?
         ' execute call,
    else, \ non-immediate
           ' state call,
               ' @ call,
      if,   \ compile?
           ' call, call,
      else, \ interactive
         ' execute call,
      then,
    then,
  then,
                   ret,

0 header, (evaluate) \ internal (non-standard)
               begin,
  ' parse-name call, \ addr len
               dup,
          ' 0> call,
               while,
   ' interpret call,
               repeat,
               ret,

\ quit ( -- ) ( R: i * x -- ) Empty the return stack, store zero in SOURCE-ID if it is present, make
\      the user input device the input source, and enter interpretation state. Do not display a message.
\      Repeat the following: Accept a line from the input source into the input buffer, set >IN to zero,
\      and interpret. Display the system prompt if in interpretation state, all processing has been
\      completed, and no ambiguous condition exists.
0 header, quit
\  ' (clear-return) call, TODO: no such thing
                 0 literal,
       ' source-id call,
               ' ! call,
           ' false call,
           ' state call,
               ' ! call,
                   begin,
          ' refill call,
                   while,
      ' (evaluate) call,
                   2drop,
              ' bl call,
                   emit,
            char o literal,
                   emit,
            char k literal,
                   emit,
              ' cr call,
                   repeat,
                   \ TODO: failed to refill
                   ret,

0 header, align, \ ( -- ) align here on even address boundary
  ' initslot call,
      ' here call,
           1 literal,
             and,
        ' 0= call,
             not, \ really 0<> but not defined
             if,
           1 literal,
         ' h call,
        ' +! call,
             then,
             ret,

\ header, ( "<spaces>name -- ) append header to dictionary (non-standard, note: no flag)
0 header, header,
          ' align, call, \ aligned header + 6 bytes, code is even-aligned
          ' latest call,
               ' @ call, \ link to current (soon to be previous) word
            ' here call,
          ' latest call,
               ' ! call, \ update latest to this word
               ' , call, \ append link
      ' parse-name call, ( addr len -- )
             ' rot call,
                   over,
                   or,
              ' c, call, \ append flag/len ( addr len -- )
                 3 literal,
             ' min call,
                   swap,
                   over, \ first 3 letters only ( len addr len -- )
                   over,
                   add,
                   swap, ( end start -- )
                   do,   \ append name
               ' i call,
              ' c@ call,
              ' c, call,
                   loop,
                 3 literal,
                   swap,
                   sub,
                 0 literal,
                   ?do, \ pad
                 0 literal,
              ' c, call,
                   loop,
                   ret,  ( addr len )

\ [ ( -- ) enter interpretation state (immediate word)
$80 header, [
           ' false call,
           ' state call,
               ' ! jump,

\ ] ( -- ) enter compilation state
0 header, ]
            ' true call,
           ' state call,
               ' ! jump,

\ ; ( -- ) end current definition, make visible in dictionary and enter interpretation state
$80 header, ;
               ' [ call,
            ' ret, call,
                   ret,

0 header, swap \ (used by bootstrap)
  swap, ret,

0 header, - \ (used by bootstrap)
  sub, ret,

\ \ bye ( -- ) halt machine TODO: not needed once bootstrapped
\ 0 header, bye
\   zero, halt,

( --- end of dictionary ------------------------------------------------------ )

start,

\    ' (clear-data) call, \ TODO: no such thing
         ' decimal call, \ default base
memory-size $101 - literal, \ $101 bytes from end of memory (extra byte so that end+1 doesn't overflow)
     ' source-addr call,
               ' ! call,
               $ff literal, \ size
      ' source-len call,
               ' ! call,
            ' quit call,
                   zero,
                   halt,

here     ' h      4 + s! \ update dictionary pointer to compile-time position
latest @ ' latest 4 + s! \ update latest to compile-time

." Kernel size: " here . ." bytes" cr