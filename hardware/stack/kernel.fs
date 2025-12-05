require assembler.fs

( --- primitive control-flow ------------------------------------------------- )

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
  latest @ \ ( find link -- )
  begin
    2dup memory + 3 + dup 1- c@ $7f and \ get name/len ( find link find name len -- )
    rot count 2over ( find link name len find len name len -- )
    rot over = if 3 min rot over str= else 2drop 2drop 0 then ( find link name eq? -- )
    if drop 3 + memory - -rot 2drop exit then \ return xt if names match
    drop s@ dup 0= if ." Word not found: " drop count type cr exit then \ follow link, until end
  again ;

: ' bl word find-word ;

\ TODO: commenting out until used : ['] ' postpone literal ; immediate

true warnings ! \ intentionally redefining (latest header, ' ['])

( --- primitives ------------------------------------------------------------- )

branch, \ skip dictionary

\ ! ( val addr -- ) store 16-bit value
0 header, !
  swap, st16+, drop, ret,

( --- memory ----------------------------------------------------------------- )

\ create variable ([header] [lit16 ret .] <ADDR> <VALUE>)
: var, 0 header, here 4 + lit16, ret, 0 , ;

( --- secondary control-flow ------------------------------------------------- )

( --- assembler -------------------------------------------------------------- )

( --- interpreter ------------------------------------------------------------ )

\ base ( -- base ) address of current number-conversion radix (2..36, initially 10)
var, base

\ decimal ( -- ) set number-conversion radix to 10
0 header, decimal
    10 lit8,
' base call,
' !    call,
       ret,

( --- end of dictionary ------------------------------------------------------ )

patch,
' decimal call, \ default base
\   ' quit jump,

\ here     ' dp     16 + s! \ update dictionary pointer to compile-time position
\ latest @ ' latest 16 + s! \ update latest to compile-time

." Kernel size: " here . ." bytes" cr

halt,