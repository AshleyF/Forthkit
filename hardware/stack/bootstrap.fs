header, : ] header, ] ;

: halt,   0 slot,                ;
: add,    1 slot,                ;
: sub,    2 slot,                ;
: mul,    3 slot,                ;
: div,    4 slot,                ;
: not,    5 slot,                ;
: and,    6 slot,                ;
: or,     7 slot,                ;
: xor,    8 slot,                ;
: shl,    9 slot,                ;
: shr,   10 slot,                ;
: in,    11 slot,                ;
: out,   12 slot,                ;
: read,  13 slot,                ;
: write, 14 slot,                ;
: ld16+, 15 slot,                ;
: ld8+,  16 slot,                ;
: st16+, 17 slot,                ;
: st8+,  18 slot,                ;
: 0jump, 21 slot, here      - c, ;
: next,  22 slot, here swap - c, ;
: drop,  23 slot,                ;
: dup,   24 slot,                ;
: over,  25 slot,                ;
: swap,  26 slot,                ;
: push,  27 slot,                ;
: pop,   28 slot,                ;
: peek,  29 slot,                ;
: nop,   31 slot,                ;

: (bye) [ halt, ] ;
: bye 0 (bye) ;

: + [ add, ] ;
: * [ mul, ] ;
: / [ div, ] ;

: dup [ dup, ] ;
: or [ or, ] ;

: drop [ drop, ] ;
: 2drop [ drop, drop, ] ;

: dup [ dup, ] ;
: over [ over, ] ;

: >r [ push, ] ;
: r> [ pop, ] ;
: r@ [ peek, ] ;

: immediate latest @ 2 + dup @ 128 or swap ! ;

: \ 10 parse 2drop ; immediate

\ now we can use comments like this

: 2>r [ swap, push, push, ] ; \ ( y x -- ) ( R: -- y x ) move y x pair to return stack
: 2r> [ pop, pop, swap, ] ; \ ( -- y x ) ( R: y x -- ) move y x pair from return stack
: 2r@ [ pop, peek, swap, dup, push, ] ; \ ( -- y x ) ( R: y x -- y x ) copy y x pair from return stack

: nip [ swap, drop, ] ; \ ( y x -- x ) drop second stack value
: tuck [ swap, over, ] ; \ ( y x -- x y x ) copy top stack value under second value
: 2dup [ over, over, ] ; \ ( y x -- y x y x ) duplicate top two stack values

: 1+ 1 + ;
: 1- 1 - ;

: invert [ not, ] ; \ ( x -- result ) invert bits
: negate invert 1+ ; \ ( x -- result ) arithetic inverse (invert 1+) (0 swap -)
: and [ and, ] ;
: xor [ xor, ] ;
: nand [ not, and, ] ; \ (non-standard)

: rshift [ shr, ] ;
: lshift [ shl, ] ;

: key [ in, ] ;
: emit [ out, ] ;

: read-block [ read, ] ; \ ( file addr size -- ) block file of size -> address
: write-block [ write, ] ; \ ( file addr size -- ) block file of size -> address

: 0<> [ 0= invert ] ; \ ( y x -- b ) true if not equal to zero

: hex 16 base ! ; \ ( -- ) set number-conversion radix to 16
: octal 8 base ! ; \ ( -- ) set number-conversion radix to 8 (non-standard)
: binary 2 base ! ; \ ( -- ) set number-conversion radix to 2 (non-standard)

: 2* [ 1 shl, ] ; \ ( x -- result ) multiply by 2 (1 lshift)
: 2/ [ 1 shr, ] ; \ ( x -- result ) divide by 2 (1 rshift)

: abort [ quit ] ; \ TODO (clear-data) doesn't exist

\ TODO : mod [ ] ; \ ( y x -- remainder ) remainder of division
\ TODO : /mod [ ] ; \ ( y x -- remainder quotient ) remainder and quotient result of division
\ TODO : ?dup [ ] ; \ ( x -- 0 | x x ) duplicate top stack value if non-zero
\ TODO : 2swap [ ] ; \ ( w z y x -- y x w z ) swap top two pairs of stack values
\ TODO : depth [ ] ; \ ( -- depth ) data stack depth \ TODO: why 8?

\ -- PRIMITIVE CONTROL FLOW ----------------------------------------------------

: jump, ( addr -- ) call, ret, ;
: zero, dup, dup, xor, ;

: 0branch, here 0jump, here 1- ; \ ( -- dest ) dummy jump if 0 to address, push pointer to patch
: branch, zero, 0branch, ; \ ( -- dest ) 
: patch, initslot here over - swap c! ; \ ( orig -- ) \ TODO: verify-sbyte

\ ... if ... then | ... if ... else ... then
: if 0branch, ; immediate \ ( C: -- orig ) dummy branch on 0, push pointer to address
: else branch, swap patch, ; immediate \ ( C: orig1 -- orig2 ) patch previous branch to here, dummy unconditionally branch over false block
: then patch, ; immediate \ ( orig -- ) patch if/else to continue here
 
\ begin ... again | begin ... until | begin ... while ... repeat  (note: not begin ... while ... again!)
: begin initslot here ; immediate \ ( C: -- dest ) begin loop
: again zero, 0jump, ; immediate \ ( C: dest -- ) jump back to beginning
: until 0branch, ! ; immediate \ ( C: dest -- ) branch on 0 to address \ NEW: not in kernel;
: while 0branch, swap ; immediate \ ( C: dest -- orig dest ) continue while condition met (0= if), 
: repeat zero, 0jump, patch, ; immediate \ ( C: orig dest -- ) jump back to beginning, patch while to here

\ -- CONTINUE BOOTSTRAPPING ----------------------------------------------------

\ some of these are no-ops for us, but are standard and should be used for portability
: align ; \ ( -- ) reserve space to align data space pointer (no-op on SM16) \ TODO: implement?;
: aligned ; \ ( addr -- addr ) align address (no-op on SM16) \ TODO: implement?;
: chars ; \ ( x -- n-chars ) size in address units of n-chars (no-op)

: char+ 1+ ; \ ( addr -- addr ) add size of char to address (1+)
: cells 2* ; \ ( x -- n-cells ) size in address units of n-cells (2*)
: cell+ 2 + ; \ ( addr -- addr ) add size of cell to address (2 +)

: 2! tuck ! cell+ ! ; \ ( y x addr -- ) store x y at consecutive addresses (tuck ! cell+ !)
: 2@ dup cell+ @ swap @ ; \ ( addr -- y x ) fetch pair of consecutive addresses (dup cell+ @ swap @)

: abs dup 0< if negate then ; \ ( x -- |x| ) absolute value (dup 0< if negate then)
: j 2r> 2r@ drop -rot 2>r ; \ ( -- x ) ( R: x -- x ) copy next outer loop index (2r> 2r@ drop -rot 2>r) (x r twelve add, x x ld16, x pushd, ret,)

here .
