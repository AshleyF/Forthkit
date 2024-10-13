( assembler for register VM )

variable dp ( dictionary pointer )
: here dp @ ;
:  , here m! here 2 + dp ! ; ( append )
: c, here b! here 1 + dp ! ; ( append byte )

: halt,   0 c, ;               (       halt,  →  halt machine       )
: ldc,    1 c,  , c, ;         (   x v ldc,   →  x = v              )
: ld,     2 c, c, c, ;         (   a x ld,    →  x = mem[a]         )
: st,     3 c, c, c, ;         (   x a st,    →  mem[a] = x         )
: ldb,    4 c, c, c, ;         (   a x ldb,   →  x = mem[a]         )
: stb,    5 c, c, c, ;         (   x a stb,   →  mem[a] = x         )
: cp,     6 c, c, c, ;         (   y x cp,    →  x = y              )
: in,     7 c, c, ;            (     x in,    →  x = getc           )
: out,    8 c, c, ;            (     x out,   →  putc x             )
: inc,    9 c, c, c, ;         (   y x inc,   →  x = y + 1          )
: dec,   10 c, c, c, ;         (   y x dec,   →  x = y - 1          )
: add,   11 c, c, c, c, ;      ( z y x add,   →  x = z + y          )
: sub,   12 c, c, swap c, c, ; ( z y x sub,   →  x = z - y          )
: mul,   13 c, c, c, c, ;      ( z y x mul,   →  x = z × y          )
: div,   14 c, c, swap c, c, ; ( z y x div,   →  x = z ÷ y          )
: mod,   15 c, c, swap c, c, ; ( z y x mod,   →  x = z mod y        )
: and,   16 c, c, c, c, ;      ( z y x and,   →  x = z and y        )
: or,    17 c, c, c, c, ;      ( z y x or,    →  x = z or  y        )
: xor,   18 c, c, c, c, ;      ( z y x xor,   →  x = z xor y        )
: not,   19 c, c, c, ;         (   y x not,   →  x = not y          )
: shl,   20 c, c, swap c, c, ; ( z y x shl,   →  x = z << y         )
: shr,   21 c, c, swap c, c, ; ( z y x shr,   →  x = z >> y         )
: beq,   22 c,  , c, c, ;      ( x y a beq,   →  pc = a if x = y    )
: bne,   23 c,  , c, c, ;      ( x y a bne,   →  pc = a if x ≠ y    )
: bgt,   24 c,  , swap c, c, ; ( x y a bgt,   →  pc = a if x > y    )
: bge,   25 c,  , swap c, c, ; ( x y a bge,   →  pc = a if x ≥ y    )
: blt,   26 c,  , swap c, c, ; ( x y a blt,   →  pc = a if x < y    )
: ble,   27 c,  , swap c, c, ; ( x y a ble,   →  pc = a if x ≤ y    )
: jump,  28 c,  , ;            (     a jump,  →  pc = a             )
: call,  29 c,  , ;            (     a call,  →  push[pc], pc = a   )
: exec,  30 c, c, ;            (     x exec,  →  pc = [x]           )
: ret,   31 c, ;               (       ret,   →  pc = pop[]         )
: read,  32 c, c, c, c, ;      (       read,  →  block file to core )
: write, 33 c, c, c, c, ;      (      write,  →  core to block file )

: label here constant ;
: ahead, here 1 + 0 jump, ; ( dummy jump, push address )
: continue, here swap m! ; ( patch jump )

: assemble 0 here 0 write halt ;
