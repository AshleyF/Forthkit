#include <stdio.h>
#include <wchar.h>
#include <locale.h>

int main(void)
{
    short reg[64] = {};
    unsigned char mem[0x10000];
    short rstack[256] = {};
    short* r = rstack;
    short pc = 0;

    FILE *file = fopen("image.bin", "r");
    if (!file || !fread(&mem, sizeof(mem), 1, file))
    {
        printf("Could not open boot image.\n");
        return 1;
    }
    fclose(file);

    short x, y, z;

    void setcell(unsigned short a, short y)
    {
        mem[a] = y;
        mem[a + 1] = y >> 8;
    }

    short getcell(unsigned short a)
    {
        return mem[a] | (mem[a + 1] << 8);
    }

    short getinst()
    {
        short c = getcell(pc);
        pc += 2;
        return c;
    }

    #define NEXT getinst()
    #define X x = NEXT;
    #define XY X; y = NEXT;
    #define XYZ XY; z = NEXT;
    #define Rx reg[x]
    #define Ry reg[y]
    #define Rz reg[z]

    setlocale(LC_ALL, "");

    while (1)
    {
        //printf("%04X -> %02X %02X %02X %02X %02X %02X %02X %02X %02X %02X \n", pc, mem[pc], mem[pc+1], mem[pc+2], mem[pc+3], mem[pc+4], mem[pc+5], mem[pc+6], mem[pc+7], mem[pc+8], mem[pc+9]);
        switch(mem[pc++])
        {
            case  0:      return 0; // halt
            case  1: XY;  Rx = y;               break; // ldc (x = v)
            case  2: XY;  Rx = getcell(Ry);     break; // ld (x = m[y])
            case  3: XY;  setcell(Rx, Ry);      break; // st (m[x] = y)
            case  4: XY;  Rx = mem[Ry];         break; // ldb (x = m[y])
            case  5: XY;  mem[Rx] = Ry;         break; // stb (m[x] = y)
            case  6: XY;  Rx = Ry;              break; // cp (x = y)
            case  7: X;   Rx = getc(stdin);     break; // in (x = getc())
            case  8: X;   wprintf(L"%lc", Rx);  break; // out (putc(x)())
            case  9: XY;  Rx = Ry + 1;          break; // inc (x = ++y)
            case 10: XY;  Rx = Ry - 1;          break; // dec (x = --y)
            case 11: XYZ; Rx = Ry + Rz;         break; // add (x = y + z)
            case 12: XYZ; Rx = Ry - Rz;         break; // sub (x = y - z)
            case 13: XYZ; Rx = Ry * Rz;         break; // mul (x = y * z)
            case 14: XYZ; Rx = Ry / Rz;         break; // div (x = y / z)
            case 15: XYZ; Rx = Ry % Rz;         break; // mod (x = y % z)
            case 16: XYZ; Rx = Ry & Rz;         break; // and (x = y & z)
            case 17: XYZ; Rx = Ry | Rz;         break; // or (x = y | z)
            case 18: XYZ; Rx = Ry ^ Rz;         break; // xor (x = y ^ z)
            case 19: XY;  Rx = ~Ry;             break; // not (x = ~y)
            case 20: XYZ; Rx = Ry << Rz;        break; // shl (x = y << z)
            case 21: XYZ; Rx = Ry >> Rz;        break; // shr (x = y >> z)
            case 22: XYZ; if (Ry == Rz) pc = x; break; // beq (branch if x == y)
            case 23: XYZ; if (Ry != Rz) pc = x; break; // bne (branch if x != y)
            case 24: XYZ; if (Ry >  Rz) pc = x; break; // bgt (branch if x > y)
            case 25: XYZ; if (Ry >= Rz) pc = x; break; // bge (branch if x >= y)
            case 26: XYZ; if (Ry <  Rz) pc = x; break; // blt (branch if x < y)
            case 27: XYZ; if (Ry <= Rz) pc = x; break; // ble (branch if x <= y)
            case 28: X;   pc = x;               break; // jump (pc = v)
            case 29: X;   *(r++) = pc; pc = x;  break; // call (jsr(v))
            case 30: X;   *(r++) = pc; pc = Rx; break; // exec (jsr(x))
            case 31:      pc = *(--r);          break; // return (ret)
            case 32: // dump
                file = fopen("image.bin", "w");
                if (!file || !fwrite(&mem, sizeof(mem), 1, file))
                {
                    printf("Could not write boot image.\n");
                    return 1;
                }
                fclose(file);
                break;
            case 33: // debug
                printf("Inst: %i Reg: %04x %04x %04x %04x %04x %04x %04x Stack: %04x %04x %04x %04x %04x %04x %04x %04x Return: %i %i %i %i %i %i %i %i\n", mem[pc], reg[0], reg[1], reg[2], reg[3], reg[4], reg[5], reg[6], mem[32767], mem[32766], mem[32765], mem[32764], mem[32763], mem[32762], mem[32761], mem[32760], mem[32255], mem[32254], mem[32253], mem[32252], mem[32251], mem[32250], mem[32249], mem[32248]);
                break;
            default:
                printf("Invalid instruction! (pc=%04X [%04X])\n", pc - 2, getcell(pc - 2));
                return 1;
        }
    }

    return 0;
}