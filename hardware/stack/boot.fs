require assembler.fs

segment,

123 lit8,
456 lit16,
in,
dup,
out,
out,
halt,

execute,

write-image
." Boot image written"

bye
