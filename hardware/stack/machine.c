#include "../shared/memory.c"

unsigned short dat[0x8];
unsigned short ret[0x8];
unsigned short p = 0, s = 0, d = 7, r = 7;

#define STACK(s, delta) s = (s + delta) & 7
#define X dat[d]
#define Y dat[(d - 1) & 7]
#define Z dat[(d - 2) & 7]
#define R ret[r]
#define BINOP(op) Y = Y op X; STACK(d, -1)

unsigned short next() {
    unsigned short h = mem[p++];
    unsigned short l = mem[p++];
    return l << 8 | h;
}

int main(void) {
    readBlock(0, 0, sizeof(mem));
    //int count = 0; while (count++ < 1000) {
    while (1) {
        unsigned short c = next();
        if ((c & 1) == 0) { // call?
            STACK(r, 1);
            R = p;
            p = c;
            //printf("%x CALL: %x\n", R - 2, p);
            // TODO: TCO
        } else { // instructions
            for (short slot = 0; slot < 15; slot += 5) {
                short i = (c >> (11 - slot)) & 0x1F;
                //printf("%x[%x] INSTRUCTION: %i\n", p - 2, slot / 5, i);
                switch (i) {
                    case  0: return dat[d]; // HALT - Halt execution
                    case  1: BINOP(+); break; // ADD - Addition
                    case  2: BINOP(-); break; // SUB - Subtraction
                    case  3: BINOP(*); break; // MUL - Multiplication
                    case  4: BINOP(/); break; // DIV - Division
                    case  5: BINOP(%); break; // MOD - Modulus
                    case  6: X = ~X; break; // NOT - Bitwise not
                    case  7: BINOP(&); break; // AND - Bitwise and
                    case  8: BINOP(|); break; // OR - Bitwise or
                    case  9: BINOP(^); break; // XOR - Bitwise xor
                    case 10: BINOP(<<); break; // SHL - Shift left
                    case 11: BINOP(>>); break; // SHR - Shift right
                    case 12: STACK(d, 1); X = getc(stdin); break; // IN - Input character
                    case 13: putc(X, stdout); STACK(d, -1); break; // OUT - Output character
                    case 14: readBlock(Z, Y, X); STACK(d, -3); break; // READ - Read block
                    case 15: writeBlock(Z, Y, X); STACK(d, -3); break; // WRITE - Write block
                    case 16: STACK(d, 1); X = mem[Y] | (mem[Y + 1] << 8); Y += 2; break; // LD16+ - Fetch cell at address, and increment over
                    case 17: STACK(d, 1); X = mem[Y]; Y++; break; // LD8+ - Fetch byte at address, and increment over
                    case 19: mem[Y] = X & 0xFF; mem[Y + 1] = X >> 8; STACK(d, -1); X += 2; break; // ST16+ - Store cell at address, and increment over
                    case 18: mem[Y] = X & 0xFF; STACK(d, -1); X++; break; // ST8+ - Store byte at address, and increment over
                    case 20: STACK(d, 1); X = next(); break; // LIT16 - Fetch literal next cell
                    case 21: unsigned short t = next(); if (X == 0) { p = t; slot = 15; } break; // 0JUMP - Jump to address in next cell if T >= 0
                    case 22: if (R > 0) { R--; p = next(); slot = 15; } else { STACK(R, -1); p += 2; } break; // NEXT - If R > 0, R-- and loop to next cell address, otherwise drop R and continue
                    case 23: STACK(d, -1); break; // DROP - Drop top of stack
                    case 24: STACK(d, 1); X = Y; break; // DUP - Duplicate top of stack
                    case 25: STACK(d, 1); X = Z; break; // OVER - yx -> yxy
                    case 26: t = X; X = Y; Y = t; break; // SWAP - yx -> xy
                    case 27: STACK(r, 1); R = X; STACK(d, -1); break; // PUSH - Push top of data stack to return stack
                    case 28: STACK(d, 1); X = R; STACK(r, -1); break; // POP - Pop top of return stack to data stack
                    case 29: STACK(d, 1); X = R; break; // PEEK - Copy top of return stack to data stack
                    case 30: p = R; STACK(r, -1); slot = 15; break; // RET - Return from call
                    case 31: break; // NOP
                }
            }
        }
    }
}