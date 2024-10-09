#include <stdio.h>
#include <wchar.h>
#include <locale.h>

#include <stdio.h>
#include <fcntl.h>
#include <unistd.h>

unsigned char mem[0x10000];

void setcell(unsigned short a, short y)
{
    extern unsigned char mem[];
    mem[a] = y;
    mem[a + 1] = y >> 8;
}

short getcell(unsigned short a)
{
    extern unsigned char mem[];
    return mem[a] | (mem[a + 1] << 8);
}

int main(void)
{
    // Set stdin to non-blocking mode
    int flags = fcntl(STDIN_FILENO, F_GETFL, 0);
    fcntl(STDIN_FILENO, F_SETFL, flags | O_NONBLOCK);

    short reg[64] = {};
    short rstack[256] = {};
    short* r = rstack;
    short pc = 0;

    FILE *file = fopen("image.bin", "r");
    fseek(file, 0, SEEK_END);
    long size = ftell(file);
    fseek(file, 0, SEEK_SET);
    if (!file || !fread(&mem, size, 1, file)) // assumes size <= sizeof(mem)
    {
        printf("Could not open boot image.\n");
        return 1;
    }
    fclose(file);

    short x, y, z, v;

    #define NEXT mem[pc++]
    #define X x = NEXT;
    #define XY X; y = NEXT
    #define XYZ XY; z = NEXT;
    #define V v = mem[pc] | mem[pc + 1] << 8; pc += 2
    #define Rx reg[x]
    #define Ry reg[y]
    #define Rz reg[z]
    #define OUT wprintf(L"%lc", Rx); fflush(stdout)
    #define IN Rx = getc(stdin); if (feof(stdin)) { clearerr(stdin); }

    setlocale(LC_ALL, "");

    while (1)
    {
        //printf("%04X -> %02X %02X %02X %02X %02X %02X %02X %02X %02X %02X \n", pc, mem[pc], mem[pc+1], mem[pc+2], mem[pc+3], mem[pc+4], mem[pc+5], mem[pc+6], mem[pc+7], mem[pc+8], mem[pc+9]);
        switch(NEXT)
        {
            case  0:        return 0;                    // halt
            case  1: V; X;  Rx = v;               break; // ldc (x = v) // TODO: swap x <-> v
            case  2: XY;    Rx = getcell(Ry);     break; // ld (x = m[y])
            case  3: XY;    setcell(Rx, Ry);      break; // st (m[x] = y)
            case  4: XY;    Rx = mem[Ry];         break; // ldb (x = m[y])
            case  5: XY;    mem[Rx] = Ry;         break; // stb (m[x] = y)
            case  6: XY;    Rx = Ry;              break; // cp (x = y)
            case  7: X;     IN;                   break; // in (x = getc())
            case  8: X;     OUT;                  break; // out (putc(x)())
            case  9: XY;    Rx = Ry + 1;          break; // inc (x = ++y)
            case 10: XY;    Rx = Ry - 1;          break; // dec (x = --y)
            case 11: XYZ;   Rx = Ry + Rz;         break; // add (x = y + z)
            case 12: XYZ;   Rx = Ry - Rz;         break; // sub (x = y - z)
            case 13: XYZ;   Rx = Ry * Rz;         break; // mul (x = y * z)
            case 14: XYZ;   Rx = Ry / Rz;         break; // div (x = y / z)
            case 15: XYZ;   Rx = Ry % Rz;         break; // mod (x = y % z)
            case 16: XYZ;   Rx = Ry & Rz;         break; // and (x = y & z)
            case 17: XYZ;   Rx = Ry | Rz;         break; // or (x = y | z)
            case 18: XYZ;   Rx = Ry ^ Rz;         break; // xor (x = y ^ z)
            case 19: XY;    Rx = ~Ry;             break; // not (x = ~y)
            case 20: XYZ;   Rx = Ry << Rz;        break; // shl (x = y << z)
            case 21: XYZ;   Rx = Ry >> Rz;        break; // shr (x = y >> z)
            case 22: V; XY; if (Rx == Ry) pc = v; break; // beq (branch if x == y)
            case 23: V; XY; if (Rx != Ry) pc = v; break; // bne (branch if x != y)
            case 24: V; XY; if (Rx >  Ry) pc = v; break; // bgt (branch if x > y)
            case 25: V; XY; if (Rx >= Ry) pc = v; break; // bge (branch if x >= y)
            case 26: V; XY; if (Rx <  Ry) pc = v; break; // blt (branch if x < y)
            case 27: V; XY; if (Rx <= Ry) pc = v; break; // ble (branch if x <= y)
            case 28: V;     pc = v;               break; // jump (pc = v)
            case 29: V;     *(r++) = pc; pc = v;  break; // call (jsr(v))
            case 30: X;     *(r++) = pc; pc = Rx; break; // exec (jsr(x))
            case 31:        pc = *(--r);          break; // return (ret)
            case 32: XY; // dump
                file = fopen("image.bin", "w");
                if (!file || !fwrite(mem + Ry, 1, Rx - Ry, file))
                {
                    printf("Could not write boot image.\n");
                    return 1;
                }
                fclose(file);
                break;
            default:
                printf("Invalid instruction! (pc=%04X [%04X])\n", pc - 2, getcell(pc - 2));
                return 1;
        }
    }

    return 0;
}
