# Register Machine

Virtual "hardware" target machine (VM in C).
Build with [`./machine.sh](./machine.sh).

## Instruction Set

It is a register-based machine with 32 register cells and 32K cells of memory, each 16-bit.
Instructions are followed by zero to three operands - register indices, memory addresses, ...

| Mnumonic | Op Code |     |     |     | Effect           | Description                 |
| -------- | ------- | --- | --- | --- | ---------------- | --------------------------- |
| ldc      | 0       | x   | v   |     | x = v            | Load constant value         |
| ld       | 1       | x   | a   |     | x = mem[a]       | Load from memory            |
| st       | 2       | a   | x   |     | mem[a] = x       | Store to memory             |
| cp       | 3       | x   | y   |     | x = y            | Copy between registers      |
| in       | 4       | x   |     |     | x = getc()       | Read from console           |
| out      | 5       | x   |     |     | putc(x)          | Write to console            |
| inc      | 6       | x   | y   |     | x = y + 1        | Increment register          |
| dec      | 7       | x   | y   |     | x = y - 1        | Decrement register          |
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
| dump     | 30      |     |     |     |                  | Dump core to image.bin      |
| debug    | 31      |     |     |     |                  | Print machine state         |

The machine loads an `image.bin` dump of little-endian encoded memory cells at startup and begins executing at address zero.

## Demo

A demo `image.bin` may be built (see Assembler section below) which will simply capitalize console input by subtracting 32 from input characters:

| Assembly    | Op  |     |     |     | Encoded             |
| ----------- | --- | --- | --- | --- | ------------------- |
| `ldc u 32`  | 0   | 0   | 32  |     | 0000 0000 2000      |
| `in c`      | 4   | 1   |     |     | 0400 0100           |
| `sub c c u` | 9   | 1   | 1   | 0   | 0900 0100 0100 0000 |
| `out c`     | 5   | 1   |     |     | 0500                |
| `jump 0003` | 26  | 3   |     |     | 1a00 0300           |

The encoding could be more compact, but we're more concerned with keeping things simple. The full `image.bin` contains the following bytes (in pairs forming 16-bit `short` memory cells): `0000 0000 2000 0400 0100 0900 0100 0100 0000 0500 0100 1a00 0300`

After assembling a `image.bin` (see Assembler section below), we may run the machine and type something (e.g. `hello`):

    $ ./machine
    hello
    HELLO

## Machine Walkthrough

This is a register-based machines, like many popular architectures today (e.g. Intel, AMD, ARM, ...).

```c
    short reg[32] = {};
    unsigned short mem[0x8000];
    short rstack[256] = {};
    short* r = rstack;
    short pc = 0;
```

We have 32 registers, 16K cells of memory, and a 256 element deep return stack and `r` pointer to support the `call` instruction, and a program counter (`pc`) pointing to instructions to be executed.

```c
    FILE *file = fopen("image.bin", "r");
    if (!file || !fread(&mem, sizeof(mem), 1, file))
    {
        printf("Could not open boot image.\n");
        return 1;
    }
    fclose(file);

```

When the machine boots an image file populates memory.

```c
    short x, y, z;

    #define NEXT mem[pc++]
    #define X x = NEXT;
    #define XY X; y = NEXT;
    #define XYZ XY; z = NEXT;
    #define Rx reg[x]
    #define Ry reg[y]
    #define Rz reg[z]
```

The instructions reference register numbers. For example `add` needs to know which two registers to sum and the register in which to deposit the result. We'll refer to these as `x`, `y` and `z`. The macros are shorthand for fetching the `NEXT` slot in the instruction stream, the `X`, `XY` or `XYZ` register numbers (depending on how many operands the instruction expects), and finally a simple shorthand for fetching register values by index (`Rx`, `Ry` and `Rz`).

```c
    while (1)
    {
        switch(NEXT)
        {
            case  0: ...
            case  1: ...
            case  2: ...
            ...
        }
    }

    return 0;
```

The main loop merely fetches and processes instructions one-by-one.


```c
        switch(NEXT)
        {
            case  0: XY;  Rx = y;               break; // ldc (x = v)
            case  1: XY;  Rx = mem[Ry];         break; // ld (x = m[y])
            case  2: XY;  mem[Rx] = Ry;         break; // st (m[x] = y)
            case  3: XY;  Rx = Ry;              break; // cp (x = y)
            ...
        }
```

The first several instructions move data around. Instruction `0` loads a constant (`ldc`) from the instruction stream into a register (`XY; Rx = y;` expands to `x = mem[pc++]; y = mem[pc++]; reg[x] = y`). The next two instructions load (`ld`) from and store (`st`) to memory by an address taken from the instruction stream. Instruction `3` copies (`cp`) one register into another.

```c
        switch(NEXT)
        {
            ...
            case  4: X;   Rx = getc(stdin);     break; // in (x = getc())
            case  5: X;   putc(Rx, stdout);     break; // out (putc(x))
            ...
        }
```

Standard I/O is supported by `in` and `out` instructions.

```c
        switch(NEXT)
        {
            ...
            case  6: XY;  Rx = Ry + 1;          break; // inc (x = ++y)
            case  7: XY;  Rx = Ry - 1;          break; // dec (x = --y)
            case  8: XYZ; Rx = Ry + Rz;         break; // add (x = y + z)
            case  9: XYZ; Rx = Ry - Rz;         break; // sub (x = y - z)
            case 10: XYZ; Rx = Ry * Rz;         break; // mul (x = y * z)
            case 11: XYZ; Rx = Ry / Rz;         break; // div (x = y / z)
            case 12: XYZ; Rx = Ry % Rz;         break; // mod (x = y % z)
            case 13: XYZ; Rx = Ry << Rz;        break; // shl (x = y << z)
            case 14: XYZ; Rx = Ry >> Rz;        break; // shr (x = y >> z)
            ...
        }
```

We have basic arithmetic operations, just as in the Python interpreter. For convenience we have added `inc` and `dec` as well. The left and right shift instructions (`shl`/`shr`) do bit shifts and may be though of as multiplying/dividing by 2 (in fact, some Forths call these `2*` and `2/`). Note that these work on 16-bit signed `short` values and not floating point as in the Python interpreter, which will pose an interesting problem when we port turtle graphics.

```c
        switch(NEXT)
        {
            ...
            case 13: XYZ; Rx = Ry & Rz;         break; // and (x = y & z)
            case 14: XYZ; Rx = Ry | Rz;         break; // or (x = y | z)
            case 15: XYZ; Rx = Ry ^ Rz;         break; // xor (x = y ^ z)
            case 16: XY;  Rx = ~Ry;             break; // not (x = ~y)
            ...
            case 19: XYZ; if (Ry == Rz) pc = x; break; // beq (branch if x == y)
            case 20: XYZ; if (Ry != Rz) pc = x; break; // bne (branch if x != y)
            case 21: XYZ; if (Ry >  Rz) pc = x; break; // bgt (branch if x > y)
            case 22: XYZ; if (Ry >= Rz) pc = x; break; // bge (branch if x >= y)
            case 23: XYZ; if (Ry <  Rz) pc = x; break; // blt (branch if x < y)
            case 24: XYZ; if (Ry <= Rz) pc = x; break; // ble (branch if x <= y)
            case 25: X;   pc = Rx;              break; // exec (pc = x)
            case 26: X;   pc = x;               break; // jump (pc = v)
            case 27: X;   *(r++) = pc; pc = x;  break; // call (jsr(v))
            case 28:      pc = *(--r);          break; // return (ret)
            case 29:      return 0; // halt
            case 30: // dump
                file = fopen("image.bin", "w");
                if (!file || !fwrite(&mem, sizeof(mem), 1, file))
                {
                    printf("Could not write boot image.\n");
                    return 1;
                }
                fclose(file);
                break;
            case 31: // debug
                printf("Inst: %i Reg: %04x %04x %04x %04x %04x %04x %04x Stack: %04x %04x %04x %04x %04x %04x %04x %04x Return: %i %i %i %i %i %i %i %i\n", mem[pc], reg[0], reg[1], reg[2], reg[3], reg[4], reg[5], reg[6], mem[32767], mem[32766], mem[32765], mem[32764], mem[32763], mem[32762], mem[32761], mem[32760], mem[32255], mem[32254], mem[32253], mem[32252], mem[32251], mem[32250], mem[32249], mem[32248]);
                break;
            default:
                printf("Invalid instruction! (pc=%i [%i])\n", pc - 1, mem[pc - 1]);
                return 1;
        }
```

```c
        switch(NEXT)
        {
            ...
            case  4: X;   Rx = getc(stdin);     break; // in (x = getc())
            case  5: X;   putc(Rx, stdout);     break; // out (putc(x))
            case  6: XY;  Rx = Ry + 1;          break; // inc (x = ++y)
            case  7: XY;  Rx = Ry - 1;          break; // dec (x = --y)
            case  8: XYZ; Rx = Ry + Rz;         break; // add (x = y + z)
            case  9: XYZ; Rx = Ry - Rz;         break; // sub (x = y - z)
            case 10: XYZ; Rx = Ry * Rz;         break; // mul (x = y * z)
            case 11: XYZ; Rx = Ry / Rz;         break; // div (x = y / z)
            case 12: XYZ; Rx = Ry % Rz;         break; // mod (x = y % z)
            case 13: XYZ; Rx = Ry & Rz;         break; // and (x = y & z)
            case 14: XYZ; Rx = Ry | Rz;         break; // or (x = y | z)
            case 15: XYZ; Rx = Ry ^ Rz;         break; // xor (x = y ^ z)
            case 16: XY;  Rx = ~Ry;             break; // not (x = ~y)
            case 17: XYZ; Rx = Ry << Rz;        break; // lsh (x = y << z)
            case 18: XYZ; Rx = Ry >> Rz;        break; // rsh (x = y >> z)
            case 19: XYZ; if (Ry == Rz) pc = x; break; // beq (branch if x == y)
            case 20: XYZ; if (Ry != Rz) pc = x; break; // bne (branch if x != y)
            case 21: XYZ; if (Ry >  Rz) pc = x; break; // bgt (branch if x > y)
            case 22: XYZ; if (Ry >= Rz) pc = x; break; // bge (branch if x >= y)
            case 23: XYZ; if (Ry <  Rz) pc = x; break; // blt (branch if x < y)
            case 24: XYZ; if (Ry <= Rz) pc = x; break; // ble (branch if x <= y)
            case 25: X;   pc = Rx;              break; // exec (pc = x)
            case 26: X;   pc = x;               break; // jump (pc = v)
            case 27: X;   *(r++) = pc; pc = x;  break; // call (jsr(v))
            case 28:      pc = *(--r);          break; // return (ret)
            case 29:      return 0; // halt
            case 30: // dump
                file = fopen("image.bin", "w");
                if (!file || !fwrite(&mem, sizeof(mem), 1, file))
                {
                    printf("Could not write boot image.\n");
                    return 1;
                }
                fclose(file);
                break;
            case 31: // debug
                printf("Inst: %i Reg: %04x %04x %04x %04x %04x %04x %04x Stack: %04x %04x %04x %04x %04x %04x %04x %04x Return: %i %i %i %i %i %i %i %i\n", mem[pc], reg[0], reg[1], reg[2], reg[3], reg[4], reg[5], reg[6], mem[32767], mem[32766], mem[32765], mem[32764], mem[32763], mem[32762], mem[32761], mem[32760], mem[32255], mem[32254], mem[32253], mem[32252], mem[32251], mem[32250], mem[32249], mem[32248]);
                break;
            default:
                printf("Invalid instruction! (pc=%i [%i])\n", pc - 1, mem[pc - 1]);
                return 1;
        }
```

## Assembler

A [Forth-based assembler is provided](./assembler.4th), allowing the above program [to be expressed](./test.4th) as:

    0 const u
    1 const c

      32 u ldc,
     label &start
         c in,
     c u c sub,
         c out,
    &start jump,

This is a pretty nice assembly format, leaving all the power of Forth available as a "macro assembler."

A new `image.bin` may be build with [`./test.sh`](./test.sh).

In addition to the `label` mechanism to give names to addresses for backward jumps (most common), there are `ahead,` and `then,` words to skip over code (likely for library routines).

### Assembler Walkthrough



## Interpreter

An [interpreter](./interpreter.4th) may be assembled with [`./interpreter.sh`](./interpreter.sh). This reads Forth tokens, compiles dictionary headers and literals, and manages a stack.

The dictionary format is as follows. Words are length-suffixed characters followed by a "link" field pointing to the link field of the previous word (or `0` if the first word), followed by an "immediate flag" indicating whether the word should be executed even in compiling mode, followed by machine code (VM bytecode) and presumably a `ret` instruction.

| Name   | Link | Flag | Code      | Name   | Link | Flag | Code      | ... |
| ------ | ---- | ---- | --------- | ------ | ---- | ---- | --------- | --- |
| `foo3` | 0    | -1   | ... `ret` | `bar3` | 0    | -1   | ... `ret` | ... |

In addition to numeric literals support, the following words are currently in the dictionary:

| Word        |                                                                                                                 |
| ----------- | --------------------------------------------------------------------------------------------------------------- |
| `create`    | Reads token and creates word header                                                                             |
| `immediate` | Set immediate flag in latest word header                                                                        |
| `compile`   | Switch to compiling mode (literals packed, words looked up and calls compiled or executed if flagged immediate) |
| `interact`  | Switch to interactive mode (literals pushed to stack and words executed)                                        |
| `;`         | Compile `ret` instruction and exit compiling mode                                                               |
| `pushx`     | Push number in `x` register                                                                                     |
| `popx`      | Pop number in `x` register                                                                                      |
| `,`         | Append TOS to dictionary                                                                                        |
| `literal`   | Compile inline literal from the stack                                                                           |
| `find`      | Find following token in dictionary and push address                                                             |
| `forget`    | Reset dictionary to following token                                                                             |
| `dump`      | Core dump to `image.bin`                                                                                         |
| `(`         | Begin comment, terminated by `)`                                                                                |

These should be enough to bootstrap a new assembler and meta-circular interpreter!

### Inner Interpreter Walkthrough