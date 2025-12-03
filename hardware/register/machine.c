#include "../shared/memory.c"

unsigned short reg[0x10];

#define NEXT mem[reg[0]++]
#define LOW(b) b & 0x0F
#define HIGH(b) LOW(b >> 4)

int main(void) {
    readBlock(0, 0, sizeof(mem));
    while (1) {
        unsigned char c = NEXT;
        unsigned char i = HIGH(c);
        unsigned char x = LOW(c);
        switch(i) {
            case 0: return reg[x]; // HALT
            case 8: reg[x] = getc(stdin); break; // IN
            case 9: putc(reg[x], stdout); break; // OUT
            case 14: reg[x] = (signed char)NEXT; break; // LIT8
            default: // instructions that need a second byte
                unsigned char j = NEXT;
                unsigned char y = HIGH(j);
                unsigned char z = LOW(j);
                switch(i) {
                    case 1: reg[z] = reg[y] + reg[x]; break; // ADD
                    case 2: reg[z] = reg[y] - reg[x]; break; // SUB
                    case 3: reg[z] = reg[y] * reg[x]; break; // MUL
                    case 4: reg[z] = reg[y] / reg[x]; break; // DIV
                    case 5: reg[z] = ~(reg[y] & reg[x]); break; // NAND
                    case 6: reg[z] = reg[y] << reg[x]; break; // SHL
                    case 7: reg[z] = (unsigned short)reg[y] >> reg[x]; break; // SHR (logical)
                    case 10: readBlock(reg[z], reg[y], reg[x]); break; // READ
                    case 11: writeBlock(reg[z], reg[y], reg[x]); break; // WRITE
                    case 12: reg[z] = mem[reg[y]] | (mem[reg[y] + 1] << 8); reg[y] += reg[x]; break; // LD16+
                    case 13: // ST16+
                        mem[reg[y]] = reg[z] & 0xFF;
                        mem[reg[y] + 1] = reg[z] >> 8;
                        reg[y] += reg[x]; 
                        break;
                    case 15: if (reg[x] == 0) reg[z] = reg[y]; break; // CP?
                    default: printf("Invalid instruction! (%i)\n", i); return 1;
                }
        }
    }
}