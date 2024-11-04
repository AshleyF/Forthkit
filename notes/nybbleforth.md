# [nybbleForth](https://github.com/larsbrinkhoff/nybbleForth)

Very minimalist defintions

: >mark   here ;
: >resolve   here over - swap 1- c! ;

: begin,   0insn here ;
: again,   0 lit, 0branch, ;
: ahead,   0 again, >mark ;
: then,   0insn >resolve ;

: if,   0 0branch, >mark 0insn ;
: until,   0branch, ;

: else,   ahead, swap then, ;
: while,   swap if, ;
: repeat,   again, then, ;

: drop    if then ; / weird!
: 2drop   + drop ; / weird!

variable  temp
: swap   >r temp ! r> temp @ ;
: over   >r temp ! temp @ r> temp @ ;
: rot    >r swap r> swap ;

: r@   r> temp ! temp @ >r temp @ ;
: 2>r   r> swap rot >r >r >r ;
: 2r>   r> r> r> rot >r swap ;

: dup    temp ! temp @ temp @ ;
: 2dup   over over ;
: ?dup   temp ! temp @ if temp @ temp @ then ;

: nip    >r temp ! r> ;

: invert   -1 nand ;
: negate   invert 1 + ;
: -        negate + ;

: 1+   1 + ;
: 1-   -1 + ;
: +!   dup >r @ + r> ! ;
: 0=   if 0 else -1 then ;
: =    - 0= ;
: <>   = 0= ;

: execute   >r ;

: 0<   [ 1 cell 8 * 1 - lshift ] literal nand invert if -1 else 0 then ;
: or   invert swap invert nand ;
: xor   2dup nand 1+ dup + + + ;
: and   nand invert ;
: 2*    dup + ;

: <   2dup xor 0< if drop 0< else - 0< then ;
: u<   2dup xor 0< if nip 0< else - 0< then ;
: >   swap < ;
: u>   swap u> ;

: c@   @ 255 and ;
: c!   dup >r @ 65280 and + r> ! ;

create rstack  rsize allot
variable rp
: @rp   rp @ ;
: 0rp   rstack rsize + rp ! ;