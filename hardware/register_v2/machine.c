#include <stdio.h>

unsigned short reg[0x10];
unsigned char mem[0x10000];

FILE* openBlock(unsigned short block, const char * mode)
{
    char filename[0xf];
    snprintf(filename, sizeof(filename), "block%d.bin", block);
    return fopen(filename, mode);
}

void readBlock(unsigned short block, unsigned short address, long maxsize)
{
    FILE *file = openBlock(block, "r");
    fseek(file, 0, SEEK_END);
    long size = ftell(file);
    fseek(file, 0, SEEK_SET);
    if (!file || !fread(mem + address, maxsize < size ? maxsize : size, 1, file)) // assumes size+address <= sizeof(mem)
    {
        printf("Could not open block file.\n");
    }
    fclose(file);
}

void writeBlock(unsigned short block, unsigned short address, long size)
{
    FILE *file = openBlock(block, "w");
    if (!file || !fwrite(mem + address, 1, size, file))
    {
        printf("Could not write block file.\n");
    }
    fclose(file);
}

#define NEXT mem[reg[0]++]
#define LOW(b) b & 0x0F
#define HIGH(b) LOW(b >> 4);

int main(void)
{
    readBlock(0, 0, sizeof(mem));
    while (1)
    {
        unsigned char c = NEXT;
        unsigned char i = HIGH(c);
        unsigned char x = LOW(c);
        
        // instructions that need a second byte (all except HALT, LDC, IN, OUT)
        if (i >= 2 && i <= 15 && i != 12 && i != 13) {
            unsigned char j = NEXT;
            unsigned char y = HIGH(j);
            unsigned char z = LOW(j);
            
            switch(i)
            {
                case 2:
                    reg[z] = mem[reg[y]] | (mem[reg[y] + 1] << 8); reg[y] += reg[x];
                    if (z == 0 && y == 0 && x == 1) // jump?
                    {
                        //printf("JUMP: %i\n", reg[z]);
                    }
                    break; // LD+
                case 3: 
                    mem[reg[y]] = reg[z] & 0xFF;
                    mem[reg[y] + 1] = reg[z] >> 8;
                    reg[y] += reg[x]; 
                    break; // ST+
                case 4: if (reg[x] == 0) reg[z] = reg[y]; break; // CP?
                case 5: reg[z] = (unsigned short)((int)(short)reg[y] + (int)(short)reg[x]); break; // ADD
                case 6: reg[z] = (unsigned short)((int)(short)reg[y] - (int)(short)reg[x]); break; // SUB
                case 7: reg[z] = (unsigned short)((int)(short)reg[y] * (int)(short)reg[x]); break; // MUL
                case 8: reg[z] = (unsigned short)((int)(short)reg[y] / (int)(short)reg[x]); break; // DIV
                case 9: reg[z] = ~(reg[y] & reg[x]); break; // NAND
                case 10: reg[z] = reg[y] << reg[x]; break; // SHL
                case 11: reg[z] = (unsigned short)reg[y] >> reg[x]; break; // SHR (logical)
                case 14: readBlock(reg[z], reg[y], reg[x]); break; // READ
                case 15: writeBlock(reg[z], reg[y], reg[x]); break; // WRITE
                default: printf("Invalid instruction! (%i)\n", i); return 1;
            }
        }
        else {
            // instructions that only use one byte
            switch(i)
            {
                case 0: return reg[x]; // HALT
                case 1: {
                    signed char v = NEXT;  // fetch signed byte and sign-extend
                    reg[x] = v;
                    break; // LDC
                }
                case 12: reg[x] = getc(stdin); break; // IN
                case 13: putc(reg[x], stdout); break; // OUT
                default: printf("Invalid instruction! (%i)\n", i); return 1;
            }
        }
    }
}