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
: ldc,    1 2nybbles, c, ; \ ( v x -- ) x=v (load constant signed v into x)
: ld+,    2 4nybbles, ; \ ( z y x -- ) z<-[y] y+=x (load from memory and inc/dec pointer)
: st+,    3 4nybbles, ; \ ( z y x -- ) z->[y] y+=x (store to memory and inc/dec pointer)
: cp?,    4 4nybbles, ; \ ( z y x -- ) z=y if x=0 (conditional copy)
: add,    5 4nybbles, ; \ ( z y x -- ) z=y+x (addition)
: sub,    6 4nybbles, ; \ ( z y x -- ) z=y-x (subtraction)
: mul,    7 4nybbles, ; \ ( z y x -- ) z=y*x (multiplication)
: div,    8 4nybbles, ; \ ( z y x -- ) z=y/x (division)
: nand,   9 4nybbles, ; \ ( z y x -- ) z=y nand x (not-and)
: shl,   10 4nybbles, ; \ ( z y x -- ) z=y<<x (bitwise shift-left)
: shr,   11 4nybbles, ; \ ( z y x -- ) z=y>>x (bitwise shift-right)
: in,    12 2nybbles, ; \ ( y x -- ) x=getc() (read from console)
: out,   13 2nybbles, ; \ ( y x -- ) putc(x) (write to console)
: read,  14 4nybbles, ; \ ( z y x -- ) read(z,y,x) (file z of size y -> address x)
: write, 15 4nybbles, ; \ ( z y x -- ) write(z,y,x) (file z of size y <- address x)

: cp, zero cp?, ; \ ( y x -- ) y=x (unconditional copy)
: ld, zero ld+, ; \ ( y x -- ) y<-[x] (load from memory)
: st, zero st+, ; \ ( y x -- ) y->[x] (store to memory)
: jump, pc pc ld, , ; \ ( addr -- ) unconditional jump to address (following cell)
: ldv, pc two ld+, , ; \ ( val reg -- )

: push, dup dup four sub, st, ; \ ( reg ptr -- )
: pop, four ld+, ; \ ( reg ptr -- )
: pushd, d push, ; \ ( reg -- )
: popd, d pop, ; \ ( reg -- )
: pushr, r push, ; \ ( reg -- )
: popr, r pop, ; \ ( reg -- )

: literal, x ldv, x pushd, ; \ ( val -- ) push literal value onto stack

: call, pc pushr, jump, ; \ call, ( addr -- )
: ret, x popr, x x four add, pc x cp, ; \ ret, ( -- )

\ -- ADD MISSING PRIMITIVES ----------------------------------------------------

\ bye ( -- ) halt machine
: bye [ zero halt, ] ;

\ (bye) ( code -- ) halt machine with return code (non-standard)
: (bye) [ x popd,  x halt, ] ;

\ / ( y x -- quotient ) division
: / [ x popd,  y popd,  x y x div,  x pushd, ] ;

\ mod ( y x -- remainder ) remainder of division
: mod [ x popd,  y popd,  z y x div,  z z x mul,  z y z sub,  z pushd, ] ;

\ -- PRIMITIVE CONTROL FLOW ----------------------------------------------------

: s! over 8 rshift over 1+ c! c! ; \ ( val addr -- ) \ note: no memory +

: 0branch, x popd,  0 y ldv,  here 2 -  pc y x cp?, ; \ ( -- dest ) dummy jump if 0 to address, push pointer to patch
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

: postpone parse-name >counted find 1 = if call, then ; immediate \ only works for immediate words -- TODO: error for not found or non-immediate

: char parse-name drop c@ ;
: [char] char postpone literal ; immediate

: ( [char] ) parse 2drop ; immediate

( now comments like this work too )

\ create ( "<spaces>name" -- ) parse name, create definition, runtime ( -- a-addr ) pushes address of data field (does not allocate data space in data field). Execution semantics may be extended by does>.
: create
         header,  \ code to push dfa and return
    x pc cp,      \ x=pc
    14 y ldc,     \ y=14
   x x y add,     \ x+=y
       x pushd,
         ret,
;

: >body 16 + ; ( xt -- a-addr )

: buffer: create allot ; ( u "<name>" -- ; -- addr )

\ poor man's . ( n -- ) print decimal number
: . dup 10000 / dup 48 + emit 10000 * -
    dup 1000  / dup 48 + emit  1000 * -
    dup 100   / dup 48 + emit   100 * -
    dup 10    / dup 48 + emit    10 * -
                    48 + emit ;

3 4 + .
