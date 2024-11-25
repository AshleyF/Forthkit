require assembler.fs

2 constant upper
3 constant x

 32 upper ldc,  \ 1220       LDC upper $20

   label 'loop  \            'loop = 0002
        x in,   \ C3         IN x
x x upper sub,  \ 6233       SUB upper x x
        x out,  \ D3         OUT x
    'loop jump, \ 2100 0200  LD+ zero pc pc 0002
 
write-boot-block bye