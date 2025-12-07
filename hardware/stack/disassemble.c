#include <stdio.h>

void disassemble(short c) {
    printf("%x: ", c);
    c = (c & 0xff) << 8 | ((c & 0xff00) >> 8);
    if ((c & 1) == 0) { // call?
        printf("CALL[%x] ", c);
    } else { // instructions
        for (short slot = 0; slot < 15; slot += 5) {
            short i = (c >> (11 - slot)) & 0x1F;
            switch (i) {
                case  0: printf("halt "); break;
                case  1: printf("add "); break;
                case  2: printf("sub "); break;
                case  3: printf("mul "); break;
                case  4: printf("div "); break;
                case  5: printf("not "); break;
                case  6: printf("and "); break;
                case  7: printf("or "); break;
                case  8: printf("xor "); break;
                case  9: printf("shl "); break;
                case 10: printf("shr "); break;
                case 11: printf("in "); break;
                case 12: printf("out "); break;
                case 13: printf("read "); break;
                case 14: printf("write "); break;
                case 15: printf("ld16+ "); break;
                case 16: printf("ld8+ "); break;
                case 17: printf("st16+ "); break;
                case 18: printf("st8+ "); break;
                case 19: printf("lit16 "); break;
                case 20: printf("lit8 "); break;
                case 21: printf("0jump "); break;
                case 22: printf("next "); break;
                case 23: printf("drop "); break;
                case 24: printf("dup "); break;
                case 25: printf("over "); break;
                case 26: printf("swap "); break;
                case 27: printf("push "); break;
                case 28: printf("pop "); break;
                case 29: printf("peek "); break;
                case 30: printf("ret "); break;
                case 31: printf("nop "); break;
            }
        }
    }
    printf("\n");
}

int main(void) {
    disassemble(0x11BE);
    disassemble(0x3FA8);
}