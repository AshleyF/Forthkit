# Interesting Instruction Sets

## [DCPU-16](https://web.archive.org/web/20120509184912/http://0x10c.com/doc/dcpu-16.txt)

16-bit, 32K words RAM, 8 registers + PC, SP, overflow (O)

* Interesting:
  * Can manually assign to PC, push PC to stack, etc.
  * Can reference registers or memory at registers as values
* Values: R, [R], [PC+R], pop [SP++], peek [SP], push [--SP], SP, PC, O [PC], literals
* Copy (SET a to b)
* ALU: ADD, SUB, MUL, DIV, MOD, SHL, SHR, AND, BOR, XOR
* Conditional: IFE (a=b), IFN (a!=b), IFG (a>b), IFB (a&b!=0)
* Jump: JSR  

## [UM-32](https://esolangs.org/wiki/UM-32)

32-bit, 8 registers

* Conditional: if C,A=B
* Load/Store: A=B[C] / A[B]=C
* ALU: A=B+C / A=B*C / A=B/C / A=B nand C
* HALT
* alloc/dealloc
* I/O: C=getc() / putc(C) 
* JUMP (dup memory and set instruction pointer)
* LIT: A = load immediate (25-bit quantity)

## MyRegVM

16-bit, 16 registers (including PC as register 0)

* Interesting:
  * Can get/set PC, so no JMP/CALL/EXEC needed
  * Single SHIFT instruction with positive (left)/negative (right)
* HALT
* Load/Store: LD, ST
* Conditional: A=B if C
* ALU: ADD, SUB, MUL, DIV, AND, OR, XOR, NOT, SHL, SHR
* I/O: char IN, OUT, disk READ, WRITE