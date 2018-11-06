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

    #define NEXT mem[pc++]
    #define RVAL reg[NEXT]

    while (1)
    {
        switch(NEXT)
        {
            case 0: // ldc (x = v)
                reg[NEXT] = NEXT;
                break;
            case 1: // ld (x = m[y])
                reg[NEXT] = mem[RVAL];
                break;
            case 2: // st (m[x] = y)
                mem[RVAL] = RVAL;
                break;
            case 3: // cp (x = y)
                reg[NEXT] = reg[NEXT];
                break;
            case 4: // in (x = getc())
                reg[NEXT] = getc(stdin);
                break;
            case 5: // out (putc(x))
                putc(RVAL, stdout);
                break;
            case 6: // inc (x = ++y)
                reg[NEXT] = RVAL + 1;
                break;
            case 7: // dec (x = --y)
                reg[NEXT] = RVAL - 1;
                break;
            case 8: // add (x = y + z)
                reg[NEXT] = RVAL + RVAL;
                break;
            case 9: // sub (x = y - z)
                reg[NEXT] = RVAL - RVAL;
                break;
            case 10: // mul (x = y * z)
                reg[NEXT] = RVAL * RVAL;
                break;
            case 11: // div (x = y / z)
                reg[NEXT] = RVAL / RVAL;
                break;
            case 12: // mod (x = y % z)
                reg[NEXT] = RVAL % RVAL;
                break;
            case 13: // and (x = y & z)
                reg[NEXT] = RVAL & RVAL;
                break;
            case 14: // or (x = y | z)
                reg[NEXT] = RVAL | RVAL;
                break;
            case 15: // xor (x = y ^ z)
                reg[NEXT] = RVAL ^ RVAL;
                break;
            case 16: // not (x = ~y)
                reg[NEXT] = ~RVAL;
                break;
            case 17: // lsh (x = y << z)
                reg[NEXT] = RVAL << RVAL;
                break;
            case 18: // rsh (x = y >> z)
                reg[NEXT] = RVAL >> RVAL;
                break;
            case 19: // beq (branch if x == y)
                x = NEXT;
                if (RVAL == RVAL) pc = x;
                break;
            case 20: // bne (branch if x != y)
                x = NEXT;
                if (RVAL != RVAL) pc = x;
                break;
            case 21: // bgt (branch if x > y)
                x = NEXT;
                if (RVAL > RVAL) pc = x;
                break;
            case 22: // bge (branch if x >= y)
                x = NEXT;
                if (RVAL >= RVAL) pc = x;
                break;
            case 23: // blt (branch if x < y)
                x = NEXT;
                if (RVAL < RVAL) pc = x;
                break;
            case 24: // ble (branch if x <= y)
                x = NEXT;
                if (RVAL <= RVAL) pc = x;
                break;
            case 25: // exec (pc = x)
                pc = RVAL;
                break;
            case 26: // jump (pc = v)
                pc = NEXT;
                break;
            case 27: // call (jsr(v))
                *(r++) = pc + 1;
                pc = NEXT;
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
