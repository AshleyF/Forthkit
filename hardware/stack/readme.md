# Stack Machine

## Instruction Set

\ HALT
\ ADD
\ SUB
\ MUL
\ DIV
\ NAND
\ SHL
\ SHR
\ IN
\ OUT
\ READ
\ WRITE
\ LDC
\ LD+
\ ST+
\ CP?

0. `HALT` - Halt execution
6. `ADD` - Addition
7. `SUB` - Subtraction
8. `MUL` - Multiplication
9. `DIV` - Division
10. `NOT` - Bitwise not
11. `AND` - Bitwise and
12. `OR` - Bitwise or
13. `XOR` - Bitwise xor
14. `SHL` - Shift left
15. `SHR` - Shift right
16. `IN` - Input character
17. `OUT` - Output character
18. `READ` - Read block
19. `WRITE` - Write block
2. `FETCH` - Fetch cell at address
3. `STORE` - Store value at address
1. `LIT` - Fetch next cell
4. `IF` - Jump to address in next cell if T >= 0
5. `-IF` - Jump to address in next cell if T < 0
29. `NEXT` - If R <= 0, drop R and continue, otherwise R-- and loop to address in next cell
20. `DROP` - Drop top of stack
21. `DUP` - Duplicate top of stack
22. `OVER` - yx -> yxy
23. `SWAP` - yx -> xy
24. `NIP` - yx -> X
25. `TUCK` - yx -> xyx
26. `PUSH` - Push top of data stack to return stack
27. `POP` - Pop top of return stack to data stack
28. `PEEK` - Copy top of return stack to data stack
30. `EX` - Swap R <-> PC
31. `RET` - Return from call

Other possibilities: `MOD`, `1+`, `1-`, `ROT`, `-ROT` `UNEXT`, ...

## Execution

Code is aligned on 2-byte cells.

- Fetch cell
- If low bit is not set, call address
    - If next cell is `RET` (or last cell) then jump
- Otherwise, process three 5-bit instructions in high bits
    - cell & 1111100000000000 >> 11
    - cell & 0000011111000000 >> 6
    - cell & 0000000000111110 >> 1
- Program counter points to next cell
    - `LIT`, `IF`, and `-IF` fetch next cell and advances
- 16-element data and return stacks
    - Maybe implemented as T, S + 14 data and R + 15 return