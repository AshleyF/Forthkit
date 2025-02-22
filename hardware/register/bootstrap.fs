header, : ] header, ] ;

: \ 10 parse 2drop ; immediate

: char parse-name drop c@ ;
: [char] char postpone literal ; immediate

: ( [char] ) parse 2drop ; immediate

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
  here 12 + postpone literal \ compile push instance code address
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

: begin ( C: -- dest ) here ; immediate \ begin loop
: again ( C: dest -- ) jump, ; immediate \ jump back to beginning
: until ( C: dest -- ) 0branch, ! ( s! -> ! ) ; immediate \ branch on 0 to address
: while ( C: dest -- orig dest ) 0branch, swap ; immediate \ continue while condition met (0= if), 
: repeat ( C: orig dest -- ) postpone again ( again, -> postpone again, because now immediate ) here swap ! ( s! -> ! ) ; immediate \ jump back to beginning, patch while to here

: do ( limit start -- ) ( C: -- false addr ) \ begin do-loop (immediate 2>r begin false)
           ['] 2>r call,
                   false \ no addresses to patch (initially)
          postpone begin ( begin, -> postpone begin ) ; immediate

: ?do ( limit start -- ) ( C: -- false addr true addr )
          ['] 2dup call,
           ['] 2>r call,
            ['] <> call,
                   false \ terminator for patching
          postpone if ( if, -> postpone if )
                   true  \ patch if to loop
          postpone begin ( begin, -> postpone begin ) ; immediate

: leave ( C: -- addr true )
                   branch,
                   -rot true -rot ; immediate \ patch to loop (swap under if address)

: +loop ( n -- ) ( C: ... flag addr -- ) \ end do-loop, add n to loop counter (immediate r> + r@ over >r < if again then 2r> 2drop)
             ['] r> call,
              ['] + call,
             ['] r@ call,
           ['] over call,
             ['] >r call,
              ['] < call,
                    postpone if ( if, -> if )
                    swap postpone again ( again, -> again )
                    postpone then ( then, -> then, )
                    begin while
                    patch,
                    repeat
            ['] 2r> call,
          ['] 2drop call, ; immediate

: loop ( C: addr -- )
        1 postpone literal
          postpone +loop ; immediate \ end do-loop (immediate 1 +loop)

: i 2r@  ( including return from here ) drop ;
: j 2r> 2r@ drop -rot 2>r ;

\ : str= \ non-standard gforth
\   rot over <> if drop 2drop false exit then \ not equal lengths
\   0 do
\     2dup c@ swap c@ <> if 2drop false unloop exit then \ chars not equal
\     char+ swap char+ 
\   loop
\   2drop true ;

: spaces 0 max 0 ?do space loop ;

: u< 2dup xor 0< if nip else - then 0< ;
: u> swap u< ;

: within ( test low high -- flag ) over - -rot - u> ;

: .( [char] ) parse type ; immediate

: s" branch, here [char] " parse tuck here swap cmove dup allot rot patch, swap literal, literal, ; immediate

\ branch,  -> branch                          compile dummy jump
\ here     -> branch here
\ [char] " -> branch here "
\ parse    -> branch here addr len
\ tuck     -> branch here len addr len
\ here     -> branch here len addr len here
\ swap     -> branch here len addr here len
\ cmove    -> branch here len                 copy string into compiled word
\ dup      -> branch here len len
\ allot    -> branch here len
\ rot      -> here len branch
\ patch,   -> here len                        patch dummy jump
\ swap     -> len here
\ literal, -> len                             compile literal address
\ literal, ->                                 compile literal length

: c" branch, here [char] " parse tuck dup c, here swap cmove allot swap patch, literal, ; immediate

\ branch,  -> branch                          compile dummy jump
\ here     -> branch here
\ [char] " -> branch here "
\ parse    -> branch here addr len
\ tuck     -> branch here len addr len
\ dup      -> branch here len addr len len
\ c,       -> branch here len addr len        compile string count
\ here     -> branch here len addr len here
\ swap     -> branch here len addr here len
\ cmove    -> branch here len                 copy string into compiled word
\ allot    -> branch here
\ swap     -> here branch
\ patch,   -> here                            patch dummy jump
\ literal, ->                                 compile literal address

: ." postpone s" ['] type call, ; immediate

: latest-cfa latest @ 2 + dup c@ 127 and + 1+ ; ( non-standard )
: recurse latest-cfa call, ; immediate
: tail-recurse latest-cfa jump, ; immediate

: move 0 ?do over @ over ! cell+ swap cell+ swap loop 2drop ; \ TODO: handle overlap

: (abort") rot 0<> if type abort else 2drop then ; \ internal non-standard
: abort" postpone s" ['] (abort") call, ; immediate

\ evaluate ( i * x c-addr u -- j * x ) save the current input source specification.
\   Store minus-one (-1) in SOURCE-ID if it is present.
\   Make the string described by c-addr and u both the input source and input buffer, set >IN to zero, and
\   interpret. When the parse area is empty, restore the prior input source specification.
: evaluate
  source-len  @ >r    source-len  !
  source-addr @ >r    source-addr !
  source-id   @ >r -1 source-id   !
  >in         @ >r  0 >in         !
  (evaluate)
  0  source-id   !
  r> >in         !
  r> source-id   !
  r> source-addr !
  r> source-len  !
;

: >= 2dup > -rot = or ; \ non-standard

: write-boot-block ( -- ) 0 0 here write-block ; \ taken from assembler.fs

.( writing boot block )
write-boot-block
