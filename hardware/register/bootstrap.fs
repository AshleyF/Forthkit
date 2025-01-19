header, : ] header, ] ;

: [char] parse-name drop c@ postpone literal ; immediate

: ( [char] ) parse 2drop ; immediate
: \ 10 ( newline ) parse 2drop ; immediate

( now we can use comments like this! )
\ or like this to the end of the line!

\ create ( "<spaces>name" -- ) parse name, create definition, runtime ( -- a-addr ) pushes address of data field (does not allocate data space in data field). Execution semantics may be extended by does>.
: create
        header,  \ code to push dfa and return
   x pc cp,      \ x=pc
   14 y ldc,     \ y=14
  x x y add,     \ x+=y
      x pushd,
        ret,
;

: >body ( xt -- a-addr ) 16 + ;

: buffer: ( u "<name>" -- ; -- addr ) create allot ;

( patch return to jump to instance code address given )
: (does)
  latest @ 2 + \ to length/flag
  dup c@ + 1+ \ to code
  10 + \ to return
  33 over ! \ 2100 -> ld+ zero pc pc  pc=[pc]  --  jump to following address  TODO: assemble?
  2 + ! \ to address passed to us
;

\ ['] ( "<spaces>name" -- ) parse and find name
: ['] ' postpone literal ; immediate

\ does> ( C: colon-sys1 -- colon-sys2 ) append run-time and initialization semantics below to definition.
\       ( -- ) ( R: nest-sys1 -- ) Runtime: replace execution semantics
\       ( i * x -- i * x a-addr ) ( R: -- nest-sys2 ) Initiation: push data field address
\       ( i * x -- j * x ) Execution: execute portion of definition beginning with initiation semantics
: does>
  here 12 + literal, \ compile push instance code address
  ['] (does) jump,
; immediate

\ variable ( "<spaces>name" -- ) parse name, create definition, reserve cell of data space, runtime ( -- a-addr ) push address of data space (note: uninitialized)
: variable create 1 cells allot ;

\ constant ( x "<spaces>name" -- ) parse name, create definition to push x at runtime ( -- x )
: constant create , does> @ ;

\ can redefine
32 constant bl
2 constant cell

\ can redefine
: cells cell * ; \ note: less efficient than 2*
: cell+ cell + ;

\ another create ... does> ... example
: point create , , does> dup cell+ @ swap @ ;

( --- primitive control-flow ------------------------------------------------- )

: branch, ( -- dest ) 0 jump,  here 2 - ; \ dummy jump, push pointer to patch
: 0branch, ( -- dest ) x popd,  0 y ldv,  here 2 -  pc y x cp?, ; \ dummy jump if 0 to address, push pointer to patch
: patch, ( orig -- ) here swap ! ; \ patch jump to continue here (note: s! -> !)

\ ... if ... then
\ ... if ... else ... then
: if ( C: -- orig ) 0branch, ; immediate \ dummy branch on 0, push pointer to address
: then ( orig -- ) patch, ; immediate \ patch if/else to continue here
: else ( C: orig1 -- orig2 ) branch, swap patch, ; immediate \ patch previous branch to here, dummy unconditionally branch over false block (note: then -> patch,)

: begin ( C: -- dest ) here ; \ begin loop
: again ( C: dest -- ) jump, ; \ jump back to beginning
: until ( C: dest -- ) 0branch, ! ( s! -> ! ) ; \ branch on 0 to address
: while ( C: dest -- orig dest ) 0branch, swap ; \ continue while condition met (0= if), 
: repeat ( C: orig dest -- ) again ( again, -> again ) here swap ! ( s! -> ! ) ; \ jump back to beginning, patch while to here

: do ( limit start -- ) ( C: -- false addr ) \ begin do-loop (immediate 2>r begin false)
           ['] 2>r call,
                   false \ no addresses to patch (initially)
                   begin ( begin, -> begin ) ;

: ?do ( limit start -- ) ( C: -- false addr true addr )
          ['] 2dup call,
            ['] <> call,
                   false \ terminator for patching
                   if ( if, -> if )
                   true  \ patch if to loop
           ['] 2>r call,
                   begin ( begin, -> begin ) ;

: leave ( C: -- addr true )
                   branch,
                   -rot true -rot ; \ patch to loop (swap under if address)

: +loop ( n -- ) ( C: ... flag addr -- ) \ end do-loop, add n to loop counter (immediate r> + r@ over >r < if again then 2r> 2drop)
             ['] r> call,
              ['] + call,
             ['] r@ call,
           ['] over call,
             ['] >r call,
              ['] < call,
                    if ( if, -> if )
                    swap again ( again, -> again )
                    then ( then, -> then, )
                    begin while
                    patch,
                    repeat
            ['] 2r> call,
          ['] 2drop call, ;

: loop ( C: addr -- )
                 1 literal,
                   +loop ; \ end do-loop (immediate 1 +loop)

: str= \ non-standard gforth
  rot over <> if drop 2drop false exit then \ not equal lengths
  0 do
    2dup c@ swap c@ <> if 2drop false unloop exit then \ chars not equal
    char+ swap char+ 
  loop
  2drop true ;

: write-boot-block ( -- ) 0 0 here write-block ; \ taken from assembler.fs
