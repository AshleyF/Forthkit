#include <stdio.h>

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