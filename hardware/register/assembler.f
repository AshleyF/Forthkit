( assembler for register VM )

variable dp ( dictionary pointer )
: here dp @ ;
: , here m! here 1 + dp ! ; ( append )

: halt,   0 , ;            (       halt,  →  halt machine       )
: ldc,    1 , , , ;        (   v x ldc,   →  x = v              )
: ld,     2 , , , ;        (   a x ld,    →  x = mem[a]         )
: st,     3 , , , ;        (   x a st,    →  mem[a] = x         )
: ldb,    4 , , , ;        (   a x ldb,   →  x = mem[a]         )
: stb,    5 , , , ;        (   x a stb,   →  mem[a] = x         )
: cp,     6 , , , ;        (   y x cp,    →  x = y              )
: in,     7 , , ;          (     x in,    →  x = getc           )
: out,    8 , , ;          (     x out,   →  putc x             )
: inc,    9 , , , ;        (   y x inc,   →  x = y + 1          )
: dec,   10 , , , ;        (   y x dec,   →  x = y - 1          )
: add,   11 , , , , ;      ( z y x add,   →  x = z + y          )
: sub,   12 , , swap , , ; ( z y x sub,   →  x = z - y          )
: mul,   13 , , , , ;      ( z y x mul,   →  x = z × y          )
: div,   14 , , swap , , ; ( z y x div,   →  x = z ÷ y          )
: mod,   15 , , swap , , ; ( z y x mod,   →  x = z mod y        )
: and,   16 , , , , ;      ( z y x and,   →  x = z and y        )
: or,    17 , , , , ;      ( z y x or,    →  x = z or  y        )
: xor,   18 , , , , ;      ( z y x xor,   →  x = z xor y        )
: not,   19 , , , ;        (   y x not,   →  x = not y          )
: shl,   20 , , swap , , ; ( z y x shl,   →  x = z << y         )
: shr,   21 , , swap , , ; ( z y x shr,   →  x = z >> y         )
: beq,   22 , , , , ;      ( x y a beq,   →  pc = a if x = y    )
: bne,   23 , , , , ;      ( x y a bne,   →  pc = a if x ≠ y    )
: bgt,   24 , , swap , , ; ( x y a bgt,   →  pc = a if x > y    )
: bge,   25 , , swap , , ; ( x y a bge,   →  pc = a if x ≥ y    )
: blt,   26 , , swap , , ; ( x y a blt,   →  pc = a if x < y    )
: ble,   27 , , swap , , ; ( x y a ble,   →  pc = a if x ≤ y    )
: jump,  28 , , ;          (     a jump,  →  pc = a             )
: call,  29 , , ;          (     a call,  →  push[pc], pc = a   )
: exec,  30 , , ;          (     x exec,  →  pc = [x]           )
: ret,   31 , ;            (       ret,   →  pc = pop[]         )
: dump,  32 , ;            (       dump,  →  core to image.bin  )
: debug, 33 , ;            (       debug, →  show machine state )

: label here constant ;
: ahead, here 1 + 0 jump, ; ( dummy jump, push address )
: continue, here swap m! ; ( patch jump )

: assemble here . dump halt ;
