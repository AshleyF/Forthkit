# Register Machine

Virtual "hardware" target machine.
Build with [`sh ./build.sh](./build.sh).

## Instruction Set

It is a register-based machine with 32 register cell and 32K cells of memory, each 16-bit.
Instructions are followed by zero to three operands - register indices, memory addresses, ...

| Mnumonic | Op Code |     |     |     | Effect           | Description                 |
| -------- | ------- | --- | --- | --- | ---------------- | --------------------------- |
| ldc      | 0       | x   | v   |     | x = v            | Load constant value         |
| ld       | 1       | x   | a   |     | x = mem[a]       | Load from memory            |
| st       | 2       | a   | x   |     | mem[a] = x       | Store to memory             |
| cp       | 3       | x   | y   |     | x = y            | Copy between registers      |
| in       | 4       | x   |     |     | x = getc()       | Read from console           |
| out      | 5       | x   |     |     | putc(x)          | Write to console            |
| inc      | 6       | x   |     |     | x++              | Increment register          |
| dec      | 7       | x   |     |     | x--              | Decrement register          |
| add      | 8       | x   | y   | z   | x = y + z        | Addition                    |
| sub      | 9       | x   | y   | z   | x = y - z        | Subtraction                 |
| mul      | 10      | x   | y   | z   | x = y × z        | Multiplication              |
| div      | 11      | x   | y   | z   | x = y ÷ z        | Division                    |
| mod      | 12      | x   | y   | z   | x = y mod z      | Modulus                     |
| and      | 13      | x   | y   | z   | x = y ∧ z        | Logical/bitwise and         |
| or       | 14      | x   | y   | z   | x = y ∨ z        | Logical/bitwise or          |
| xor      | 15      | x   | y   | z   | x = y ⊕ z        | Logical/bitwise xor         |
| not      | 16      | x   | y   |     | x = ¬y           | Logical/bitwise not         |
| shl      | 17      | x   | y   | z   | x = y << z       | Bitwise shift-left          |
| shr      | 18      | x   | y   | z   | x = y >> z       | Bitwise shift-right         |
| beq      | 19      | a   | x   | y   | pc = a if x = y  | Branch if equal             |
| bne      | 20      | a   | x   | y   | pc = a if x ≠ y  | Branch if not equal         |
| bgt      | 21      | a   | x   | y   | pc = a if x > y  | Branch if greater than      |
| bge      | 22      | a   | x   | y   | pc = a if x ≥ y | Branch if greater or equal  |
| blt      | 23      | a   | x   | y   | pc = a if x < y  | Branch if less than         |
| ble      | 24      | a   | x   | y   | pc = a if x ≤ y | Branch if less or equal     |
| exec     | 25      | x   |     |     | pc = [x]         | Jump to address in register |
| jump     | 26      | a   |     |     | pc = a           | Jump to address             |
| call     | 27      | a   |     |     | push(pc), pc = a | Call address, save return   |
| ret      | 28      |     |     |     | pc = pop()       | Return from call            |
| halt     | 29      |     |     |     |                  | Halt machine                |

The machine loads a [`boot.bin`](./boot.bin) image of little-endian encoded memory cells at startup and begins executing at address zero.

## Demo

A demo hand-made [`boot.bin`](./boot.bin) is provided which will simply capitalize console input by subtracting 32 from input characters:

| Assembly    |     |     |     |     |
| ----------- | --- | --- | --- | --- |
| `ldc u 32`  | 0   | 0   | 32  |     |
| `in c`      | 4   | 1   |     |     |
| `sub c c u` | 9   | 1   | 1   | 0   |
| `out c`     | 5   | 1   |     |     |
| `jump 0003` | 26  | 3   |     |     |

Encoded: `0000 0000 2000 0400 0100 0900 0100 0100 0000 0500 0100 1a00 0300`

Running the machine and typing `hello`:

    $ ./machine.exe
    hello
    HELLO

## Assembler

A [Forth-based assembler is provided](./assembler.4th), allowing the above program [to be expressed](./capitalize.4th) as:

    0 const u
    1 const c

    label &start

      32 u ldc,
         c in,
     c u c sub,
         c out,
    &start jump,

This is a pretty nice (though reversed) assembly format, leaving all the power of Forth available as a powerful "macro assembler."
A new [`boot.bin`](./boot.bin) may be build with [`sh ./capitalize.sh`](./capitalize.sh).

In addition to `label` to give names to addresses for backward jumps (most common), there are `leap` and `ahead` words to skip over code (likely for library routines).
