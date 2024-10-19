// v2

#include <stdio.h>
#include <wchar.h>
#include <locale.h>

#include <stdio.h>
#include <fcntl.h>
#include <unistd.h>
#include <limits.h>

typedef unsigned char byte;

short reg[8] = {};
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

short getValue(byte r)
{
    extern short reg[];
    if ((r & 0x8) == 0)
    {
        return reg[r];
    }
    else
    {
        // TODO: fail if not aligned address?
        short rv = reg[r & 0x7];
        return mem[rv] | (mem[rv + 1] << 8); // little endian
    }
}

void setValue(byte r, short v)
{
    if ((r & 0x8) == 0)
    {
        reg[r] = v;
    }
    else
    {
        // TODO: fail if not aligned address?
        short rv = reg[r & 0x7];
        mem[rv] = (v & 0xFF);
        mem[rv + 1] = (v >> 8); // little endian
    }
}

int main(void)
{
    // Set stdin to non-blocking mode
    int flags = fcntl(STDIN_FILENO, F_GETFL, 0);
    fcntl(STDIN_FILENO, F_SETFL, flags | O_NONBLOCK);
    setlocale(LC_ALL, "");

    readBlock(0, SHRT_MAX, 0);

    short max = 30;
    while (--max > 0)
    {
        short pc = reg[0];
        byte c = next();
        byte j = next();
        byte i = high(c);
        byte x = low(c);
        byte y = high(j);
        byte z = low(j);
        if ((i & 0b1000) != 0)
        {
            printf("+");
        }
        else
        {
            printf(" ");
        }
        switch(i & 0b111)
        {
            case 0: // CCP
                printf("CCP %2i=%2i if %2i=0            ", x, y, z);
                if (getValue(z) == 0)
                {
                    setValue(x, getValue(y));
                }
                break;
            case 1: // ADD
                printf("ADD %2i=%2i+%2i                 ", x, y, z);
                setValue(x, getValue(y) + getValue(z));
                break;
            case 2: // MUL
                printf("MUL %2i=%2i*%2i                 ", x, y, z);
                setValue(x, getValue(y) * getValue(z));
                break;
            case 3: // DIV
                printf("DIV %2i=%2i/%2i                 ", x, y, z);
                setValue(x, getValue(y) / getValue(z));
                break;
            case 4: // NAND
                printf("NAND %2i=%2i&%2i                ", x, y, z);
                setValue(x, ~(getValue(y) & getValue(z)));
                break;
            case 5: // SHIFT
                printf("SHL %2i=%2i<<>>%2i              ", x, y, z);
                short zv = getValue(z);
                if (zv < 0)
                {
                    setValue(x, getValue(y) << -zv);
                }
                else
                {
                    setValue(x, getValue(y) >> zv);
                }
                break;
            case 6: // IN
                printf("IN %2i->%2i (%2i)               ", x, y, z);
                setValue(x, getc(stdin));
                if (feof(stdin)) { clearerr(stdin); }
                break;
            case 7: // OUT
                printf("OUT %2i<-%2i (%2i)              ", x, y, z);
                wprintf(L"%lc", getValue(x));
                fflush(stdout);
                break;
            default:
                printf("Invalid instruction! (%i)\n", i);
                return 1;
        }
        if ((i & 0b1000) != 0)
        {
            if ((x & 0b1000) != 0) reg[x & 0b111] += 2;
            if ((y & 0b1000) != 0) reg[y & 0b111] += 2;
            if ((z & 0b1000) != 0) reg[z & 0b111] += 2;
        }

        printf("pc:%2i zero:%2i x:%2i y:%2i z:%2i 5:%2i 6:%2i 7:%2i m100:%2i m101:%2i m102:%2i\n", reg[0], reg[1], reg[2], reg[3], reg[4], reg[5], reg[6], reg[7], mem[100], mem[101], mem[102]);
        if (reg[0] == pc)
        {
            printf("HALT!");
            return 0;
        }
    }

    return 0;
}
