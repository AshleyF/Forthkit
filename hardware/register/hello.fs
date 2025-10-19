require assembler.fs

\ Hello World test program

\ Output "Hello, World!" character by character
72 2 ldc, 0 2 out,    \ H
101 2 ldc, 0 2 out,   \ e
108 2 ldc, 0 2 out,   \ l
108 2 ldc, 0 2 out,   \ l
111 2 ldc, 0 2 out,   \ o
44 2 ldc, 0 2 out,    \ ,
32 2 ldc, 0 2 out,    \ (space)
87 2 ldc, 0 2 out,    \ W
111 2 ldc, 0 2 out,   \ o
114 2 ldc, 0 2 out,   \ r
108 2 ldc, 0 2 out,   \ l
100 2 ldc, 0 2 out,   \ d
33 2 ldc, 0 2 out,    \ !
10 2 ldc, 0 2 out,    \ newline

zero halt,

\ Write the program to block0.bin using the fixed helper
write-boot-block

bye
