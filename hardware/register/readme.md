# Register Machine

Virtual "hardware" target machine (VM in C).
Build with [`./machine.sh](./machine.sh).

## Instruction Set

It is a register-based machine with 32 register cells and 32K cells of memory, each 16-bit.
Instructions are followed by zero to three operands - register indices, memory addresses, ...

| Mnumonic | Op Code |     |     |     | Effect           | Description                 |
| -------- | ------- | --- | --- | --- | ---------------- | --------------------------- |
| halt     |  0      |     |     |     |                  | Halt machine                |
| ldc      |  1      | x   | v   |     | x = v            | Load constant value         |
| ld       |  2      | x   | a   |     | xx = mem[a]      | Load from memory            |
| st       |  3      | a   | x   |     | mem[a] = xx      | Store to memory             |
| cp       |  4      | x   | y   |     | x = y            | Copy between registers      |
| ldb      |  5      | x   | a   |     | x = mem[a]       | Load byte from memory       |
| stb      |  6      | a   | x   |     | mem[a] = x       | Store byte to memory        |
| in       |  7      | x   |     |     | x = getc()       | Read from console           |
| out      |  8      | x   |     |     | putc(x)          | Write to console            |
| inc      |  9      | x   | y   |     | x = y + 1        | Increment register          |
| dec      | 10      | x   | y   |     | x = y - 1        | Decrement register          |
| add      | 11      | x   | y   | z   | x = y + z        | Addition                    |
| sub      | 12      | x   | y   | z   | x = y - z        | Subtraction                 |
| mul      | 13      | x   | y   | z   | x = y × z        | Multiplication              |
| div      | 14      | x   | y   | z   | x = y ÷ z        | Division                    |
| mod      | 15      | x   | y   | z   | x = y mod z      | Modulus                     |
| and      | 16      | x   | y   | z   | x = y ∧ z        | Logical/bitwise and         |
| or       | 17      | x   | y   | z   | x = y ∨ z        | Logical/bitwise or          |
| xor      | 18      | x   | y   | z   | x = y ⊕ z        | Logical/bitwise xor         |
| not      | 19      | x   | y   |     | x = ¬y           | Logical/bitwise not         |
| shl      | 20      | x   | y   | z   | x = y << z       | Bitwise shift-left          |
| shr      | 21      | x   | y   | z   | x = y >> z       | Bitwise shift-right         |
| beq      | 22      | a   | x   | y   | pc = a if x = y  | Branch if equal             |
| bne      | 23      | a   | x   | y   | pc = a if x ≠ y  | Branch if not equal         |
| bgt      | 24      | a   | x   | y   | pc = a if x > y  | Branch if greater than      |
| bge      | 25      | a   | x   | y   | pc = a if x ≥ y | Branch if greater or equal  |
| blt      | 26      | a   | x   | y   | pc = a if x < y  | Branch if less than         |
| ble      | 27      | a   | x   | y   | pc = a if x ≤ y | Branch if less or equal     |
| exec     | 28      | x   |     |     | pc = [x]         | Jump to address in register |
| jump     | 29      | a   |     |     | pc = a           | Jump to address             |
| call     | 30      | a   |     |     | push(pc), pc = a | Call address, save return   |
| ret      | 31      |     |     |     | pc = pop()       | Return from call            |
| dump     | 32      |     |     |     |                  | Dump core to image.bin      |
| debug    | 33      |     |     |     |                  | Print machine state         |

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
    setlocale(LC_ALL, "");

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
We set the locale so that we can emit Unicode characters.

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
            case  5: X;   wprintf(L"%lc", Rx);  break; // out (putc(x))
            ...
        }
```

Standard I/O is supported by `in` and `out` instructions. We use `wprintf(...)` rather than `putc(...)` in order to support emitting Unicode characters.

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
            case 17: XYZ; Rx = Ry << Rz;        break; // shl (x = y << z)
            case 18: XYZ; Rx = Ry >> Rz;        break; // shr (x = y >> z)
            ...
        }
```

Bit twiddling instructions include `and`, `or`, `xor`, `not` and shift left (`shl`) and right (`shr`). Again, some of the bitwise operators can be thought of as logical operators if we use `-1` (all bit set) to represent true and `0` to represent false. Also, `shl` can be thought of as multiplication by 2 and `shr` as division by 2. In fact, some Forths call these `2*` and `2/`.

```c
        switch(NEXT)
        {
            ...
            case 19: XYZ; if (Ry == Rz) pc = x; break; // beq (branch if x == y)
            case 20: XYZ; if (Ry != Rz) pc = x; break; // bne (branch if x != y)
            case 21: XYZ; if (Ry >  Rz) pc = x; break; // bgt (branch if x > y)
            case 22: XYZ; if (Ry >= Rz) pc = x; break; // bge (branch if x >= y)
            case 23: XYZ; if (Ry <  Rz) pc = x; break; // blt (branch if x < y)
            case 24: XYZ; if (Ry <= Rz) pc = x; break; // ble (branch if x <= y)
        }
```

We have a full suite of conditional branching operators.

```c
        switch(NEXT)
        {
            ...
            case 25: X;   pc = Rx;              break; // exec (pc = x)
            case 26: X;   pc = x;               break; // jump (pc = v)
            case 27: X;   *(r++) = pc; pc = x;  break; // call (jsr(v))
            case 28:      pc = *(--r);          break; // return (ret)
            case 29:      return 0; // halt
            ...
        }
```

We can executle (`exec`) an indirect address through a register, or we can `jump` to a direct address given in the instruction stream. We can also `call` a given address. This places the program counter (`pc`) on a return stack, which is restored when we return (`ret`). Finally, we can explicitly `halt` the machine.

```c
        switch(NEXT)
        {
            ...
            case 30: // dump
                file = fopen("image.bin", "w");
                if (!file || !fwrite(&mem, sizeof(mem), 1, file))
                {
                    printf("Could not write boot image.\n");
                    return 1;
                }
                fclose(file);
                break;
            ...
        }
```

The `dump` instruction will facilitate using this machine to build boot images.

```c
        switch(NEXT)
        {
            ...
            case 31: // debug
                printf("Inst: %i Reg: %04x %04x %04x %04x %04x %04x %04x Stack: %04x %04x %04x %04x %04x %04x %04x %04x Return: %i %i %i %i %i %i %i %i\n", mem[pc], reg[0], reg[1], reg[2], reg[3], reg[4], reg[5], reg[6], mem[32767], mem[32766], mem[32765], mem[32764], mem[32763], mem[32762], mem[32761], mem[32760], mem[32255], mem[32254], mem[32253], mem[32252], mem[32251], mem[32250], mem[32249], mem[32248]);
                break;
            default:
                printf("Invalid instruction! (pc=%i [%i])\n", pc - 1, mem[pc - 1]);
                return 1;
        }
```

To help with debugging, this `debug` instruction will display internal state.

## Assembler

A [Forth-based assembler is provided](./assembler.f), allowing the above program [to be expressed](./test.f) as:

```forth
    0 constant u
    1 constant c

      u 32 ldc,
     label &start
         c in,
     c u c sub,
         c out,
    &start jump,
```

This is a pretty nice assembly format, leaving all the power of Forth available as a "macro assembler."

A new `image.bin` may be build with [`./test.sh`](./test.sh).

In addition to the `label` mechanism to give names to addresses for backward jumps (most common), there are `ahead,` and `continue,` words to skip over code (likely for library routines).

### Assembler Walkthrough

Building an assembler in Forth is surprisingly easy.

```forth
variable dp ( dictionary pointer )
: here dp @ ;
: , here m! here 1 + dp ! ; ( append )
```

We start with a _dictionary pointer_ (so named because we'll soon use this assembler to pack a dictionary structure). The `here` word merely fetches the pointer. The comma (`,`) word appends a value to the dictionary space and increments the pointer.

With just these, we can build words taking instruction operands from the stack and packing into the dictionary.

```forth
: halt,   0 c, ;               (       halt,  →  halt machine       )
: ldc,    1  , c, c, ;         (   x v ldc,   →  x = v              )
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
: dump,  32 c, ;               (       dump,  →  core to image.bin  )
: debug, 33 c, ;               (       debug, →  show machine state )
```

In a few places we do a `swap` to order the arguments in a _natural_ way. For example `z y x sub,` packs a subtraction instruction meaning _x = z - y_ (with _z_ and _y_ swapped), because this resembles the ordering for infix expressions (left minus right).

```forth
: label here constant ;
: ahead, here 1 + 0 jump, ; ( dummy jump, push address )
: continue, here swap m! ; ( patch jump )
```

Because the assembler is hosted in Forth, we have all the power of Forth to automate and make helper words for anything we like; make this a _macro assembler_.

For now, we've added `lable` word that creates a constant giving a name to the current address (`here`). This is used, for example, in the test assembly above where we `label &start` at the beginning of a loop and later `&start jump,`.

The `label` mechanism works for backward jumps, which may be most commont. The `ahead,` and `continue,` words allow us to skip over code. A little tricky, but `ahead,` packs a `jump,` with a dummy (`0`) value and pushes the address of the jump value (`here 1 +`). The `continue,` word is used wherever we want to jump _to_. It patches the jump value to do here (`here swap m!`; storing the current `here` at the previously pushed address).

```forth
: assemble here . dump halt ;
```

Finally, the `assemble` word dumps memory to an image file (and displays the current size of the dictionary).

## Interpreter

A [kerel](./kernel.f) may be assembled with [`./kernel.sh`](./kernel.sh). This reads Forth tokens, compiles dictionary headers and literals, and manages a stack.

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
| `dump`      | Core dump to `image.bin`                                                                                         |
| `(`         | Begin comment, terminated by `)`                                                                                |

These should be enough to bootstrap a new assembler and meta-circular interpreter!

### Inner Interpreter Walkthrough