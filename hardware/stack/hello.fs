require assembler.fs

skip,

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

start,

'message literal,
ld8+, \ get length
for,
  ld8+, out, \ output character
next,
drop, zero, halt,