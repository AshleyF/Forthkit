require assembler.fs

    2 constant one       1     one lit8, \ literal 1
    3 constant two       2     two lit8, \ literal 2
    4 constant four      4    four lit8, \ literal 4
    5 constant eight     8   eight lit8, \ literal 8
    6 constant twelve   12  twelve lit8, \ literal 12
    7 constant fifteen  15 fifteen lit8, \ literal 15
    
    8 constant #t       -1     #t lit8, \ literal true (-1)
 zero constant #f                      \ literal false (0)
 
    9 constant x
   10 constant y
   11 constant z
   12 constant w

: lit16, ( val reg -- ) pc two ld16+, , ;

( --- stacks ----------------------------------------------------------------- )

: push, ( reg ptr -- ) dup dup four sub,  st16, ;
: pop,  ( reg ptr -- ) four ld16+, ;

13 constant d \ data stack pointer (initialized after dictionary below)

: pushd, ( reg -- ) d push, ;
: popd,  ( reg -- ) d pop, ;

: literal, x lit16,  x pushd, ; \ 8 bytes

14 constant r  memory-size r lit16, \ return stack pointer

: pushr, ( reg -- ) r push, ;
: popr,  ( reg -- ) r pop, ;

: call, ( addr -- ) pc pushr,  jump, ;    \ 6 bytes
: ret, x popr,  x x four add,  pc x cp, ; \ 6 bytes (pc popr, would complicate calls)

( --- primitive control-flow ------------------------------------------------- )

: 0branch, ( -- dest ) x popd,  0 y lit16,  here 2 -  pc y x cp?, ; \ dummy jump if 0 to address, push pointer to patch
: branch, skip, ;
: patch, start, ;

\ ... if ... then | ... if ... else ... then
: if, ( C: -- orig ) 0branch, ; \ dummy branch on 0, push pointer to address
: else, ( C: orig1 -- orig2 ) branch, swap patch, ; \ patch previous branch to here, dummy unconditionally branch over false block
: then, ( orig -- ) patch, ; \ patch if/else to continue here

\ begin ... again | begin ... until | begin ... while ... repeat  (note: not begin ... while ... again!)
: begin, ( C: -- dest ) here ; \ begin loop
: again, ( C: dest -- ) jump, ; \ jump back to beginning
\ UNUSED : until, ( C: dest -- ) 0branch, s! ; \ branch on 0 to address
: while, ( C: dest -- orig dest ) 0branch, swap ; \ continue while condition met (0= if), 
: repeat, ( C: orig dest -- ) again, patch, ; \ jump back to beginning, patch while to here

( --- dictionary ------------------------------------------------------------- )

false warnings ! \ intentionally redefining (latest header, ' ['])

variable latest  $ffff latest !

\ [LINK][FLAG/LEN]NAME

: header, ( flag "<spaces>name" -- )
  latest @          \ link to current (soon to be previous) word
  here latest !     \ update latest to this word
  ,                 \ append link
  parse-name        \ ( flag addr len -- )
  rot over or c,    \ append flag/len
  over + swap       \ ( end start -- )
  do i c@ c, loop ; \ append name

: find-word ( addr -- )
  latest @ \ ( find link -- )
  begin
    2dup memory + 3 + dup 1- c@ $7f and \ get name/len
    rot count 2over str= ( compare 0= ) if + memory - -rot 2drop exit then \ return xt if names match
    2drop s@ dup 0= if ." Word not found: " drop count type cr exit then \ follow link, until end
  again ;

: ' bl word find-word ;

: ['] ' postpone literal ; immediate

true warnings ! \ intentionally redefining (latest header, ' ['])

( --- primitives ------------------------------------------------------------- )

               skip, \ skip dictionary

\ (clear-data) empty return stack (non-standard)
0 header, (clear-data)
 memory-size 2 + d lit16,
                   ret,

\ (clear-return) empty return stack (non-standard)
0 header, (clear-return)
                 x popr,
     memory-size r lit16,
          x x four add, \ ret,
              pc x cp,

\ execute ( i * x xt -- j * x ) perform the semantics identified by xt
0 header, execute
                 x popd,
              pc x cp,

\ @ ( addr -- ) fetch 16-bit value
0 header, @
                 x popd,
               x x ld16,
                 x pushd,
                   ret,

\ ! ( val addr -- ) store 16-bit value
0 header, !
                 x popd,
                 y popd,
               y x st16,
                   ret,

\ +! ( val addr -- ) store 16-bit value (dup @ rot + swap !)
0 header, +!
                 x popd,
                 y popd,
               z x ld16,
             z z y add,
               z x st16,
                   ret,

\ c@ ( addr -- ) fetch 8-bit value
0 header, c@
                 x popd,
               x x ld16,
             $ff y lit16, 
             x x y and,
                 x pushd,
                   ret,

\ c! ( val addr -- ) store 8-bit value
0 header, c!
                 x popd,
                 y popd,
             $ff z lit16, 
             y y z and,  \ mask to lower
               z z not,  \ upper mask
               w x ld16, \ existing value
             w w z and,  \ mask to upper
             y y w or,   \ combine
               y x st16,
                   ret,

\ + ( y x -- sum ) addition
0 header, +
                 x popd,
                 y popd,
             x y x add,
                 x pushd,
                   ret,

\ 1+ ( x -- inc ) increment (1 +)
0 header, 1+
                 x popd,
           x x one add,
                 x pushd,
                   ret,

\ 1- ( x -- dec ) decrement (1 -)
0 header, 1-
                 x popd,
           x x one sub,
                 x pushd,
                   ret,

\ - ( y x -- differece ) subtraction
0 header, -
                 x popd,
                 y popd,
             x y x sub,
                 x pushd,
                   ret,

\ * ( y x -- product ) subtraction
0 header, *
                 x popd,
                 y popd,
             x y x mul,
                 x pushd,
                   ret,

\ and ( y x -- result ) logical/bitwise and
0 header, and
                 x popd,
                 y popd,
             z x y and,
                 z pushd,
                   ret,

\ or ( y x -- result ) logical/bitwise or
0 header, or
                 x popd,
                 y popd,
             z x y or,
                 z pushd,
                   ret,

\ key ( -- char ) read from console
0 header, key
                 x in,
                 x pushd,
                   ret,

\ emit ( char -- ) write to console
0 header, emit
                 x popd,
                 x out,
                   ret,

\ drop ( x -- ) remove top stack value
0 header, drop
          d d four add,
                   ret,

\ 2drop ( y x -- ) remove top two stack values
0 header, 2drop
         d d eight add,
                   ret,

\ dup ( x -- x x ) duplicate top stack value
0 header, dup
               x d ld16,
                 x pushd,
                   ret,

\ 2dup ( y x -- y x y x ) duplicate top two stack values
0 header, 2dup
               x d ld16,
          y d four add,
               y y ld16,
                 y pushd,
                 x pushd,
                   ret,

\ nip ( y x -- x ) drop second stack value
0 header, nip
                 x popd,
               x d st16,
                   ret,

\ over ( y x -- y x y ) copy second stack value to top
0 header, over
          x d four add,
               x x ld16,
                 x pushd,
                   ret,

\ 2over ( w z y x -- w z y x w z ) copy second pair of stack values to top
0 header, 2over
        x d twelve add,
               x x ld16,
                 x pushd,
        x d twelve add,
               x x ld16,
                 x pushd,
                   ret,

\ swap ( y x -- x y ) swap top two stack values
0 header, swap
               x d ld16,
          z d four add,
               y z ld16,
               y d st16, \ swap in-place
               x z st16,
                   ret,

\ tuck ( y x -- x y x ) copy top stack value under second value
0 header, tuck
               x d ld16,
          z d four add,
               y z ld16,
               y d st16, \ swap in-place
               x z st16,
                 x pushd,
                   ret,

\ rot ( z y x -- y x z ) rotate top three stack values
0 header, rot
                 x popd,
                 y popd,
                 z popd,
                 y pushd,
                 x pushd,
                 z pushd,
                   ret,

\ -rot ( z y x -- x z y ) reverse rotate top three stack values (non-standard - rot rot)
0 header, -rot
                 x popd,
                 y popd,
                 z popd,
                 x pushd,
                 z pushd,
                 y pushd,
                   ret,

\ >r ( x -- ) ( R: x -- ) move x to return stack
0 header, >r
                 x popd,
               y r ld16, \ this return address
               x r st16, \ replace
          y y four add,  \ ret,
              pc y cp,

\ 2>r ( y x -- ) ( R: -- y x ) move y x pair to return stack
0 header, 2>r
                 x popd,
                 y popd,
               z r ld16, \ this return address
               y r st16, \ push y in-place
                 x pushr,
          z z four add,  \ ret,
              pc z cp,

\ r> ( -- x ) ( R: x -- ) move x from return stack
0 header, r>
                 z popr, \ this return address
                 x popr, \ top value before call
                 x pushd,
          z z four add,  \ ret,
              pc z cp,

\ 2r> ( -- y x ) ( R: y x -- ) move x from return stack
0 header, 2r>
                 z popr, \ this return address
                 x popr, \ top value before call
                 y popr, \ second value before call
                 y pushd,
                 x pushd,
          z z four add,  \ ret,
              pc z cp,

\ r@ ( -- x ) ( R: x -- x ) copy x from return stack
0 header, r@
                 z popr, \ this return address
               x r ld16, \ top value before call
                 x pushd,
          z z four add,  \ ret,
              pc z cp,

\ 2r@ ( -- y x ) ( R: y x -- y x ) copy y x pair from return stack
0 header, 2r@
                 z popr, \ this return address
               x r ld16, \ top value before call
          y r four add,
               y y ld16,
                 y pushd,
                 x pushd,
          z z four add,  \ ret,
              pc z cp,

\ = ( y x -- b ) true if equal
0 header, =
                 x popd,
               y d ld16,
             z y x sub, \ zero if equal
              y #f cp,
            y #t z cp?,
               y d st16,
                   ret,

\ <> ( y x -- b ) true if not equal
0 header, <>
                 x popd,
               y d ld16,
             z y x sub, \ zero if equal
              y #t cp,
            y #f z cp?,
               y d st16,
                   ret,

\ 0= ( y x -- b ) true if equal to zero
0 header, 0=
               x d ld16,
              y #f cp,
            y #t x cp?,
               y d st16,
                   ret,

\ 0< ( x -- b ) true if x less than zero (15 rshift negate 1+)
0 header, 0<
               x d ld16,
       x x fifteen shr, \ sign bit to 1s place
           x x one and,
               x x not, \ negate
           x x one add,
               x d st16,
                   ret,

\ < ( y x -- b ) true if y less than x (- 0<) TODO: handle overflow (see bootstrap)!
0 header, <
                 x popd,
               y d ld16,
             z y x sub, \ negative if y less than x
       z z fifteen shr, \ sign bit to 1s place
           z z one and,
               z z not, \ negate
           z z one add,
               z d st16,
                   ret,

\ 0> ( x -- b ) true if x greater than zero (1- 15 rshift 1-)
0 header, 0>
               x d ld16,
           x x one sub, \ negative if not greater than zero
       x x fifteen shr, \ sign bit to 1s place
           x x one and,
               x x not, \ negate
           x x one add,
               x x not, \ invert
               x d st16,
                   ret,

\ > ( y x -- b ) true if y greater than x (- 0>) TODO: handle overflow (see bootstrap)!
0 header, >
                 x popd,
               y d ld16,
             x y x sub, \ negative if y less than x
           x x one sub, \ negative if y is equal to x
       x x fifteen shr, \ sign bit to 1s place
           x x one and,
               x x not, \ negate
           x x one add,
               x x not, \ invert
               x d st16,
                   ret,

\ exit ( -- ) ( R: addr -- ) return from call
0 header, exit
                 x popr, \ discard this call
                   ret,

( --- memory ----------------------------------------------------------------- )

: var,
                 0 header,
              x pc cp,
              14 y lit8,    \ count this and following instructions
             x x y add,     \ point just beyond this code -- data field
                 x pushd,   \ 4 bytes
                   ret,     \ 6 bytes
                   2 h +! ; \ 2 allot

\ h ( -- addr ) return address of dictionary pointer (variable h) (non-standard, common internal)
var, h \ initialized after dictionary (below)

\ here ( -- addr ) current dictionary pointer address (h @)
0 header, here
               ' h call,
               ' @ jump,

\ allot ( n -- ) advance dictionary pointer (h +!)
0 header, allot
               ' h call,
              ' +! jump,

\ , ( x -- ) append x in newly reserved cell (here ! 1 cells allot)
0 header, ,
            ' here call,
               ' ! call,
                 2 literal, \ TODO: 1 cells
           ' allot jump,

\ c, ( x -- ) append x chars in newly reserved space (here c! 1 chars allot)
0 header, c,
            ' here call,
              ' c! call,
                 1 literal,
\          ' chars call, \\ TODO: use chars;
           ' allot jump,

( --- secondary control-flow ------------------------------------------------- )

\ <limit> <start> do ... loop
\ <limit> <start> do ... <n> +loop
\ <limit> <start> do ... unloop exit ... loop
\ <limit> <start> do ... if ... leave then ... loop
: do, ( limit start -- ) ( C: -- false addr ) \ begin do-loop (immediate 2>r begin false)
           ['] 2>r call,
                   false \ no addresses to patch (initially)
                   begin, ;

: ?do, ( limit start -- ) ( C: -- false addr true addr )
          ['] 2dup call,
           ['] 2>r call,
            ['] <> call,
                   false \ terminator for patching
                   0branch,
                   true  \ patch branch to loop
                   begin, ;

: leave, ( C: -- addr true )
                   branch,
                   -rot true -rot ; \ patch to loop (swap under if address)

: loop, ( C: addr -- )
                 1 literal,
            ['] r> call,
             ['] + call,
            ['] r@ call,
          ['] over call,
            ['] >r call,
             ['] < call,
                   if,
                   swap again,
                   then,
                   begin while
                   patch,
                   repeat
           ['] 2r> call,
         ['] 2drop call, ;

\ i ( -- x ) ( R: x -- x ) copy innermost loop index (2r@ drop)
0 header, i
             ' 2r@ call, \ including return from here
            ' drop jump,

\ unloop ( -- ) ( R: y x -- ) remove loop parameters (r> 2r> rot >r 2drop)
0 header, unloop
              ' r> call, \ this return address
             ' 2r> call, \ loop parameters
             ' rot call,
              ' >r call, \ replace this return address
           ' 2drop jump,

( --- assembler -------------------------------------------------------------- )

\ ret, ( -- )
0 header, ret,
             $E9C4 literal,
               ' , call,
             $9914 literal,
               ' , call,
             $90F1 literal,
               ' , jump,

\ literal, ( val -- )
0 header, literal,
             $09C3 literal,
               ' , call,
               ' , call, \ value
             $DD24 literal,
               ' , call,
             $D9D1 literal,
               ' , jump,

\ call, ( addr -- )
0 header, call,
             $EE24 literal,
               ' , call,
             $E0D1 literal,
               ' , call,
             $00C1 literal,
               ' , call,
               ' , jump, \ address

\ lshift ( y x -- result ) left shift
\ needed to bootstrap assembler
0 header, lshift
                 x popd,
                 y popd,
             x y x shl,
                 x pushd,
                   ret,

\ rshift ( y x -- result ) right shift
\ needed to bootstrap assembler
0 header, rshift
                 x popd,
                 y popd,
             x y x shr,
                 x pushd,
                   ret,

( --- interpreter ------------------------------------------------------------ )

\ true ( -- true ) return true flag (-1 constant true)
0 header, true
                -1 literal,
                   ret,

\ false ( -- false ) return false flag (0 constant false)
0 header, false
                 0 literal,
                   ret,

\ bl ( -- c ) space character value (32 constant bl)
0 header, bl
                32 literal,
                   ret,

\ cr ( -- ) cause newline (10 emit)
0 header, cr
                10 literal,
            ' emit jump,

\ type ( addr len -- ) display the character string (0 max 0 ?do dup c@ emit char+ loop drop)
0 header, type
             ' dup call, \ essentially 0 max, but we don't have max yet
              ' 0< call,
                   if,
            ' drop call,
                 0 literal,
                   then,
                 0 literal,
                   ?do,
             ' dup call,
              ' c@ call,
            ' emit call,
              ' 1+ call, \ TODO: char+;
                   loop,
            ' drop jump,

\ pad ( -- addr ) address of transient region for intermediate processing
0 header, pad
            ' here call,
              1024 literal, \ arbitrary distance away
               ' + jump,
\        ' aligned jump,

\ base ( -- base ) address of current number-conversion radix (2..36, initially 10)
var, base

\ decimal ( -- ) set number-conversion radix to 10
0 header, decimal
                10 literal,
            ' base call,
               ' ! jump,

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

\ fill ( c-addr u char -- ) if u is greater than zero, store char in each of u consecutive characters of memory beginning at c-addr.
0 header, fill
            ' -rot call,
                 0 literal,
                   ?do,
            ' 2dup call,
              ' c! call,
              ' 1+ call,
                   loop,
           ' 2drop jump,

\ erase ( addr u -- ) if u is greater than zero, clear all bits in each of u consecutive address units of memory beginning at addr.
0 header, erase
                 0 literal,
            ' fill jump,

\ >in offset to parse area within input buffer
var, >in

\ accept ( c-addr +n1 -- +n2 ) receive string of, at most, n1 chars into c-addr, returning number of chars (n2)
0 header, accept
                 0 literal, \ c-addr n1 0
                   begin,   \ c-addr n1 n2
              ' 1+ call,    \ c-addr n1 n2+                  increment n2
            ' 2dup call,    \ c-addr n1 n2+ n1 n2+
               ' < call,    \ c-addr n1 n2+ <
                   if,      \ c-addr n1 n2+                  bounds check
            ' drop call,    \ c-addr n1
             ' nip call,    \ n1
            ' exit call,    \ n1
                   then,    \ c-addr n1 n2+
             ' rot call,    \ n1 n2+ c-addr
             ' key call,    \ n1 n2+ c-addr key
            ' swap call,    \ n1 n2+ key c-addr
            ' 2dup call,    \ n1 n2+ key c-addr key c-addr
               ' ! call,    \ n1 n2+ key c-addr              store key
              ' 1+ call,    \ n1 n2+ key c-addr+
            ' swap call,    \ n1 n2+ c=addr+ key
             ' dup call,    \ n1 n2+ c-addr+ key key 
                13 literal, \ n1 n2+ c-addr+ key key 13
              ' <> call,    \ n1 n2+ c-addr+ key <>13
            ' swap call,    \ n1 n2+ c-addr+ <>13 key
                10 literal, \ n1 n2+ c-addr+ <>13 key 10
              ' <> call,    \ n1 n2+ c-addr+ <>13 <>10
             ' and call,    \ n1 n2+ c-addr+ <>CRLF
                   while,   \ n1 n2+ c-addr+
            ' -rot call,    \ c-addr+ n1 n2+
                   repeat,  \ c-addr+ n1 n2+
            ' drop call,    \ n1 n2+
             ' nip jump,    \ n2+

\ source-id ( -- 0 | -1 ) Identifies the input source (-1=string [evaluate], 0=input device)
var, source-id

\ refill ( -- flag ) fill input buffer from input (do nothing and return false when evaluating strings)
0 header, refill
       ' source-id call,
               ' @ call,
              ' 0= call,
             ' dup call, \ return value
                   if,
          ' source call,
            ' 2dup call, \ TODO REMOVE
           ' erase call, \ TODO REMOVE
          ' accept call,
            ' drop call, \ TODO SET LEN
                 0 literal,
             ' >in call,
               ' ! call,
                   then,
                   ret,

\ parse ( char "ccc<char>" -- c-addr u ) parse ccc delimited by char
0 header, parse
          ' source call, \ char c-addr u
            ' over call, \ char c-addr u c-addr
               ' + call, \ char c-addr end
              ' 1+ call, \ char c-addr end+
            ' swap call, \ char end c-addr
             ' >in call, \ char end c-addr in-addr
               ' @ call, \ char end c-addr in
               ' + call, \ char end start
            ' tuck call, \ char start end start
                   ?do,  \ char start
            ' over call, \ char start char
               ' i call, \ char start char i
              ' c@ call, \ char start char c
            ' over call, \ char start char c char
              ' bl call,
               ' = call, \ char start char c sp?
                   if,   \ char start char c
              ' 1- call,
               ' > call, \ char start c<=$20
                   else,
               ' = call, \ char start =?
                   then,
                   if,
             ' nip call, \ start
               ' i call, \ start i
            ' over call, \ start i start
               ' - call, \ start len
             ' dup call, \ start len len
              ' 1+ call, \ start len len+
             ' >in call, \ start len in
              ' +! call, \ start len
          ' unloop call,
            ' exit call,
                   then,
                   loop,
             ' nip call, \ start
          ' source call, \ start c-addr u
               ' + call, \ start end
            ' over call, \ start end start
               ' - call, \ start len
             ' dup call, \ start len len
              ' 1+ call, \ start len len+
             ' >in call, \ start len in
              ' +! jump, \ start len

\ (skip) ( char "<chars>..." -- "..." ) skip leading delimeter chars
0 header, (skip)
          ' source call, \ char c-addr u
             ' >in call, \ char c-addr u in
               ' @ call, \ char c-addr u in
             ' rot call, \ char c-addr u in c-addr
               ' + call, \ char c-addr u inaddr
            ' tuck call, \ char inaddr c-addr u
               ' + call, \ char inaddr end
            ' swap call, \ char end inaddr
                   ?do,  \ char
             ' dup call, \ char char
               ' i call, \ char char addr
              ' c@ call, \ char char c
            ' over call, \ char char c char
              ' bl call, \ char char c char $20
               ' = call, \ char char c sp?
                   if,   \ char char c
            ' swap call, \ char c char
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
            ' drop jump,

\ parse-name ( "<spaces>name<space>" -- c-addr u ) skip leading space and parse name delimited by space
0 header, parse-name
              ' bl call,
             ' dup call,
          ' (skip) call,
           ' parse jump,

\ cmove ( c-addr1 c-addr2 u -- ) copy characters (from lower to higher addresses)
0 header, cmove
                 0 literal,
                   ?do,
            ' over call,
              ' c@ call,
            ' over call,
              ' c! call,
              ' 1+ call, \ TODO: char+
            ' swap call,
              ' 1+ call, \ TODO: char+
            ' swap call,
                   loop,
           ' 2drop jump,

\ >counted ( c-addr u -- c-addr ) string address and length to counted string address (non-standard)
0 header, >counted
             ' dup call, \ c-addr u u
             ' pad call, \ c-addr u u pad
              ' c! call, \ c-addr u -- length
             ' pad call, \ c-addr u pad
              ' 1+ call, \ c-addr u pad+
            ' swap call, \ c-addr pad+ u
           ' cmove call,
             ' pad jump, \ pad

\ count ( c-addr1 -- c-addr2 u ) counted string to c-addr and length
0 header, count
             ' dup call,
              ' c@ call,
            ' swap call,
              ' 1+ call,
            ' swap jump,

var, latest \ common, but non-standard (confusing name conflict)

0 header, (tolower) \ non-standard
             ' dup call,
         char A 1- literal,
               ' > call,
            ' over call,
         char Z 1+ literal,
               ' < call,
             ' and call,
                   if,
                32 literal,
               ' + call,
                   then,
                   ret,

\ (equal) ( c-addr1 u1 c-addr2 u2 -- flag ) compare strings for equality (non-standard)
0 header, (equal)
             ' rot call,
            ' over call,
              ' <> call, \ lengths unequal?
                   if,
            ' drop call,
           ' 2drop call,
           ' false call,
            ' exit call,
                   then,
                 0 literal,
                   do,
            ' 2dup call,
               ' i call,
               ' + call,
              ' c@ call,
       ' (tolower) call,
            ' swap call,
               ' i call,
               ' + call,
              ' c@ call,
       ' (tolower) call,
              ' <> call,
                   if,
           ' 2drop call,
           ' false call,
          ' unloop call,
            ' exit call,
                   then,
                   loop,
           ' 2drop call,
            ' true jump,

\ find ( c-addr -- c-addr 0 | xt 1 | xt -1 ) find named definition (0=not found, 1=immediate, -1=non-immediate)
0 header, find
          ' latest call,
               ' @ call,    \ find link
                   begin,
            ' 2dup call,    \ find link find link
                 3 literal,
               ' + call,    \ find link find link+3
             ' dup call,
              ' 1- call,    \ find link find link+3 link+2
              ' c@ call,    \ find link find link+3 len/flag
               $7f literal,
             ' and call,    \ find link find link+3 len
             ' rot call,    \ find link link+3 len find
           ' count call,    \ find link link+3 len find len
           ' 2over call,    \ find link link+3 len find len link+3 len
         ' (equal) call,    \ find link link+3 len eq?
                   if,      \ find link link+3 len
               ' + call,    \ find link xt
            ' -rot call,    \ xt find link
             ' nip call,    \ xt link
                 2 literal,
               ' + call,    \ xt link+2
               ' @ call,    \ xt len/flag
               $80 literal,
             ' and call,    \ xt flag
              ' 0= call,
                   if,
                -1 literal, \ xt -1 (non-immediate)
                   else,
                 1 literal, \ xt 1 (immediate)
                   then,
            ' exit call,    \ xt (names match!)
                   then,    \ find link link+3 len
           ' 2drop call,    \ find link
               ' @ call,    \ find link (follow link)
             ' dup call,
              ' 0= call,
                   if,
            ' drop call,    \ find
                 0 literal, \ find 0  (not found)
            ' exit call,
                   then,
                   again,
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
             ' dup call,   \ n a c c
              ' 0> call,   \ n a c c0>
                   while,  \ n a c
            ' -rot call,   \ c n a
             ' dup call,   \ c n a a
              ' c@ call,   \ c n a d
               '0' literal,
               ' - call,   \ c n a d
             ' dup call,   \ c n a d d
                10 literal, \ cn a d d 10
               ' < call,   \ c n a d <
                   if,     \ c n a d  valid?
             ' rot call,   \ c a d n
            ' base call,   \ c a d n b
               ' @ call,   \ c a d n b
               ' * call,   \ c a d n
               ' + call,   \ c a n
            ' swap call,   \ c n a
              ' 1+ call,   \ c n a
             ' rot call,   \ n a c
              ' 1- call,   \ n a c
                   else,   \ c n a d  invalid
            ' drop call,   \ c n a
             ' rot call,   \ n a c
            ' exit call,
                   then,   \ n a c
                   repeat,
                   ret,

\ state ( -- a-addr ) compilation-state flag (true=compiling)
var, state

\ interpret ( c-addr u -- ) (implementation defined)
0 header, interpret
        ' >counted call,
            ' find call, \ c-addr 0 | xt 1 | xt -1
             ' dup call,
              ' 0= call,
                   if, \ not found?
            ' drop call,
           ' count call,
            ' over call, \ determine whether prefixed with -
              ' c@ call,
            char - literal,
               ' = call,
                   if,
              ' 1- call, \ decrement length
            ' swap call,
              ' 1+ call, \ increment address
            ' swap call,
                -1 literal, \ multiplier
                   else,
                 1 literal, \ multiplier
                   then,
            ' -rot call,
            ' 2dup call,
                 0 literal,
            ' -rot call,
         ' >number call,
             ' dup call,
              ' 0= call,
                   if, \ proper number
           ' 2drop call,
            ' -rot call,
           ' 2drop call,
               ' * call,
           ' state call,
               ' @ call,
                   if,   \ compile?
        ' literal, call,
                   then,
                   else, \ error
           ' 2drop call,
            ' drop call,
              ' cr call,
            ' type call,
            char ? literal,
            ' emit call,
    ' (clear-data) call,
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
             ' dup call,
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
  ' (clear-return) call,
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
           ' 2drop call,
              ' bl call,
            ' emit call,
            char o literal,
            ' emit call,
            char k literal,
            ' emit call,
              ' cr call,
                   repeat,
                   \ TODO: failed to refill
                   ret,

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

\ header, ( "<spaces>name -- ) append header to dictionary (non-standard, note: no flag)
0 header, header,
          ' latest call,
               ' @ call, \ link to current (soon to be previous) word
            ' here call,
          ' latest call,
               ' ! call, \ update latest to this word
               ' , call, \ append link
      ' parse-name call, ( addr len -- )
             ' dup call,
              ' c, call, \ append len
            ' over call,
               ' + call,
            ' swap call, ( end start -- )
                   do,   \ append name
               ' i call,
              ' c@ call,
              ' c, call,
                   loop,
                   ret,  ( addr len )

\ bye ( -- ) halt machine TODO: not needed once bootstrapped
0 header, bye
              zero halt,

( --- end of dictionary ------------------------------------------------------ )

                   start,

    ' (clear-data) call,
         ' decimal call, \ default base
memory-size $500 - literal, \ $ff bytes above stacks
     ' source-addr call,
               ' ! call,
               $ff literal, \ size ($400 bytes for stacks/$ff elements each)
      ' source-len call,
               ' ! call,
            ' quit call,
                 0 halt,

here     ' h      16 + s! \ update dictionary pointer to compile-time position
latest @ ' latest 16 + s! \ update latest to compile-time

." Kernel size: " here . ." bytes" cr

\ UNUSED unused ?
