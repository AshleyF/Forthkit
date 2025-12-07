# Stack Machine

## Instruction Set

0. `HALT` - Halt execution
1. `ADD` - Addition
2. `SUB` - Subtraction
3. `MUL` - Multiplication
4. `DIV` - Division
5. `MOD` - Modulus
6. `NOT` - Bitwise not
7. `AND` - Bitwise and
8. `OR` - Bitwise or
9. `XOR` - Bitwise xor
10. `SHL` - Shift left
11. `SHR` - Shift right
12. `IN` - Input character
13. `OUT` - Output character
14. `READ` - Read block
15. `WRITE` - Write block
16. `LD16+` - Fetch cell at address, and increment over
17. `LD8+` - Fetch byte at address, and increment over
18. `ST16+` - Store cell at address, and increment over
19. `ST8+` - Store byte at address, and increment over
20. `LIT16` - Fetch literal next cell
20. `LIT8` - Fetch literal next signed byte
21. `0JUMP` - Jump to address in next cell if T = 0
22. `NEXT` - If R > 0, R-- and loop to next byte negative offset, otherwise drop R and continue
23. `DROP` - Drop top of stack
24. `DUP` - Duplicate top of stack
25. `OVER` - yx -> yxy
26. `SWAP` - yx -> xy
27. `PUSH` - Push top of data stack to return stack
28. `POP` - Pop top of return stack to data stack
29. `PEEK` - Peek top of return stack to data stack
30. `RET` - Return from call
31. `NOP` - No-op

Other possibilities: `1+`, `1-`, `ROT`, `-ROT` `UNEXT`, ...

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
    - `LIT16`, `LIT8`, `0JUMP` and `NEXT` fetch next cell and advances
- 16-element data and return stacks
    - Maybe implemented as T, S + 14 data and R + 15 return

## Notes

- `LD` and `ST` do not increment
- Calls must be even-numbered addresses, otherwise no alignment enforced
- `literal,` goes away (becomes `lit16,`) [`: literal, x lit16,  x pushd, ;`]
  - Actually, becomes "smart" compiling `lit8` or `lit16`
  - actually not, getting rit of `lit8` because it cause unaligned code

## Ideas

- Calls with high bit set? (then jumps to zero-filled memory are not calls to 0000)
- LD+ and ST+ instructions leave incremented address
  `swap ld+ rot st+` instead of `over @ over ! 1+ swap 1+ swap`
  - Plain fetch/store becomes: `ld+ nip`/`st+ nip`
- Jump instruction? Otherwise [CALL] followed by [RET NOP NOP]
- Inline definitions depending on current slot (use slots that would otherwise be no-ops)
  - 2 instructions: always better
  - 3 instructions: equivalent to call in worst case (maybe faster processing too)
  - 4 instructions: equivalent in last slot, better in second or first slot
  - 5 instructions: equivalent in second slot, better in first
  - 6 instructions: never better than call

## Simplifications

- Stack pointers not exposed, so no `(clear-data)` or `(clear-return)`
- No `source-addr`, `source-len`, `source` words

## TODO

- `find-word` finds the *current* word (no smudge bit, recursion allowed, but not simple redefinition, not classic Forth)