require assembler.fs

skip,

label 'message
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
0 c,

label 'halt
zero halt,

start,

2 constant one    1 one   lit8, \ literal 1
3 constant eight  8 eight lit8, \ literal 8

4 constant halt-addr  'halt halt-addr lit8,
5 constant mask  mask mask not,  mask mask eight shr,

6 constant msg  'message msg lit8,
7 constant ch

label 'print
     ch msg one ld16+, \ ch = memory[msg++]
     ch ch mask and,   \ single byte
pc halt-addr ch cp?,   \ if ch == 0, pc=halt-addr (jump out)
             ch out,   \ output character
         'print jump,  \ jump back to 'print