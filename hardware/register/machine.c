#include <stdio.h>

int main(void)
{
    unsigned short mem[0x8000];
    short pc = 0;
    short reg[32] = {};

    short rstack[256] = {};
    short* r = rstack;

    FILE *file = fopen("boot.bin", "r");
    if (!file || !fread(&mem, sizeof(mem), 1, file))
    {
        printf("Could not open boot image.");
        return 1;
    }
    fclose(file);

    short x;

    inline short next()
    {
        return mem[pc++];
    }

    inline short rval()
    {
        return reg[next()];
    }

    while (1)
    {
        switch(next())
        {
            case 0: // ldc (x = v)
                reg[next()] = next();
                break;
            case 1: // ld (x = m[y])
                reg[next()] = mem[rval()];
                break;
            case 2: // st (m[x] = y)
                mem[rval()] = rval();
                break;
            case 3: // cp (x = y)
                reg[next()] = reg[next()];
                break;
            case 4: // in (x = getc())
                reg[next()] = getc(stdin);
                break;
            case 5: // out (putc(x))
                putc(rval(), stdout);
                break;
            case 6: // inc (x = ++y)
                reg[next()] = rval() + 1;
                break;
            case 7: // dec (x = --y)
                reg[next()] = rval() - 1;
                break;
            case 8: // add (x = y + z)
                reg[next()] = rval() + rval();
                break;
            case 9: // sub (x = y - z)
                reg[next()] = rval() - rval();
                break;
            case 10: // mul (x = y * z)
                reg[next()] = rval() * rval();
                break;
            case 11: // div (x = y / z)
                reg[next()] = rval() / rval();
                break;
            case 12: // mod (x = y % z)
                reg[next()] = rval() % rval();
                break;
            case 13: // and (x = y & z)
                reg[next()] = rval() & rval();
                break;
            case 14: // or (x = y | z)
                reg[next()] = rval() | rval();
                break;
            case 15: // xor (x = y ^ z)
                reg[next()] = rval() ^ rval();
                break;
            case 16: // not (x = ~y)
                reg[next()] = ~rval();
                break;
            case 17: // lsh (x = y << z)
                reg[next()] = rval() << rval();
                break;
            case 18: // rsh (x = y >> z)
                reg[next()] = rval() >> rval();
                break;
            case 19: // beq (branch if x == y)
                x = next();
                if (rval() == rval()) pc = x;
                break;
            case 20: // bne (branch if x != y)
                x = next();
                if (rval() != rval()) pc = x;
                break;
            case 21: // bgt (branch if x > y)
                x = next();
                if (rval() > rval()) pc = x;
                break;
            case 22: // bge (branch if x >= y)
                x = next();
                if (rval() >= rval()) pc = x;
                break;
            case 23: // blt (branch if x < y)
                x = next();
                if (rval() < rval()) pc = x;
                break;
            case 24: // ble (branch if x <= y)
                x = next();
                if (rval() <= rval()) pc = x;
                break;
            case 25: // exec (pc = x)
                pc = rval();
                break;
            case 26: // jump (pc = v)
                pc = next();
                break;
            case 27: // call (jsr(v))
                *(r++) = pc + 1;
                pc = next();
                break;
            case 28: // return (ret)
                pc = *(--r);
                break;
            case 29: // halt
                return 0;
            case 30: // dump
                file = fopen("boot.bin", "w");
                if (!file || !fwrite(&mem, sizeof(mem), 1, file))
                {
                    printf("Could not write boot image.");
                    return 1;
                }
                fclose(file);
                return 0;
            default:
                printf("Invalid instruction!");
                return 1;
        }
    }

    return 0;
}
