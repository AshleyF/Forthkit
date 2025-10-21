# Register Virtual Machine (Minimal)

This is a minimal version of the register-based virtual machine with just the core components needed to start fresh development.

## Files

- `machine.c` - The C implementation of the 16-bit register-based virtual machine
- `machine.fs` - The Forth implementation of the virtual machine (for development/testing)
- `memory.fs` - VM memory system and block I/O (used by machine.fs)
- `assembler.fs` - Assembly tools and compiler (depends on memory.fs)
- `machine` - Pre-compiled executable (ready to use)
- `build.sh` - Build script  
- `readme.md` - This documentation

## Building

Build from source:
```bash
./build.sh
```

This compiles `machine.c` to `machine` executable. Any gcc errors will be displayed.

## Running

### Option 1: Run compiled C VM
The machine loads and executes bytecode from `block0.bin`:
```bash
./machine
```

If no `block0.bin` exists, it will show an error or halt immediately.

### Option 2: Run Forth VM interactively  
For development and testing:
```bash
gforth machine.fs
```

This loads the VM in Forth where you can:
- Single-step through instructions with `step`
- Examine registers and memory
- Test VM operations interactively

## VM Architecture

This is a 16-bit register-based virtual machine with:
- **16 registers** (R0-R15, with R0 as program counter)
- **64KB memory space** (addresses 0x0000 to 0xFFFF)
- **16 instruction opcodes** (complete but minimal instruction set)
- **Little-endian** byte order
- **Block-based storage** system (block0.bin, block1.bin, etc.)

### Instruction Set
0. `HALT` - Halt execution
1. `LDC` - Load constant
2. `LD+` - Load with increment  
3. `ST+` - Store with increment
4. `CP?` - Conditional copy
5. `ADD` - Addition
6. `SUB` - Subtraction
7. `MUL` - Multiplication
8. `DIV` - Division
9. `NAND` - Bitwise NAND
10. `SHL` - Shift left
11. `SHR` - Shift right
12. `IN` - Input character
13. `OUT` - Output character
14. `READ` - Read block
15. `WRITE` - Write block

## Starting Fresh

This minimal system provides just the virtual machine foundation. From here you can build:

- **Assemblers** for generating bytecode
- **Compilers** for high-level languages  
- **Operating systems** and kernels
- **Applications** and games
- **Development tools** and debuggers

## Examples and Complete System

See the complete system in `../register_v2/` for:
- Full Forth kernel and compiler
- Assembler tools  
- Graphics libraries (pixels, turtle graphics)
- Example programs and tests
- Bootstrap process documentation

The book.md in the repository root contains the complete tutorial for building from this minimal VM up to a full Forth system with graphics capabilities.

# Bugs

- Seem to need to press Enter before interacting
- Forth-based machine doesn't echo as keys entered and doesn't allow editing

