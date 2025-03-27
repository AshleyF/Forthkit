# Interesting Instruction Sets

## [DCPU-16](https://web.archive.org/web/20120509184912/http://0x10c.com/doc/dcpu-16.txt)

16-bit, 32K words RAM, 8 registers + PC, SP, overflow (O)

- Interesting:
  - Can manually assign to PC, push PC to stack, etc.
  - Can reference registers or memory at registers as values
- Values: R, [R], [PC+R], pop [SP++], peek [SP], push [--SP], SP, PC, O [PC], literals
- Copy (SET a to b)
- ALU: ADD, SUB, MUL, DIV, MOD, SHL, SHR, AND, BOR, XOR
- Conditional: IFE (a=b), IFN (a!=b), IFG (a>b), IFB (a&b!=0)
- Jump: JSR  

## [UM-32](https://esolangs.org/wiki/UM-32)

32-bit, 8 registers

- Conditional: if C,A=B
- Load/Store: A=B[C] / A[B]=C
- ALU: A=B+C / A=B*C / A=B/C / A=B nand C
- HALT
- alloc/dealloc
- I/O: C=getc() / putc(C) 
- JUMP (dup memory and set instruction pointer)
- LIT: A = load immediate (25-bit quantity)

## Nga (RetroForth)

0 nop   5 push  10 ret   15 fetch 20 div   25 zret
1 lit   6 pop   11 eq    16 store 21 and   26 halt
2 dup   7 jump  12 neq   17 add   22 or    27 ienum
3 drop  8 call  13 lt    18 sub   23 xor   28 iquery
4 swap  9 ccall 14 gt    19 mul   24 shift 29 iinvoke

## RM16

16-bit, 16 registers (including PC as register 0)

- Interesting:
  - Can get/set PC, so no JMP/CALL/EXEC needed
  - Single SHIFT instruction with positive (left)/negative (right)
- HALT (with exit code)
- Load constant: LDC
- Load/Store memory: LD+, ST+ (address into register, register to address, and inc/dec)
- Conditional: CP? (A=B if C)
- ALU: ADD, SUB, MUL, DIV
- Bitwise: NAND, SHL, SHR
- I/O: char IN, OUT, disk READ, WRITE

## F18

- Transfer:
  - jump/call/execute/return, next/unext, conditional if/-if
  - `;` (return), `ex` (execute, swap P and R), `(jump)`, `(call)`
  - `next` (loop to address, dec R), `unext` (micronext, loop within I, dec R)
  - `if` (jump if T=0), `-if` (minus-if, jump if T>=0)
- Data:
  - fetch/store via P/A/B (A includes incrementing version)
  - `@p` (fetch-p, inc), `@+` (fetch-plus, via A, inc), `@b` (fetch-b), `@` (fetch, via A)
  - `!p` (store-p, inc), `!+` (store-plus, via A, inc), `!b` (store-b), `!` (store, via A)
- ALU:
  - add/mul (step), bitwise shift/not/and/or, stack, get/set A/B (B is write-only), NOP
  - `+*` (multiply-step), `2*` (two-star), `2/` (two-slash), `-` (not), `+` (plus)
  - `and`, `or` (exclusive)
  - `drop`, `dup`, `over`, `pop` (R), `push` (R)
  - `a`, `b!` (b-store, into B), `a!` (a-store, into A), 
  - `.` (nop)

## SM16

- P register points to memory (16-bit aligned) or port (negative)

| Cell | Instruction | Notes   |
| ---- | ----------- | ------- |
| 0000 | `@`         | fetch   |
| 0001 | `!`         | store   |
| 0010 | `@p`        | fetch-p |
| 0011 | `!p`        | store-p |
| 0100 | `push`      | >r      |
| 0101 | `pop`       | r>      |
| 0110 | `swap`      |         |
| 0111 | `over`      |         |
| 1000 | `drop`      |         |
| 1001 | `dup`       |         |
| 1010 | `next`      |         |
| 1011 | `shift`     |         |
| 1100 | `nand`      |         |
| 1101 | `+`         |         |
| 1110 | `*`         |         |
| 1111 | `.` (nop)   |         |

| Last | Cell | Instruction | Notes |
| ---- | ---- | ----------- | ----- |
|      | 0    | `call`      | s15   |
| 00   | 1    | `jump`      | s13   |
| 01   | 1    | `?jump`     | s13   |
| 10   | 1    | `return`    |       |
| 011  | 1    | `repeat`    |       |
| 111  | 1    | `.`  (nop)  |       |

Examples:

- Load 10 cells into memory and execute
  - @p @p push .
  - 0 (value)
  - 10 (value)
  - @p over ! unext
  - drop . . .
  - 0000000000000001 (0 jump)

- Print alphabet
  - Humm... how to output to a port?!

## Brief VM

- No leading bit is a 16-bit call
  - Call followed by return is tail optimized
- Lower 7-bits is instruction
- Instructions:
  - `push` / `pop` / `peek`
  - `(return)`
  - `c@` / `c!` / `@` / `!` / `+`
  - `-` / `*` / `/` / `mod` / `neg`
  - `and` / `or` / `xor` / `not` / `shift`
  - `=` / `<>` / `>` / `>=` / `<` / `<=`
  - `1+` / `1-`
  - `drop` / `dup` / `swap` / `pick` / `roll` / `clear`
  - `forget`
  - `call`
  - `choice` / `if`
  - `reset`

## SM16 II

- 32K memory
- 16-cell data/return stacks (circular)
- 127 byte codes (high bit set)
- 15-bit calls (followed by return, TCO)
- Protocol: prefix (2-bytes, execute flag+len) n-bytes