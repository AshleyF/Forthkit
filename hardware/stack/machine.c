#include "../shared/memory.c"

unsigned short dat[0xf];
unsigned short ret[0xf];
unsigned short p = 0, d = 7, r = 7;

#define STACK(s, delta) s = (s + delta) & 15
#define X dat[d]
#define Y dat[(d - 1) & 15]
#define Z dat[(d - 2) & 15]
#define R ret[r]
#define BINOP(op) Y = Y op X; X = 0; /* TODO: remove X = 0 */ STACK(d, -1) 

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
                    case  0:
                        //printf("Test 0: %i\n", (d - 0) & 15);
                        //printf("Test 1: %i\n", (d - 1) & 15);
                        //printf("Test 2: %i\n", (d - 2) & 15);
                        //printf("Test 3: %i\n", (d - 3) & 15);
                        //printf("Test 4: %i\n", (d - 4) & 15);
                        //printf("Test 5: %i\n", (d - 5) & 15);
                        //printf("Test 6: %i\n", (d - 6) & 15);
                        //printf("Test 7: %i\n", (d - 7) & 15);
                        //printf("Test 8: %i\n", (d - 8) & 15);
                        //printf("Test 9: %i\n", (d - 9) & 15);
                        //printf("Test 10: %i\n", (d - 10) & 15);
                        //printf("Test 11: %i\n", (d - 11) & 15);
                        //printf("Test 12: %i\n", (d - 12) & 15);
                        //printf("Test 13: %i\n", (d - 13) & 15);
                        //printf("Test 14: %i\n", (d - 14) & 15);
                        //printf("Test 15: %i\n", (d - 15) & 15);
                        //printf("Test 16: %i\n", (d - 16) & 15);
                        //printf("Test 17: %i\n", (d - 17) & 15);
                        //printf("Test 18: %i\n", (d - 18) & 15);
                        //printf("Test 19: %i\n", (d - 19) & 15);
                        //printf("Test 20: %i\n", (d - 20) & 15);
                        printf("STACK: %i %i %i %i %i %i %i %i %i %i %i %i %i %i %i %i\n", dat[(d - 15) & 15], dat[(d - 14) & 15], dat[(d - 13) & 15], dat[(d - 12) & 15], dat[(d - 11) & 15], dat[(d - 10) & 15], dat[(d - 9) & 15], dat[(d - 8) & 15], dat[(d - 7) & 15], dat[(d - 6) & 15], dat[(d - 5) & 15], dat[(d - 4) & 15], dat[(d - 3) & 15], Z, Y, X);
                        break;
                        //return X; // HALT - Halt execution
                    case  1: BINOP(+); break; // ADD - Addition
                    case  2: BINOP(-); break; // SUB - Subtraction
                    case  3: BINOP(*); break; // MUL - Multiplication
                    case  4: BINOP(/); break; // DIV - Division
                    case  5: X = ~X; break; // NOT - Bitwise not
                    case  6: BINOP(&); break; // AND - Bitwise and
                    case  7: BINOP(|); break; // OR - Bitwise or
                    case  8: BINOP(^); break; // XOR - Bitwise xor
                    case  9: BINOP(<<); break; // SHL - Shift left
                    case 10: BINOP(>>); break; // SHR - Shift right
                    case 11: STACK(d, 1); X = getc(stdin); break; // IN - Input character
                    case 12: putc(X, stdout); STACK(d, -1); break; // OUT - Output character
                    case 13: readBlock(Z, Y, X); STACK(d, -3); break; // READ - Read block
                    case 14: writeBlock(Z, Y, X); STACK(d, -3); break; // WRITE - Write block
                    case 15: STACK(d, 1); X = mem[Y] | (mem[Y + 1] << 8); Y += 2; break; // LD16+ - Fetch cell at address, and increment over
                    case 16: STACK(d, 1); X = mem[Y]; Y++; break; // LD8+ - Fetch byte at address, and increment over
                    case 17: mem[Y] = X & 0xFF; mem[Y + 1] = X >> 8; Y += 2; STACK(d, -1); break; // ST16+ - Store cell at address, and increment over
                    case 18: mem[Y] = X & 0xFF; Y++; STACK(d, -1); break; // ST8+ - Store byte at address, and increment over
                    case 19: STACK(d, 1); X = next(); break; // LIT16 - Fetch literal next cell
                    case 20: STACK(d, 1); X = (signed char)mem[p++]; break; // LIT8 - Fetch literal next signed byte
                    case 21: if (X == 0) { p += (signed char)mem[p]; slot = 15; } else { p++; } STACK(d, -1); break; // 0JUMP - Jump to relative to offset in next byte if T = 0
                    case 22: if (R > 0) { R--; p -= mem[p]; slot = 15; } else { STACK(R, -1); p++; } break; // NEXT - If R > 0, R-- and loop back to next byte negative offset, otherwise drop R and continue
                    case 23: STACK(d, -1); break; // DROP - Drop top of stack
                    case 24: STACK(d, 1); X = Y; break; // DUP - Duplicate top of stack
                    case 25: STACK(d, 1); X = Z; break; // OVER - yx -> yxy
                    case 26: short t = X; X = Y; Y = t; break; // SWAP - yx -> xy
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