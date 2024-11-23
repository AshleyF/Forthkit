#include <stdio.h>

short reg[16] = {};
unsigned char mem[0x8000];

void readBlock(short block, short maxsize, short address)
{
    char filename[16];
    snprintf(filename, sizeof(filename), "block%d.bin", block);
    FILE *file = fopen(filename, "r");
    fseek(file, 0, SEEK_END);
    long size = ftell(file);
    fseek(file, 0, SEEK_SET);
    if (!file || !fread(mem + address, maxsize < size ? maxsize : size, 1, file)) // assumes size+address <= sizeof(mem)
    {
        printf("Could not open block file.\n");
    }
    fclose(file);
}

void writeBlock(short block, short size, short address)
{
    char filename[16];
    snprintf(filename, sizeof(filename), "block%d.bin", block);
    FILE *file = fopen(filename, "w");
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
    readBlock(0, 0x7FFF, 0);
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
            case 0: // HALT
                return reg[x];
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
            case 9: // NAND
                reg[z] = ~(reg[y] & reg[x]);
                break;
            case 10: // SHL
                reg[z] = reg[y] << reg[x];
                break;
            case 11: // SHR
                reg[z] = reg[y] >> reg[x];
                break;
            case 12: // IN
                reg[0]--;
                reg[x] = getc(stdin);
                break;
            case 13: // OUT
                reg[0]--;
                putc(reg[x], stdout);
                break;
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
    }
}