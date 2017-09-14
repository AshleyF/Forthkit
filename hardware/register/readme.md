# Register Machine

Virtual "hardware" target virtual machine.
It is a register-based machine with 32 registers, each 16-bit.
32K cells of memory is available, each 16-bits.
Instructions are followed by zero or more operands - register indices, memory addresses, ....

| Mnumonic | Op Code |     |     |     | Effect | Description |
| -------- | ------- | --- | --- | --- | ------ | ----------- |
| ldc      | 0       | x   | v   |     | x = v  | Load constant value |

ld 1 x a x = mem[a] Load from memory
st 2 a x mem[a] = x] Store to memory
cp 3 x y x = y Copy between registers
in 4 x x = getc() Read from console
out 5 x putc(x) Write to console
inc 6 x x + + Increment register
dec 7 x x   Decrement register
add 8 x y z x = y + z Addition
sub 9 x y z x = y  z Subtraction
mul 10 x y z x = y  z Multiplication
div 11 x y z x = y  z Division
mod 12 x y z x = y mod z Modulus
and 13 x y z x = y ^ z Logical/bitwise and
or 14 x y z x = y _ z Logical/bitwise or
xor 15 x y z x = y z Logical/bitwise xor
not 16 x y x = :y Logical/bitwise not
shl 17 x y z x = y  z Bitwise shift-left
shr 18 x y z x = y  z Bitwise shift-right
beq 19 a x y pc = a j x = y Branch if equal
bne 20 a x y pc = a j x 6= y Branch if not equal
bgt 21 a x y pc = a j x > y Branch if greater than
 y Branch if greater or equal
blt 23 a x y pc = a j x < y Branch if less than
	ble 24 a x y pc = a j x  y Branch if less or equal
exec 25 x pc = [x] Jump to address in register
jump 26 a pc = a Jump to address
call 27 a push(pc); pc = a Call address, save return
ret 28 pc = pop() Return from call
halt 29 Halt machine
