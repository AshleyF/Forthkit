.( Unit tests)

\ F.3.1 Basic Assumptions
\ These test assume a two's complement implementation where the range of signed numbers
\ is -2^(n-1) ... 2^(n-1)-1 and the range of unsigned numbers is 0 ... 2^(n-1).

T{ -> }T                      ( Start with a clean slate )
( Test if any bits are set; Answer in base 1 )
T{ : BITSSET? IF 0 0 ELSE 0 THEN ; -> }T
T{  0 BITSSET? -> 0 }T        ( Zero is all bits clear )
T{  1 BITSSET? -> 0 0 }T      ( Other numbers have at least one bit )
T{ -1 BITSSET? -> 0 0 }T

\ F.3.2 Booleans
\ To test the booleans it is first necessary to test F.6.1.0720 AND, and F.6.1.1720 INVERT.
\ Before moving on to the test F.6.1.0950 CONSTANT.
\ The latter defines two constants (0S and 1S) which will be used in the further test.
\ It is then possible to complete the testing of F.6.1.0720 AND, F.6.1.1980 OR, and F.6.1.2490 XOR.

 0 constant 0S \ all zeros (definition missing in the standard)
-1 constant 1S \ all ones (definition missing in the standard)

\ F.6.1.0720 AND
T{ 0 0 AND -> 0 }T
T{ 0 1 AND -> 0 }T
T{ 1 0 AND -> 0 }T
T{ 1 1 AND -> 1 }T
T{ 0 INVERT 1 AND -> 1 }T
T{ 1 INVERT 1 AND -> 0 }T
T{ 0S 0S AND -> 0S }T
T{ 0S 1S AND -> 0S }T
T{ 1S 0S AND -> 0S }T
T{ 1S 1S AND -> 1S }T

\ F.6.1.1720 INVERT
T{ 0S INVERT -> 1S }T
T{ 1S INVERT -> 0S }T

\ F.6.1.0950 CONSTANT
T{ 123 CONSTANT X123 -> }T
T{ X123 -> 123 }T
T{ : EQU CONSTANT ; -> }T
T{ X123 EQU Y123 -> }T
T{ Y123 -> 123 }T

\ F.6.1.1980 OR
T{ 0S 0S OR -> 0S }T
T{ 0S 1S OR -> 1S }T
T{ 1S 0S OR -> 1S }T
T{ 1S 1S OR -> 1S }T

\ F.6.1.2490 XOR
T{ 0S 0S XOR -> 0S }T
T{ 0S 1S XOR -> 1S }T
T{ 1S 0S XOR -> 1S }T
T{ 1S 1S XOR -> 0S }T

\ F.3.3 Shifts
\ To test the shift operators it is necessary to calculate the most significant bit of a cell:

1S 1 RSHIFT INVERT CONSTANT MSB

\ RSHIFT is tested later. MSB must have at least one bit set:

T{ MSB BITSSET? -> 0 0 }T

\ The test F.6.1.0320 2*, F.6.1.0330 2/, F.6.1.1805 LSHIFT, and F.6.1.2162 RSHIFT can now be performed.

\ 2*
T{   0S 2*       ->   0S }T
T{    1 2*       ->    2 }T
T{ 4000 2*       -> 8000 }T
T{   1S 2* 1 XOR ->   1S }T
T{  MSB 2*       ->   0S }T

\ 2/
T{          0S 2/ ->   0S }T
T{           1 2/ ->    0 }T
T{        4000 2/ -> 2000 }T
T{          1S 2/ ->   1S }T \ MSB PROPOGATED
T{    1S 1 XOR 2/ ->   1S }T
T{ MSB 2/ MSB AND ->  MSB }T

\ LSHIFT
T{   1 0 LSHIFT ->    1 }T
T{   1 1 LSHIFT ->    2 }T
T{   1 2 LSHIFT ->    4 }T
\ T{   1 F LSHIFT -> 8000 }T      \ BIGGEST GUARANTEED SHIFT
T{  1 15 LSHIFT -> 32768 }T      \ TODO: support hex (above)
T{  1S 1 LSHIFT 1 XOR -> 1S }T
T{ MSB 1 LSHIFT ->    0 }T

\ RSHIFT
T{    1 0 RSHIFT -> 1 }T
T{    1 1 RSHIFT -> 0 }T
T{    2 1 RSHIFT -> 1 }T
T{    4 2 RSHIFT -> 1 }T
\ T{ 8000 F RSHIFT -> 1 }T                \ Biggest
T{ 32768 15 RSHIFT -> 1 }T                \ TODO: support hex (above)
T{  MSB 1 RSHIFT MSB AND ->   0 }T    \ RSHIFT zero fills MSBs
T{  MSB 1 RSHIFT     2*  -> MSB }T

\ F.3.4 Numeric notation
\ The numeric representation can be tested with the following test cases:

\ TODO: support all these formats!
\ DECIMAL
\ T{ #1289       -> 1289        }T
\ T{ #12346789.  -> 12346789. }T
\ T{ #-1289      -> -1289       }T
\ T{ #-12346789. -> -12346789.  }T
\ T{ $12eF       -> 4847        }T
\ T{ $12aBcDeF.  -> 313249263.  }T
\ T{ $-12eF      -> -4847       }T
\ T{ $-12AbCdEf. -> -313249263. }T
\ T{ %10010110   -> 150         }T
\ T{ %10010110.  -> 150.        }T
\ T{ %-10010110  -> -150        }T
\ T{ %-10010110. -> -150.       }T
\ T{ 'z'         -> 122         }T

\ F.3.5 Comparisons
\ Before testing the comparison operators it is necessary to define a few constants
\ to allow the testing of the upper and lower bounds.

0 INVERT CONSTANT MAX-UINT
0 INVERT 1 RSHIFT CONSTANT MAX-INT
0 INVERT 1 RSHIFT INVERT CONSTANT MIN-INT
0 INVERT 1 RSHIFT CONSTANT MID-UINT
0 INVERT 1 RSHIFT INVERT CONSTANT MID-UINT+1

0S CONSTANT <FALSE>
1S CONSTANT <TRUE>

\ With these constants defined, it is now possible to perform the F.6.1.0270 0=, F.6.1.0530 =,
\ F.6.1.0250 0<, F.6.1.0480 <, F.6.1.0540 >, F.6.1.2340 U<, F.6.1.1880 MIN, and F.6.1.1870 MAX test.

\ F.6.1.0270 0=
T{        0 0= -> <TRUE>  }T
T{        1 0= -> <FALSE> }T
T{        2 0= -> <FALSE> }T
T{       -1 0= -> <FALSE> }T
T{ MAX-UINT 0= -> <FALSE> }T
T{ MIN-INT  0= -> <FALSE> }T
T{ MAX-INT  0= -> <FALSE> }T

\ F.6.1.0530 =
T{  0  0 = -> <TRUE>  }T
T{  1  1 = -> <TRUE>  }T
T{ -1 -1 = -> <TRUE>  }T
T{  1  0 = -> <FALSE> }T
T{ -1  0 = -> <FALSE> }T
T{  0  1 = -> <FALSE> }T
T{  0 -1 = -> <FALSE> }T

\ F.6.1.0250 0<
T{       0 0< -> <FALSE> }T
T{      -1 0< -> <TRUE>  }T
T{ MIN-INT 0< -> <TRUE>  }T
T{       1 0< -> <FALSE> }T
T{ MAX-INT 0< -> <FALSE> }T

\ F.6.1.0480 <
T{       0       1 < -> <TRUE>  }T
T{       1       2 < -> <TRUE>  }T
T{      -1       0 < -> <TRUE>  }T
T{      -1       1 < -> <TRUE>  }T
T{ MIN-INT       0 < -> <TRUE>  }T
T{ MIN-INT MAX-INT < -> <TRUE>  }T
T{       0 MAX-INT < -> <TRUE>  }T
T{       0       0 < -> <FALSE> }T
T{       1       1 < -> <FALSE> }T
T{       1       0 < -> <FALSE> }T
T{       2       1 < -> <FALSE> }T
T{       0      -1 < -> <FALSE> }T
T{       1      -1 < -> <FALSE> }T
T{       0 MIN-INT < -> <FALSE> }T
T{ MAX-INT MIN-INT < -> <FALSE> }T
T{ MAX-INT       0 < -> <FALSE> }T

\ F.6.1.0540 >
T{       0       1 > -> <FALSE> }T
T{       1       2 > -> <FALSE> }T
T{      -1       0 > -> <FALSE> }T
T{      -1       1 > -> <FALSE> }T
T{ MIN-INT       0 > -> <FALSE> }T
T{ MIN-INT MAX-INT > -> <FALSE> }T
T{       0 MAX-INT > -> <FALSE> }T
T{       0       0 > -> <FALSE> }T
T{       1       1 > -> <FALSE> }T
T{       1       0 > -> <TRUE>  }T
T{       2       1 > -> <TRUE>  }T
T{       0      -1 > -> <TRUE>  }T
T{       1      -1 > -> <TRUE>  }T
T{       0 MIN-INT > -> <TRUE>  }T
T{ MAX-INT MIN-INT > -> <TRUE>  }T
T{ MAX-INT       0 > -> <TRUE>  }T

\ F.6.1.2340 U<
T{        0        1 U< -> <TRUE>  }T
T{        1        2 U< -> <TRUE>  }T
T{        0 MID-UINT U< -> <TRUE>  }T
T{        0 MAX-UINT U< -> <TRUE>  }T
T{ MID-UINT MAX-UINT U< -> <TRUE>  }T
T{        0        0 U< -> <FALSE> }T
T{        1        1 U< -> <FALSE> }T
T{        1        0 U< -> <FALSE> }T
T{        2        1 U< -> <FALSE> }T
T{ MID-UINT        0 U< -> <FALSE> }T
T{ MAX-UINT        0 U< -> <FALSE> }T
T{ MAX-UINT MID-UINT U< -> <FALSE> }T

\ F.6.1.1880 MIN
T{       0       1 MIN ->       0 }T
T{       1       2 MIN ->       1 }T
T{      -1       0 MIN ->      -1 }T
T{      -1       1 MIN ->      -1 }T
T{ MIN-INT       0 MIN -> MIN-INT }T
T{ MIN-INT MAX-INT MIN -> MIN-INT }T
T{       0 MAX-INT MIN ->       0 }T
T{       0       0 MIN ->       0 }T
T{       1       1 MIN ->       1 }T
T{       1       0 MIN ->       0 }T
T{       2       1 MIN ->       1 }T
T{       0      -1 MIN ->      -1 }T
T{       1      -1 MIN ->      -1 }T
T{       0 MIN-INT MIN -> MIN-INT }T
T{ MAX-INT MIN-INT MIN -> MIN-INT }T
T{ MAX-INT       0 MIN ->       0 }T

\ F.6.1.1870 MAX
T{       0       1 MAX ->       1 }T
T{       1       2 MAX ->       2 }T
T{      -1       0 MAX ->       0 }T
T{      -1       1 MAX ->       1 }T
T{ MIN-INT       0 MAX ->       0 }T
T{ MIN-INT MAX-INT MAX -> MAX-INT }T
T{       0 MAX-INT MAX -> MAX-INT }T
T{       0       0 MAX ->       0 }T
T{       1       1 MAX ->       1 }T
T{       1       0 MAX ->       1 }T
T{       2       1 MAX ->       2 }T
T{       0      -1 MAX ->       0 }T
T{       1      -1 MAX ->       1 }T
T{       0 MIN-INT MAX ->       0 }T
T{ MAX-INT MIN-INT MAX -> MAX-INT }T
T{ MAX-INT       0 MAX -> MAX-INT }T

.( Test complete.)
\ bye
