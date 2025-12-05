# Stack Machine

## Instruction Set

0. `HALT` - Halt execution
1. `ADD` - Addition
2. `SUB` - Subtraction
3. `MUL` - Multiplication
4. `DIV` - Division
5. `NOT` - Bitwise not
6. `AND` - Bitwise and
7. `OR` - Bitwise or
8. `XOR` - Bitwise xor
9. `SHL` - Shift left
10. `SHR` - Shift right
11. `IN` - Input character
12. `OUT` - Output character
13. `READ` - Read block
14. `WRITE` - Write block
15. `LD16+` - Fetch cell at address, and increment over
16. `LD8+` - Fetch byte at address, and increment over
17. `ST16+` - Store cell at address, and increment over
18. `ST8+` - Store byte at address, and increment over
19. `LIT16` - Fetch literal next cell
20. `LIT8` - Fetch literal next byte (signed)
21. `IF` - Jump to address in next cell if T = 0
22. `NEXT` - If R > 0, R-- and loop to next cell address, otherwise drop R and continue
23. `DROP` - Drop top of stack
24. `DUP` - Duplicate top of stack
25. `OVER` - yx -> yxy
26. `SWAP` - yx -> xy
27. `PUSH` - Push top of data stack to return stack
28. `POP` - Pop top of return stack to data stack
29. `PEEK` - Peek top of return stack to data stack
30. `RET` - Return from call
31. `NOP` - No-op

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

## Notes

- `LD` and `ST` do not increment
- Calls must be even-numbered addresses, otherwise no alignment enforced

## Ideas

- Calls with high bit set? (then jumps to zero-filled memory are not calls to 0000)
- LD+ and ST+ instructions leave incremented address
  `swap ld+ rot st+` instead of `over @ over ! 1+ swap 1+ swap`
  - Plain fetch/store becomes: `ld+ nip`/`st+ nip`
- Jump instruction? Otherwise [CALL] followed by [RET NOP NOP]

## Simplifications

- Stack pointers not exposed, so no `(clear-data)` or `(clear-return)`
- No `source-addr`, `source-len`, `source` words