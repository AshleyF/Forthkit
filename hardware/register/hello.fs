require assembler.fs

branch,

label 'message
char H c,
char e c,
char l c,
char l c,
char o c,
char , c,
bl c,
char W c,
char o c,
char r c,
char l c,
char d c,
char ! c,
0 c,
0 c,

label 'halt
zero halt,

patch,

2 constant one  1 one ldc, \ literal 1
3 constant msg  'message msg ldc,
4 constant ch
5 constant halt-addr  'halt halt-addr ldc,

label 'print
ch msg one ld+, \ ch = memory[msg++]
pc halt-addr ch cp?, \ if ch == 0, copy halt-addr to PC (jump out)
ch out, \ output character
'print jump, \ jump back to 'print

write-boot-block bye
