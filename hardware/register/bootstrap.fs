header, : ] header, ] ;

: immediate latest @ 2 + dup @ 128 or swap ! ;

: \ 10 parse 2drop ; immediate

\ now we can use comments like this

\ -- ASSEMBLER WORDS -----------------------------------------------------------

: pc 0 ; \ TODO 0 constant pc
: zero 1 ; \ TODO 1 constant zero
: one 2 ; \ TODO 2 constant one
: two 3 ; \ TODO 3 constant two
: four 4 ; \ TODO 4 constant four
: eight 5 ; \ TODO 5 constant eight
: twelve 6 ; \ TODO 6 constant twelve
: fifteen 7 ; \ TODO 7 constant fifteen
: #t 8 ; \ TODO 8 constant #t
: #f zero ; \ TODO: zero constant #f
: x 9 ; \ TODO: 9 constant x
: y 10 ; \ TODO: 10 constant y
: z 11 ; \ TODO: 11 constant z
: w 12 ; \ TODO: 12 constant w
: d 13 ; \ TODO: 13 constant d
: r 14 ; \ TODO: 14 constant r

: 2nybbles, 4 lshift or c, ;
: 4nybbles, 2nybbles, 2nybbles, ;

: halt,   0 2nybbles, ; \ ( x -- ) halt(x) (halt with exit code x)
: add,    1 4nybbles, ; \ ( z y x -- ) z=y+x (addition)
: sub,    2 4nybbles, ; \ ( z y x -- ) z=y-x (subtraction)
: mul,    3 4nybbles, ; \ ( z y x -- ) z=y*x (multiplication)
: div,    4 4nybbles, ; \ ( z y x -- ) z=y/x (division)
: nand,   5 4nybbles, ; \ ( z y x -- ) z=y nand x (not-and)
: shl,    6 4nybbles, ; \ ( z y x -- ) z=y<<x (bitwise shift-left)
: shr,    7 4nybbles, ; \ ( z y x -- ) z=y>>x (bitwise shift-right)
: in,     8 2nybbles, ; \ ( y x -- ) x=getc() (read from console)
: out,    9 2nybbles, ; \ ( y x -- ) putc(x) (write to console)
: read,  10 4nybbles, ; \ ( z y x -- ) read(z,y,x) (file z of size y -> address x)
: write, 11 4nybbles, ; \ ( z y x -- ) write(z,y,x) (file z of size y <- address x)
: ld16+, 12 4nybbles, ; \ ( z y x -- ) z<-[y] y+=x (load from memory and inc/dec pointer)
: st16+, 13 4nybbles, ; \ ( z y x -- ) z->[y] y+=x (store to memory and inc/dec pointer)
: lit8,  14 2nybbles, c, ; \ ( v x -- ) x=v (load constant signed v into x)
: cp?,   15 4nybbles, ; \ ( z y x -- ) z=y if x=0 (conditional copy)

: cp, zero cp?, ; \ ( y x -- ) y=x (unconditional copy)
: ld16, zero ld16+, ; \ ( y x -- ) y<-[x] (load from memory)
: st16, zero st16+, ; \ ( y x -- ) y->[x] (store to memory)

: jump, pc pc ld16, , ; \ ( addr -- ) unconditional jump to address (following cell)
: lit16, pc two ld16+, , ; \ ( val reg -- )
: not, dup nand, ; \ (   y x -- ) 

: pick [ x d ld16,  x x four mul,  x x four add,  x x d add,  x x ld16, x d st16, ] ; \ ( n...x -- n...x n ) copy 0-based nth stack value to top
: and, 2 pick -rot nand, dup not, ;
: or, dup dup not, over dup not, nand, ; \ ( z y x -- )

: push, dup dup four sub, st16, ; \ ( reg ptr -- )
: pop, four ld16+, ; \ ( reg ptr -- )
: pushd, d push, ; \ ( reg -- )
: popd, d pop, ; \ ( reg -- )
: pushr, r push, ; \ ( reg -- )
: popr, r pop, ; \ ( reg -- )

: literal, x lit16, x pushd, ; \ ( val -- ) push literal value onto stack

: call, pc pushr, jump, ; \ call, ( addr -- )
: ret, x popr, x x four add, pc x cp, ; \ ret, ( -- )

: read-block [ x popd,  y popd,  z popd,  z y x read, ] ; \ ( file addr size -- ) block file of size -> address
: write-block [ x popd,  y popd,  z popd,  z y x write, ] ; \ ( file addr size -- ) block file of size -> address

\ -- ADD MISSING PRIMITIVES ----------------------------------------------------

: nand [ x popd,  y popd,  x y x nand,  x pushd, ] ; \ ( y x -- not-and ) not and (non-standard)
: invert [ x d ld16,  x x not,  x d st16, ] ; \ ( x -- result ) invert bits
: negate [ x d ld16,  x x not,  x x one add,  x d st16, ] ; \ ( x -- result ) arithetic inverse (invert 1+) (0 swap -)

: ?dup [ x d ld16,  y popr,  y y four add,  pc y x cp?,  x pushd,  pc y cp, ] ; \ ( x -- 0 | x x ) duplicate top stack value if non-zero
: 2swap [ x d ld16,  y d eight add,  z y ld16,  z d st16,  x y st16,  x d four add,  y x ld16,  z d twelve add,  w z ld16,  w x st16,  y z st16, ] ; \ ( w z y x -- y x w z ) swap top two pairs of stack values
: 0<> [ x d ld16,  y #t cp,  y #f x cp?,  y d st16, ] ; \ ( y x -- b ) true if not equal to zero

: bye [ zero halt, ] ; \ ( -- ) halt machine
: (bye) [ x popd,  x halt, ] ; \ ( code -- ) halt machine with return code (non-standard)
: / [ x popd,  y popd,  x y x div,  x pushd, ] ; \ ( y x -- quotient ) division
: mod [ x popd,  y popd,  z y x div,  z z x mul,  z y z sub,  z pushd, ] ; \ ( y x -- remainder ) remainder of division
: /mod [ x popd,  y popd,  z y x div,  w z x mul,  w y w sub,  w pushd,  z pushd, ] ; \ ( y x -- remainder quotient ) remainder and quotient result of division

: hex 16 base ! ; \ ( -- ) set number-conversion radix to 16
: octal 8 base ! ; \ ( -- ) set number-conversion radix to 8 (non-standard)
: binary 2 base ! ; \ ( -- ) set number-conversion radix to 2 (non-standard)

: 2* [ x popd,  x x one shl,  x pushd, ] ; \ ( x -- result ) multiply by 2 (1 lshift)
: 2/ [ x popd,  y one fifteen shl,  y y x and,  x x one shr,  x x y or,  x pushd, ] ; \ ( x -- result ) divide by 2 (1 rshift)

: depth [ 8 x lit8,  x x d sub,  x x four div,  x pushd, ] ; \ ( -- depth ) data stack depth \ TODO: why 8?

: abort [ (clear-data) quit ] ;

\ -- PRIMITIVE CONTROL FLOW ----------------------------------------------------

: s! over 8 rshift over 1+ c! c! ; \ ( val addr -- ) \ note: no memory +

: 0branch, x popd,  0 y lit16,  here 2 -  pc y x cp?, ; \ ( -- dest ) dummy jump if 0 to address, push pointer to patch
: branch, 0 jump,  here 2 - ; \ ( -- dest ) 
: patch, here swap s! ; \ ( orig -- ) 

\ ... if ... then | ... if ... else ... then
: if 0branch, ; immediate \ ( C: -- orig ) dummy branch on 0, push pointer to address
: then patch, ; immediate \ ( orig -- ) patch if/else to continue here
: else branch, swap patch, ; immediate \ ( C: orig1 -- orig2 ) patch previous branch to here, dummy unconditionally branch over false block
 
\ \ begin ... again | begin ... until | begin ... while ... repeat  (note: not begin ... while ... again!)
: begin here ; immediate \ ( C: -- dest ) begin loop
: again jump, ; immediate \ ( C: dest -- ) jump back to beginning
: until 0branch, s! ; immediate \ ( C: dest -- ) branch on 0 to address \ NEW: not in kernel;
: while 0branch, swap ; immediate \ ( C: dest -- orig dest ) continue while condition met (0= if), 
: repeat jump, here swap s! ; immediate \ ( C: orig dest -- ) jump back to beginning, patch while to here

\ -- CONTINUE BOOTSTRAPPING ----------------------------------------------------

\ these are no-ops on RM16, but are standard and should be used for portability
: align ; \ ( -- ) reserve space to align data space pointer (no-op on RM16)
: aligned ; \ ( addr -- addr ) align address (no-op on RM16)
: chars ; \ ( x -- n-chars ) size in address units of n-chars (no-op)

: char+ 1+ ; \ ( addr -- addr ) add size of char to address (1+)
: cells 2* ; \ ( x -- n-cells ) size in address units of n-cells (2*)
: cell+ 2 + ; \ ( addr -- addr ) add size of cell to address (2 +)

: 2! tuck ! cell+ ! ; \ ( y x addr -- ) store x y at consecutive addresses (tuck ! cell+ !)
: 2@ dup cell+ @ swap @ ; \ ( addr -- y x ) fetch pair of consecutive addresses (dup cell+ @ swap @)

: xor 2dup or -rot and invert and ; \ ( y x -- result ) logical/bitwise exclusive or (2dup or -rot and invert and)
: abs dup 0< if negate then ; \ ( x -- |x| ) absolute value (dup 0< if negate then)
: j 2r> 2r@ drop -rot 2>r ; \ ( -- x ) ( R: x -- x ) copy next outer loop index (2r> 2r@ drop -rot 2>r) (x r twelve add, x x ld16, x pushd, ret,)

: postpone parse-name >counted find 1 = if call, then ; immediate \ only works for immediate words -- TODO: error for not found or non-immediate

: char parse-name drop c@ ;
: [char] char postpone literal, ; immediate

: ( [char] ) parse 2drop ; immediate

( now comments like this work too )

\ create ( "<spaces>name" -- ) parse name, create definition, runtime ( -- a-addr ) pushes address of data field (does not allocate data space in data field). Execution semantics may be extended by does>.
: create
         header,  \ code to push dfa and return
    x pc cp,      \ x=pc
    14 y lit8,    \ y=14
   x x y add,     \ x+=y
       x pushd,
         ret,
;

: >body 16 + ; ( xt -- a-addr )

: buffer: create allot ; ( u "<name>" -- ; -- addr )

( patch return to jump to instance code address given )
: (does)
  latest @ 2 + \ to length/flag
  dup c@ + 1+ \ to code
  10 + \ to return
  33 over ! \ 2100 -> ld16+ zero pc pc  pc=[pc]  --  jump to following address  TODO: assemble?
  2 + ! \ to address passed to us
;

\ ['] ( "<spaces>name" -- ) parse and find name
: ['] ' postpone literal ; immediate

\ does> ( C: colon-sys1 -- colon-sys2 ) append run-time and initialization semantics below to definition.
\       ( -- ) ( R: nest-sys1 -- ) Runtime: replace execution semantics
\       ( i * x -- i * x a-addr ) ( R: -- nest-sys2 ) Initiation: push data field address
\       ( i * x -- j * x ) Execution: execute portion of definition beginning with initiation semantics
: does>
  here 12 + postpone literal \ compile push instance code address
  ['] (does) jump,
; immediate

\ variable ( "<spaces>name" -- ) parse name, create definition, reserve cell of data space, runtime ( -- a-addr ) push address of data space (note: uninitialized)
: variable create 1 cells allot ;

\ constant ( x "<spaces>name" -- ) parse name, create definition to push x at runtime ( -- x )
: constant create , does> @ ;

\ TODO 32 constant bl \ ( -- c ) space character value (32 constant bl)
: bl 32 ;

: space bl emit ; \ ( -- ) emit space character (bl emit)
: word dup (skip) parse >counted ; \ ( char "<chars>ccc<char>" -- c-addr ) skip leading delimeters, parse ccc delimited by char, return transient counted string
: ' bl word find 0= if drop 0 then ; \ ( "<spaces>name" -- xt ) skip leading space, parse and find name

\ -- SECONDARY CONTROL FLOW ----------------------------------------------------

\ <limit> <start> do ... loop
\ <limit> <start> do ... <n> +loop
\ <limit> <start> do ... unloop exit ... loop
\ <limit> <start> do ... if ... leave then ... loop
: do \ ( limit start -- ) ( C: -- false addr ) \ begin do-loop (immediate 2>r begin false)
           ['] 2>r call,
                   false \ no addresses to patch (initially)
                   begin ; immediate

: ?do \ ( limit start -- ) ( C: -- false addr true addr )
          ['] 2dup call,
            ['] <> call,
                   false \ terminator for patching
                   if
                   true  \ patch if to loop
           ['] 2>r call,
                   begin ; immediate

: leave \ ( C: -- addr true )
                   branch,
                   -rot true -rot ; immediate \ patch to loop (swap under if address)

: loop, \ ( C: addr -- )
                 1 literal,
            ['] r> call,
             ['] + call,
            ['] r@ call,
          ['] over call,
            ['] >r call,
             ['] < call,
                   [ if swap again then ]
\                   if,
\                   swap again,
\                   then,
                   begin while
                   patch,
                   repeat
           ['] 2r> call,
         ['] 2drop call, ; immediate

\ -- FORMATTED NUMBERS ---------------------------------------------------------

variable np \ ( -- addr ) return address of pictured numeric output pointer (non-standard)

: <# pad 64 + np ! ; \ <# ( -- ) initialize pictured numeric output (pad 64 + np !)
: hold np -1 over +! @ c! ; \ ( char -- ) add char to beginning of pictured numeric output (np -1 over +! c!)
: # swap base @ 2dup mod 48 + hold / swap ; \ ( ud -- ud ) prepend least significant digit to pictured numeric output, return ud/base \ TODO: doesn't work with negative values!
: #s swap begin swap # swap dup 0<> while repeat swap ; \ ( ud -- ud ) convert all digits using # (appends at least 0) \ TODO: support double numbers
: sign 0< if 45 hold then ; \ ( n -- ) if negative, prepend '-' to pictured numeric output
: #> 2drop np @ pad 64 + over - ; \ ( xd -- addr len ) make pictured numeric output string available (np @ pad 64 + over -)
: holds begin dup while 1- 2dup + c@ hold repeat 2drop ; \ ( addr len -- ) add string to beginning of pictured numeric output (begin dup while 1- 2dup + c@ hold repeat 2drop)
: s>d dup 0< ; \ ( n -- d ) convert number to double-cell
: . dup abs s>d <# #s rot sign #> space type ; \ ( n -- ) display value in free field format (dup abs s>d <# #s rot sign #> type space)
: d. <# #s #> space type ; \ ( u -- ) display unsigned value in free field format (from double word set)
: u. 0 d. ; \ ( u -- ) display unsigned value in free field format (0 <# #s #> type space)
\ TODO: do/loop : .s depth dup 0 do dup i - pick . loop drop ; \ ( -- ) display values on the stack non-destructively (depth dup 0 do dup i - pick . loop drop) \ TODO: ?do bug

: ? @ . ; \ ( addr -- ) display value stored at address (@ .)
: unused 65535 here - ; \ ( -- remaining ) dictionary space remaining \ TODO: consider stack space?

here .
