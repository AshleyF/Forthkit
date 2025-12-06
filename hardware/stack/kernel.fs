require assembler.fs

( --- primitive control-flow ------------------------------------------------- )

: 0branch, ( -- dest ) 0 0jump, here 2 - ; \ dummy relative jump if 0 to address, push pointer to patch
: branch, ( -- dest ) 0, 0branch, ; \ dummy unconditional relative jump, push pointer to patch
: patch, ( orig -- ) dup here - swap s! ; \ patch relative branch to here

\ ... if ... then | ... if ... else ... then
: if, ( C: -- orig ) 0branch, ; \ dummy branch on 0, push pointer to address
: else, ( C: orig1 -- orig2 ) branch, swap patch, ; \ patch previous branch to here, dummy unconditionally branch over false block
: then, ( orig -- ) patch, ; \ patch if/else to continue here

\ begin ... again | begin ... until | begin ... while ... repeat  (note: not begin ... while ... again!)
: begin, ( C: -- dest ) here ; \ begin loop
: again, ( C: dest -- ) jump, ; \ jump back to beginning
: while, ( C: dest -- orig dest ) 0branch, swap ; \ continue while condition met (0= if), 
: repeat, ( C: orig dest -- ) again, here swap s! ; \ jump back to beginning, patch while to here

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
    rot over = if 3 min rot over str= nip else 2drop 2drop 0 then ( find link name eq? -- )
    if 3 + memory - -rot 2drop exit then \ return xt if names match
    drop s@ dup 0= if ." Word not found: " drop count type cr exit then \ follow link, until end
  again ;

: ' bl word find-word ;

: ['] ' postpone literal ; immediate

true warnings ! \ intentionally redefining (latest header, ' ['])

( --- primitives ------------------------------------------------------------- )

skip, \ skip dictionary

\ @ ( addr -- ) fetch 16-bit value
0 header, @
  ld16+, nip, ret,

\ ! ( val addr -- ) store 16-bit value
0 header, !
  swap, st16+, drop, ret,

\ c! ( val addr -- ) store 8-bit value
0 header, c!
  swap, st8+, drop, ret,

\ 0= ( y x -- b ) true if equal to zero
0 header, 0=
  if, false literal, else, true literal, then, ret, \ TODO: seems "brute force"

\ <> ( y x -- b ) true if not equal
0 header, <>
  sub, ( zero if equal ) if, true literal, else, false literal, then, ret, \ TODO: seems "brute force"

\ < ( y x -- b ) true if y less than x (- 0<) TODO: handle overflow (see bootstrap)!
0 header, <
  sub, ( negative if y < x ) 15 literal, shr, ( sign bit to 1s place ) 1 literal, and, not, 1 literal, add, ret,

\ 1+ ( x -- inc ) increment (1 +)
0 header, 1+
  1 literal, add, ret,

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

: ?do, ( limit start -- ) ( C: -- false addr true addr )
         2dup,
  ['] <> call,
         false \ terminator for patching
         0branch,
         true  \ patch branch to loop
         2>r,
         begin, ;

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

( --- assembler -------------------------------------------------------------- )

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

\ false ( -- false ) return false flag (0 constant false)
0 header, false
  0 literal,
    ret,

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

0 header, (evaluate) \ internal (non-standard)
               begin,
\ ' parse-name call, \ addr len
               dup,
\         ' 0> call,
               while,
\  ' interpret call,
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

( --- end of dictionary ------------------------------------------------------ )

start,

' decimal call, \ default base
   ' quit call,
          halt,

\ here     ' dp     16 + s! \ update dictionary pointer to compile-time position
\ latest @ ' latest 16 + s! \ update latest to compile-time

." Kernel size: " here . ." bytes" cr