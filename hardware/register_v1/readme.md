# Register Machine v2

Virtual "hardware" target machine (VM in C).
Build with [`./machine.sh](./machine.sh).

## Instruction Set

It is a register-based machine with 16 register cells and 32K cells of memory, each 16-bit.
The 16 instructions are followed by one to three nybble operands - register indices, memory addresses, ...

| Mnumonic | Op Code |     |     |     | Effect           | Description                  |
| -------- | ------- | --- | --- | --- | ---------------- | ---------------------------- |
| halt     |  0      | x   |     |     |                  | Halt machine with code x     |
| ldc      |  1      | x   | y   | z   | x = yz           | Load constant value (signed) |
| ld+      |  2      | x   | y   | z   | z = [y]  y += x  | Load from memory             |
| st+      |  3      | x   | y   | z   | [z] = y  z += x  | Store to memory              |
| cp?      |  4      | x   | y   | z   | z = y if x       | Conditional copy registers   |
| add      |  5      | x   | y   | z   | z = y + x        | Addition                     |
| sub      |  6      | x   | y   | z   | z = y - x        | Subtraction                  |
| mul      |  7      | x   | y   | z   | z = y × x        | Multiplication               |
| div      |  8      | x   | y   | z   | z = y ÷ x        | Division                     |
| nand     |  9      | x   | y   | z   | z = y nand x     | Nand                         |
| shl      | 10      | x   | y   | z   | z = y << x       | Bitwise shift-left           |
| shr      | 11      | x   | y   | z   | z = y >> x       | Bitwise shift-right          |
| in       | 12      | x   |     |     | x = getc()       | Read from console            |
| out      | 13      | x   |     |     | putc(x)          | Write to console             |
| read     | 14      | x   | y   | z   |                  | File (z) of (y) -> core (x)  |
| write    | 15      | x   | y   | z   |                  | File (z) of (y) <- core (x)  |

The machine loads an `block0.bin` of little-endian encoded memory cells at startup and begins executing at address zero.

## Demo

A demo `block0.bin` may be built (see Assembler section below) which will simply capitalize console input by subtracting 32 from input characters:

| Assembly          | Op    |        |      |      | Encoded             |
| ----------------- | ----- | ------ | ---- | ---- | ------------------- |
| `2 constant two`  |       |        |      |      | (compile time asm)  |
| `12 constant one` |       |        |      |      | (compile time)      |
| `13 constant x`   |       |        |      |      | (compile time)      |
| `14 constant y`   |       |        |      |      | (compile time)      |
| `15 constant z`   |       |        |      |      | (compile time)      |
|   `2 two ldc,`    | `ldc` | `two`  | 0    | 2    | 12 02  (assembler)  |
|   `1 one ldc,`    | `ldc` | `one`  | 0    | 1    | 1C 01               |
|    `32 y ldc, `   | `ldc` | `y`    | 2    | 0    | 1E 20               |
| `label 'loop`     |       |        |      |      | (compile time)      |
|       `x in,`     | `in`  | `x`    |      |      | CD                  |
| `z x one add,`    | `add` | `one`  | `x`  | `z`  | 5CDF                |
| `'loop z jmz,`    | `ld+` | `two`  | `pc` | `t`  | 2203 0600 ('loop)   |
|                   | `cp?` | `z`    | `t`  | `pc` | 4F30                |
|   `x x y sub,`    | `sub` | `y`    | `x`  | `x`  | 6EDD                |
|       `x out`     | `out` | `x`    |      |      | DD                  |
|   `'loop jump,`   | `ld+` | `zero` | `pc` | `pc` | 2100 0600 ('loop)   |

The full `block0.bin` contains the following 22 bytes: `1202 1C01 1E20 CD 5CDF 2203 0600 4F30 6EDD DD 2100 0600`.

After assembling a `block0.bin` (see Assembler section below), we may run the machine and type something (e.g. `hello`):

    $ ./machine
    hello
    HELLO

## Machine Walkthrough

This is a register-based machines, like many popular architectures today (e.g. Intel, AMD, ARM, ...).

```c
short reg[16] = {};
unsigned char mem[0x8000];
```

We have 16 registers, 32K cells of memory. The zeroth register is the program counter (`pc`) pointing to instructions to be executed.

```c
    readBlock(0, SHRT_MAX, 0);
```

When the machine boots an image file populates memory.

```c
        unsigned char c = NEXT;
        unsigned char j = NEXT;
        unsigned char i = HIGH(c);
        unsigned char x = LOW(c);
        unsigned char y = HIGH(j);
        unsigned char z = LOW(j);
```

The instructions reference register numbers. For example `add` needs to know which two registers to sum and the register in which to deposit the result. We'll refer to these as `x`, `y` and `z`. The macros are shorthand for fetching the `NEXT` slot in the instruction stream and getting the `HIGH()` or `LOW()` nybble. The `x`, `y` or `y` register numbers are used.

```c
        switch(i)
        {
            case 0: // ...
            case 1: // ...
            case 2: // ...
            // ...
        }
    }
```

The main loop merely fetches and processes instructions one-by-one.


```c
        switch(i)
        {
            case 0: // HALT
                return reg[x];
            // ...
        }
```

The first instruction is a simple halt (the default when encountering zeroed memory).

```c
        switch(i)
        {
            // ...
            case 1: // LDC
                reg[x] = (signed char)((y << 4) | z);
                break;
            case 2: // LD+
                reg[z] = (mem[reg[y]] | (mem[reg[y] + 1] << 8));
                reg[y] += reg[x];
                break;
            case 3: // ST+
                mem[reg[z]] = reg[y]; // truncated to byte
                mem[reg[z] + 1] = (reg[y] >> 8); // truncated to byte
                reg[z] += reg[x];
                break;
            case 4: // CP?
                if (reg[x] == 0) reg[z] = reg[y];
                break;
            // ...
        }
```

The next several instructions move data around. Instruction `1` loads a constant (`ldc`) from the following byte in the instruction stream into a register. The next two instructions (`2` and `3`) load (`ld+`) from and store (`st+`) to memory by an address taken from the instruction stream and then add a value to the register used as an index. Instruction `4` conditionally copies (`cp?`) one register into another depending on the value of a third register.

Note that there is no jump instruction, but we can build this by copying into the zeroth (program counter) register. And we can use the conditional copy instruction to build various conditional branching instructions. We also don't have a call instruction, but can build our own return address stack and make add this ourselves!

```c
        switch(i)
        {
            // ...
            case 5: // ADD
                reg[z] = reg[y] + reg[x];
                break;
            case 6: // SUB
                reg[z] = reg[y] - reg[x];
                break;
            case 7: // MUL
                reg[z] = reg[y] * reg[x];
                break;
            case 8: // DIV
                reg[z] = reg[y] / reg[x];
                break;
            // ...
        }
```

We have basic arithmetic operations, just as in the Python interpreter. Technically, we could have built subtraction, multiplication and division from just `add` and `shl` (below), which would be a fun exercise, but we have room to include them here for convenience.

```c
        switch(i)
        {
            // ...
            case 9: // NAND
                reg[z] = ~(reg[y] & reg[x]);
                break;
            case 10: // SHL
                reg[z] = reg[y] << reg[x];
                break;
            case 11: // SHR
                reg[z] = reg[y] >> reg[x];
                break;
            // ...
        }
```

The only bit twiddling instructions are `nand`, from which we can build the others (`and`, `or`, `xor`, `not`, ...) and shift left (`shl`) and right (`shr`). Again, some of the bitwise operators can be thought of as logical operators if we use `-1` (all bit set) to represent true and `0` to represent false. Also, `shl` can be thought of as multiplication by 2 and `shr` as division by 2. In fact, some Forths call these `2*` and `2/`. Note that these work on 16-bit signed `short` values and not floating point as in the Python interpreter, which will pose an interesting problem when we port turtle graphics.

```c
        switch(i)
        {
            // ...
            case 12: // IN
                reg[0]--;
                reg[x] = getc(stdin);
                if (feof(stdin)) { clearerr(stdin); }
                break;
            case 13: // OUT
                reg[0]--;
                wprintf(L"%lc", reg[x]);
                fflush(stdout);
                break;
            // ...
        }
```

Standard I/O is supported by `in` and `out` instructions. We use `wprintf(...)` rather than `putc(...)` in order to support emitting Unicode characters.


```c
        switch(i)
        {
            // ...
            case 14: // READ
                readBlock(reg[z], reg[y], reg[x]);
                break;
            case 15: // WRITE
                writeBlock(reg[z], reg[y], reg[x]);
                break;
            default:
                printf("Invalid instruction! (%i)\n", i);
                return 1;
        }
```

The `read` and `write` instructions allow loading and saving block files and will facilitate using this machine to build boot images.

## Assembler

A [Forth-based assembler is provided](./assembler.f), allowing the above program [to be expressed](./test.f) as:

```forth
12 constant one
13 constant x
14 constant y
15 constant z

  1 one ldc,
   32 y ldc,

label 'loop
      x in,
z x one add,
'loop z jmz,
  x x y sub,
      x out,
  'loop jump,

assemble
```

This is a pretty nice assembly format, leaving all the power of Forth available as a "macro assembler."

A new `block0.bin` may be build with [`./test.sh`](./test.sh).

In addition to the `label` mechanism to give names to addresses for backward jumps (most common), there are `ahead,` and `continue,` words to skip over code (likely for library routines).

### Assembler Walkthrough

TODO

Building an assembler in Forth is surprisingly easy.

```forth
variable h
: here h @ ;
:  , here ! here 2 + h ! ;
: c, here c! here 1 + h ! ;
```

We start with a _dictionary pointer_ (`h`, which we'll soon use to pack a dictionary structure). The `here` word merely fetches the pointer. The comma (`,`) word appends a 16-bit value to the dictionary space and increments the pointer, while c-comma (`c,`) appends a single byte.

```forth
: 2nyb, 4 << or c, ;
: 4nyb, 2nyb, 2nyb, ;
```

A couple of helpers are used to pack nybbles.

With just these, we can build words taking instruction operands from the stack and packing into the dictionary.

```forth
: halt,    0 2nyb, ;
: ldc,     1 2nyb, c, ;
: ld+,     2 4nyb, ;
: st+,     3 4nyb, ;
: cp?,     4 4nyb, ;
: add,     5 4nyb, ;
: sub,     6 4nyb, ;
: mul,     7 4nyb, ;
: div,     8 4nyb, ;
: nand,    9 4nyb, ;
: shl,    10 4nyb, ;
: shr,    11 4nyb, ;
: in,     12 2nyb, ;
: out,    13 2nyb, ;
: read,   14 4nyb, ;
: write,  15 4nyb, ;
```

Because the assembler is hosted in Forth, we have all the power of Forth to automate and make helper words for anything we like; make this a _macro assembler_.

```forth
: label here constant ;
: assemble 0 here 0 write halt ;
```

For now, we've added `label` word that creates a constant giving a name to the current address (`here`). This is used, for example, in the test assembly above where we `label 'start` at the beginning of a loop and later `'start jump,`. The `assemble` word writes memory to an image file (and displays the current size of the dictionary).

TODO: talk about the predefined constants and pseudo instructions

```forth
: ahead, here 2 + 0 zero jmz, ; ( dummy jump, push address )
: continue, here swap ! ; ( patch jump )
```

The `label` mechanism works for backward jumps, which may be most common. The `ahead,` and `continue,` words allow us to skip over code. A little tricky, but `ahead,` packs a `jump,` with a dummy (`0`) value and pushes the address of the jump value (`here 1 +`). The `continue,` word is used wherever we want to jump _to_. It patches the jump value to do here (`here swap !`; storing the current `here` at the previously pushed address).

## Interpreter

TODO

### Inner Interpreter Walkthrough

TODO