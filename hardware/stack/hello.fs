require assembler.fs

branch,

label 'message
12 c,
char H c,
char e c,
char l c,
char l c,
char o c,
char , c,
  bl   c,
char W c,
char o c,
char r c,
char l c,
char d c,
char ! c,

patch,

'message lit16,
ld8+, \ get length
for,
  ld8+, out, \ output character
next,
drop, halt,
