#include <stdio.h>
#include <stdio.h>

unsigned short reg[0xf] = {};
unsigned char mem[0x10000];

FILE* openBlock(unsigned short block, const char * mode)
{
    char filename[0xf];
    snprintf(filename, sizeof(filename), "block%d.bin", block);
    return fopen(filename, mode);
}

void readBlock(unsigned short block, long maxsize, unsigned short address)
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

void writeBlock(unsigned short block, long size, unsigned short address)
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
    readBlock(0, sizeof(mem), 0);
    while (1)
    {
        unsigned char c = NEXT;
        unsigned char j = NEXT;
        unsigned char i = HIGH(c);
        unsigned char x = LOW(c);
        unsigned char y = HIGH(j);
        unsigned char z = LOW(j);
        switch(i)
        {
            case 0: return reg[x]; // HALT
            case 1: reg[x] = (signed char)((y << 4) | z); break; // LDC
            case 2: reg[z] = (mem[reg[y]] | (mem[reg[y] + 1] << 8)); reg[y] += reg[x]; break; // LD+
            case 3: mem[reg[y]] = reg[z]; mem[reg[y] + 1] = (reg[z] >> 8); reg[y] += reg[x]; break; // ST+
            case 4: if (reg[x] == 0) reg[z] = reg[y]; break; // CP?
            case 5: reg[z] = reg[y] + reg[x]; break; // ADD
            case 6: reg[z] = reg[y] - reg[x]; break; // SUB
            case 7: reg[z] = reg[y] * reg[x]; break; // MUL
            case 8: reg[z] = reg[y] / reg[x]; break; // DIV
            case 9: reg[z] = ~(reg[y] & reg[x]); break; // NAND
            case 10: reg[z] = reg[y] << reg[x]; break; // SHL
            case 11: reg[z] = (unsigned short)reg[y] >> reg[x]; break; // SHR
            case 12: reg[0]--; reg[x] = getc(stdin); break; // IN
            case 13: reg[0]--; putc(reg[x], stdout); break; // OUT
            case 14: readBlock(reg[z], reg[y], reg[x]); break; // READ
            case 15: writeBlock(reg[z], reg[y], reg[x]); break; // WRITE
            default: printf("Invalid instruction! (%i)\n", i); return 1;
        }
    }
}