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
: 2drop drop drop ;

: dup [ dup, ] ;
: over [ over, ] ;

: >r [ push, ] ;
: r> [ pop, ] ;
: r@ [ peek, ] ;

: immediate latest @ 2 + dup @ 128 or swap ! ;

: \ 10 parse 2drop ; immediate

\ now we can use comments like this

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

here .
