// v2
#define _VERBOSE

#include <stdio.h>
#include <wchar.h>
#include <locale.h>

#include <stdio.h>
#include <fcntl.h>
#include <unistd.h>
#include <limits.h>

typedef unsigned char byte;

short reg[16] = {};
byte mem[0x8000];

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
    // printf("WRITE BLOCK %i size=%i addr=%i \n", block, size, address);
    char filename[16];
    snprintf(filename, sizeof(filename), "block%d.bin", block);
    FILE *file = fopen(filename, "w");
    if (!file || !fwrite(mem + address, 1, size, file))
    {
        printf("Could not write block file.\n");
    }
    fclose(file);
}

byte next()
{
    extern short reg[];
    return mem[reg[0]++];
}

byte low(byte b)
{
    return b & 0x0F;
}

byte high(byte b)
{
    return low(b >> 4);
}

int main(void)
{
    // Set stdin to non-blocking mode
    int flags = fcntl(STDIN_FILENO, F_GETFL, 0);
    fcntl(STDIN_FILENO, F_SETFL, flags | O_NONBLOCK);
    setlocale(LC_ALL, "");

    readBlock(0, SHRT_MAX, 0);

#ifdef VERBOSE
    short max = 1000;
    while (--max > 0)
#else
    while (1)
#endif
    {
        byte c = next();
        byte j = next();
        byte i = high(c);
        byte x = low(c);
        byte y = high(j);
        byte z = low(j);
        switch(i)
        {
            case 0: // HALT
#ifdef VERBOSE
                printf("HALT %2i\n", x);
#endif
                return reg[x];
            case 1: // LDC
#ifdef VERBOSE
                printf("LDC %2i=%2i                      ", x, ((y << 4) | z));
#endif
                reg[x] = (signed char)((y << 4) | z);
                break;
            case 2: // LD+
#ifdef VERBOSE
                printf("LD+ %2i=[%2i] +%2i               ", z, y, x);
#endif
                reg[z] = (mem[reg[y]] | (mem[reg[y] + 1] << 8));
                reg[y] += reg[x];
                break;
            case 3: // ST+
#ifdef VERBOSE
                printf("ST+ [%2i]=%2i +%2i               ", z, y, x);
#endif
                mem[reg[z]] = reg[y]; // truncated to byte
                mem[reg[z] + 1] = (reg[y] >> 8); // truncated to byte
                reg[z] += reg[x];
                break;
            case 4: // CP?
#ifdef VERBOSE
                printf("CP? %2i=%2i if %2i=0             ", z, y, x);
#endif
                if (reg[x] == 0)
                {
                    reg[z] = reg[y];
                }
                break;
            case 5: // ADD
#ifdef VERBOSE
                printf("ADD %2i=%2i+%2i                 ", z, y, x);
#endif
                reg[z] = reg[y] + reg[x];
                break;
            case 6: // SUB
#ifdef VERBOSE
                printf("SUB %2i=%2i-%2i                 ", z, y, x);
#endif
                reg[z] = reg[y] - reg[x];
                break;
            case 7: // MUL
#ifdef VERBOSE
                printf("MUL %2i=%2i*%2i                 ", z, y, x);
#endif
                reg[z] = reg[y] * reg[x];
                break;
            case 8: // DIV
#ifdef VERBOSE
                printf("DIV %2i=%2i/%2i                 ", z, y, x);
#endif
                reg[z] = reg[y] / reg[x];
                break;
            case 9: // NAND
#ifdef VERBOSE
                printf("NAND %2i=%2i&%2i                ", z, y, x);
#endif
                reg[z] = ~(reg[y] & reg[x]);
                break;
            case 10: // SHL
#ifdef VERBOSE
                printf("SHL %2i=%2i<<%2i                ", z, y, x);
#endif
                reg[z] = reg[y] << reg[x];
                break;
            case 11: // SHR
#ifdef VERBOSE
                printf("SHR %2i=%2i>>%2i                ", z, y, x);
#endif
                reg[z] = reg[y] >> reg[x];
                break;
            case 12: // IN
#ifdef VERBOSE
                printf("IN ->%2i                        ", x);
#endif
                reg[0]--;
                reg[x] = getc(stdin);
                if (feof(stdin)) { clearerr(stdin); }
                break;
            case 13: // OUT
#ifdef VERBOSE
                printf("OUT %2i->                       ", x);
#endif
                reg[0]--;
                wprintf(L"%lc", reg[x]);
                fflush(stdout);
                break;
            case 14: // READ
#ifdef VERBOSE
                printf("READ ->%2i (%2i, %2i)          ", z, y, x);
#endif
                readBlock(reg[z], reg[y], reg[x]);
                break;
            case 15: // WRITE
#ifdef VERBOSE
                printf("WRITE %2i-> (%2i, %2i)         ", z, y, x);
#endif
                writeBlock(reg[z], reg[y], reg[x]);
                break;
            default:
                printf("Invalid instruction! (%i)\n", i);
                return 1;
        }
#ifdef VERBOSE
        printf("pc:%i 1:%i 2:%i 3:%i 4:%i 5:%i 6:%i 7:%i 8:%i 9:%i 10:%i 11:%i 12:%i 13:%i 14:%i 15:%i m100:%2i m101:%2i m102:%2i\n", reg[0], reg[1], reg[2], reg[3], reg[4], reg[5], reg[6], reg[7], reg[8], reg[9], reg[10], reg[11], reg[12], reg[13], reg[14], reg[15], mem[100], mem[101], mem[102]);
#endif
    }

    return 0;
}
