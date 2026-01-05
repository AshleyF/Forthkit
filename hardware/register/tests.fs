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

\ F.3.6 Stack Operators
\ The stack operators can be tested without any prepatory work. The "normal" operators (F.6.1.1260 DROP,
\ F.6.1.1290 DUP, F.6.1.1990 OVER, F.6.1.2160 ROT, and F.6.1.2260 SWAP) should be tested first, followed by
\ the two-cell variants (F.6.1.0370 2DROP, F.6.1.0380 2DUP, F.6.1.0400 2OVER and F.6.1.0430 2SWAP) with
\ F.6.1.0630 ?DUP and F.6.1.1200 DEPTH being performed last.

\ F.6.1.1260 DROP
T{ 1 2 DROP -> 1 }T
T{ 0   DROP ->   }T

\ F.6.1.1290 DUP
T{ 1 DUP -> 1 1 }T

\ F.6.1.1990 OVER
T{ 1 2 OVER -> 1 2 1 }T

\ F.6.1.2160 ROT
T{ 1 2 3 ROT -> 2 3 1 }T

\ F.6.1.2260 SWAP
T{ 1 2 SWAP -> 2 1 }T

\ F.6.1.0370 2DROP
T{ 1 2 2DROP -> }T

\ F.6.1.0380 2DUP
T{ 1 2 2DUP -> 1 2 1 2 }T

\ F.6.1.0400 2OVER
T{ 1 2 3 4 2OVER -> 1 2 3 4 1 2 }T

\ F.6.1.0430 2SWAP
T{ 1 2 3 4 2SWAP -> 3 4 1 2 }T

\ F.6.1.0630 ?DUP
T{ -1 ?DUP -> -1 -1 }T
T{  0 ?DUP ->  0    }T
T{  1 ?DUP ->  1  1 }T

\ F.6.1.1200 DEPTH
T{ 0 1 DEPTH -> 0 1 2 }T
T{   0 DEPTH -> 0 1   }T
T{     DEPTH -> 0     }T

\ F.3.7 Return Stack Operators
\ The test F.6.1.0580 >R will test all three basic return stack operators (>R, R>, and R@).

\ F.6.1.0580 >R
T{ : GR1 >R R> ; -> }T
T{ : GR2 >R R@ R> DROP ; -> }T
T{ 123 GR1 -> 123 }T
T{ 123 GR2 -> 123 }T
T{  1S GR1 ->  1S }T      ( Return stack holds cells )

\ F.3.8 Addition and Subtraction
\ Basic addition and subtraction should be tested in the order: F.6.1.0120 +,
\ F.6.1.0160 -, F.6.1.0290 1+, F.6.1.0300 1-, F.6.1.0690 ABS and F.6.1.1910 NEGATE.

\ F.6.1.0120 +
T{        0  5 + ->          5 }T
T{        5  0 + ->          5 }T
T{        0 -5 + ->         -5 }T
T{       -5  0 + ->         -5 }T
T{        1  2 + ->          3 }T
T{        1 -2 + ->         -1 }T
T{       -1  2 + ->          1 }T
T{       -1 -2 + ->         -3 }T
T{       -1  1 + ->          0 }T
T{ MID-UINT  1 + -> MID-UINT+1 }T

\ F.6.1.0160 -
T{          0  5 - ->       -5 }T
T{          5  0 - ->        5 }T
T{          0 -5 - ->        5 }T
T{         -5  0 - ->       -5 }T
T{          1  2 - ->       -1 }T
T{          1 -2 - ->        3 }T
T{         -1  2 - ->       -3 }T
T{         -1 -2 - ->        1 }T
T{          0  1 - ->       -1 }T
T{ MID-UINT+1  1 - -> MID-UINT }T

\ F.6.1.0290 1+
T{        0 1+ ->          1 }T
T{       -1 1+ ->          0 }T
T{        1 1+ ->          2 }T
T{ MID-UINT 1+ -> MID-UINT+1 }T

\ F.6.1.0300 1-
T{          2 1- ->        1 }T
T{          1 1- ->        0 }T
T{          0 1- ->       -1 }T
T{ MID-UINT+1 1- -> MID-UINT }T

\ F.6.1.0690 ABS
T{       0 ABS ->          0 }T
T{       1 ABS ->          1 }T
T{      -1 ABS ->          1 }T
T{ MIN-INT ABS -> MID-UINT+1 }T

\ F.6.1.1910 NEGATE
T{  0 NEGATE ->  0 }T
T{  1 NEGATE -> -1 }T
T{ -1 NEGATE ->  1 }T
T{  2 NEGATE -> -2 }T
T{ -2 NEGATE ->  2 }T

\ F.3.9 Multiplication
\ The multiplication operators should be tested in the order: F.6.1.2170 S>D, F.6.1.0090 *,
\ F.6.1.1810 M*, and F.6.1.2360 UM*.

\ F.6.1.2170 S>D
T{       0 S>D ->       0  0 }T
T{       1 S>D ->       1  0 }T
T{       2 S>D ->       2  0 }T
T{      -1 S>D ->      -1 -1 }T
T{      -2 S>D ->      -2 -1 }T
T{ MIN-INT S>D -> MIN-INT -1 }T
T{ MAX-INT S>D -> MAX-INT  0 }T

\ F.6.1.0090 *
T{  0  0 * ->  0 }T          \ TEST IDENTITIE\S
T{  0  1 * ->  0 }T
T{  1  0 * ->  0 }T
T{  1  2 * ->  2 }T
T{  2  1 * ->  2 }T
T{  3  3 * ->  9 }T
T{ -3  3 * -> -9 }T
T{  3 -3 * -> -9 }T
T{ -3 -3 * ->  9 }T
T{ MID-UINT+1 1 RSHIFT 2 *               -> MID-UINT+1 }T
T{ MID-UINT+1 2 RSHIFT 4 *               -> MID-UINT+1 }T
T{ MID-UINT+1 1 RSHIFT MID-UINT+1 OR 2 * -> MID-UINT+1 }T

\ F.6.1.1810 M* TODO
\ T{       0       0 M* ->       0 S>D }T
\ T{       0       1 M* ->       0 S>D }T
\ T{       1       0 M* ->       0 S>D }T
\ T{       1       2 M* ->       2 S>D }T
\ T{       2       1 M* ->       2 S>D }T
\ T{       3       3 M* ->       9 S>D }T
\ T{      -3       3 M* ->      -9 S>D }T
\ T{       3      -3 M* ->      -9 S>D }T
\ T{      -3      -3 M* ->       9 S>D }T
\ T{       0 MIN-INT M* ->       0 S>D }T
\ T{       1 MIN-INT M* -> MIN-INT S>D }T
\ T{       2 MIN-INT M* ->       0 1S  }T
\ T{       0 MAX-INT M* ->       0 S>D }T
\ T{       1 MAX-INT M* -> MAX-INT S>D }T
\ T{       2 MAX-INT M* -> MAX-INT     1 LSHIFT 0 }T
\ T{ MIN-INT MIN-INT M* ->       0 MSB 1 RSHIFT   }T
\ T{ MAX-INT MIN-INT M* ->     MSB MSB 2/         }T
\ T{ MAX-INT MAX-INT M* ->       1 MSB 2/ INVERT  }T

\ F.6.1.2360 UM* TODO
\ T{ 0 0 UM* -> 0 0 }T
\ T{ 0 1 UM* -> 0 0 }T
\ T{ 1 0 UM* -> 0 0 }T
\ T{ 1 2 UM* -> 2 0 }T
\ T{ 2 1 UM* -> 2 0 }T
\ T{ 3 3 UM* -> 9 0 }T
\ T{ MID-UINT+1 1 RSHIFT 2 UM* ->  MID-UINT+1 0 }T
\ T{ MID-UINT+1          2 UM* ->           0 1 }T
\ T{ MID-UINT+1          4 UM* ->           0 2 }T
\ T{         1S          2 UM* -> 1S 1 LSHIFT 1 }T
\ T{   MAX-UINT   MAX-UINT UM* ->    1 1 INVERT }T

\ F.3.10 Division
\ Due to the complexity of the division operators they are tested separately from the multiplication
\ operators. The basic division operators are tested first: F.6.1.1561 FM/MOD, F.6.1.2214 SM/REM, and
\ F.6.1.2370 UM/MOD.

\ F.6.1.1561 FM/MOD TODO
\ T{       0 S>D              1 FM/MOD ->  0       0 }T
\ T{       1 S>D              1 FM/MOD ->  0       1 }T
\ T{       2 S>D              1 FM/MOD ->  0       2 }T
\ T{      -1 S>D              1 FM/MOD ->  0      -1 }T
\ T{      -2 S>D              1 FM/MOD ->  0      -2 }T
\ T{       0 S>D             -1 FM/MOD ->  0       0 }T
\ T{       1 S>D             -1 FM/MOD ->  0      -1 }T
\ T{       2 S>D             -1 FM/MOD ->  0      -2 }T
\ T{      -1 S>D             -1 FM/MOD ->  0       1 }T
\ T{      -2 S>D             -1 FM/MOD ->  0       2 }T
\ T{       2 S>D              2 FM/MOD ->  0       1 }T
\ T{      -1 S>D             -1 FM/MOD ->  0       1 }T
\ T{      -2 S>D             -2 FM/MOD ->  0       1 }T
\ T{       7 S>D              3 FM/MOD ->  1       2 }T
\ T{       7 S>D             -3 FM/MOD -> -2      -3 }T
\ T{      -7 S>D              3 FM/MOD ->  2      -3 }T
\ T{      -7 S>D             -3 FM/MOD -> -1       2 }T
\ T{ MAX-INT S>D              1 FM/MOD ->  0 MAX-INT }T
\ T{ MIN-INT S>D              1 FM/MOD ->  0 MIN-INT }T
\ T{ MAX-INT S>D        MAX-INT FM/MOD ->  0       1 }T
\ T{ MIN-INT S>D        MIN-INT FM/MOD ->  0       1 }T
\ T{    1S 1                  4 FM/MOD ->  3 MAX-INT }T
\ T{       1 MIN-INT M*       1 FM/MOD ->  0 MIN-INT }T
\ T{       1 MIN-INT M* MIN-INT FM/MOD ->  0       1 }T
\ T{       2 MIN-INT M*       2 FM/MOD ->  0 MIN-INT }T
\ T{       2 MIN-INT M* MIN-INT FM/MOD ->  0       2 }T
\ T{       1 MAX-INT M*       1 FM/MOD ->  0 MAX-INT }T
\ T{       1 MAX-INT M* MAX-INT FM/MOD ->  0       1 }T
\ T{       2 MAX-INT M*       2 FM/MOD ->  0 MAX-INT }T
\ T{       2 MAX-INT M* MAX-INT FM/MOD ->  0       2 }T
\ T{ MIN-INT MIN-INT M* MIN-INT FM/MOD ->  0 MIN-INT }T
\ T{ MIN-INT MAX-INT M* MIN-INT FM/MOD ->  0 MAX-INT }T
\ T{ MIN-INT MAX-INT M* MAX-INT FM/MOD ->  0 MIN-INT }T
\ T{ MAX-INT MAX-INT M* MAX-INT FM/MOD ->  0 MAX-INT }T

\ F.6.1.2214 SM/REM
\ T{       0 S>D              1 SM/REM ->  0       0 }T
\ T{       1 S>D              1 SM/REM ->  0       1 }T
\ T{       2 S>D              1 SM/REM ->  0       2 }T
\ T{      -1 S>D              1 SM/REM ->  0      -1 }T
\ T{      -2 S>D              1 SM/REM ->  0      -2 }T
\ T{       0 S>D             -1 SM/REM ->  0       0 }T
\ T{       1 S>D             -1 SM/REM ->  0      -1 }T
\ T{       2 S>D             -1 SM/REM ->  0      -2 }T
\ T{      -1 S>D             -1 SM/REM ->  0       1 }T
\ T{      -2 S>D             -1 SM/REM ->  0       2 }T
\ T{       2 S>D              2 SM/REM ->  0       1 }T
\ T{      -1 S>D             -1 SM/REM ->  0       1 }T
\ T{      -2 S>D             -2 SM/REM ->  0       1 }T
\ T{       7 S>D              3 SM/REM ->  1       2 }T
\ T{       7 S>D             -3 SM/REM ->  1      -2 }T
\ T{      -7 S>D              3 SM/REM ->  1      -2 }T
\ T{      -7 S>D             -3 SM/REM -> -1       2 }T
\ T{ MAX-INT S>D              1 SM/REM ->  0 MAX-INT }T
\ T{ MIN-INT S>D              1 SM/REM ->  0 MIN-INT }T
\ T{ MAX-INT S>D        MAX-INT SM/REM ->  0       1 }T
\ T{ MIN-INT S>D        MIN-INT SM/REM ->  0       1 }T
\ T{      1S 1                4 SM/REM ->  3 MAX-INT }T
\ T{       2 MIN-INT M*       2 SM/REM ->  0 MIN-INT }T
\ T{       2 MIN-INT M* MIN-INT SM/REM ->  0       2 }T
\ T{       2 MAX-INT M*       2 SM/REM ->  0 MAX-INT }T
\ T{       2 MAX-INT M* MAX-INT SM/REM ->  0       2 }T
\ T{ MIN-INT MIN-INT M* MIN-INT SM/REM ->  0 MIN-INT }T
\ T{ MIN-INT MAX-INT M* MIN-INT SM/REM ->  0 MAX-INT }T
\ T{ MIN-INT MAX-INT M* MAX-INT SM/REM ->  0 MIN-INT }T
\ T{ MAX-INT MAX-INT M* MAX-INT SM/REM ->  0 MAX-INT }T

\ F.6.1.2370 UM/MOD TODO
\ T{        0            0        1 UM/MOD -> 0        0 }T
\ T{        1            0        1 UM/MOD -> 0        1 }T
\ T{        1            0        2 UM/MOD -> 1        0 }T
\ T{        3            0        2 UM/MOD -> 1        1 }T
\ T{ MAX-UINT        2 UM*        2 UM/MOD -> 0 MAX-UINT }T
\ T{ MAX-UINT        2 UM* MAX-UINT UM/MOD -> 0        2 }T
\ T{ MAX-UINT MAX-UINT UM* MAX-UINT UM/MOD -> 0 MAX-UINT }T

\ As the standard allows a system to provide either floored or symmetric division, the remaining
\ operators have to be tested depending on the system behaviour. Two words are defined that provide a
\ form of conditional compilation.

: IFFLOORED [ -3 2 / -2 = INVERT ] LITERAL IF POSTPONE \ THEN ;
: IFSYM     [ -3 2 / -1 = INVERT ] LITERAL IF POSTPONE \ THEN ;

\ IFSYM will ignore the rest of the line when it is performed on a system with floored division and
\ perform the line on a system with symmetric division. IFFLOORED is the direct inverse, ignoring the
\ rest of the line on systems with symmetric division and processing it on systems with floored division.

\ The remaining division operators are tested by defining a version of the operator using words which have
\ already been tested (S>D, M*, FM/MOD and SM/REM). The test definition handles the special case of
\ differing signs. As the test definitions use the words which have just been tested, the tests must be
\ performed in the order: F.6.1.0240 /MOD, F.6.1.0230 /, F.6.1.1890 MOD, F.6.1.0100 */, and
\ F.6.1.0110 */MOD.

\ F.6.1.0240 /MOD TODO
\ IFFLOORED    : T/MOD >R S>D R> FM/MOD ;
\ IFSYM        : T/MOD >R S>D R> SM/REM ;
\ T{       0       1 /MOD ->       0       1 T/MOD }T
\ T{       1       1 /MOD ->       1       1 T/MOD }T
\ T{       2       1 /MOD ->       2       1 T/MOD }T
\ T{      -1       1 /MOD ->      -1       1 T/MOD }T
\ T{      -2       1 /MOD ->      -2       1 T/MOD }T
\ T{       0      -1 /MOD ->       0      -1 T/MOD }T
\ T{       1      -1 /MOD ->       1      -1 T/MOD }T
\ T{       2      -1 /MOD ->       2      -1 T/MOD }T
\ T{      -1      -1 /MOD ->      -1      -1 T/MOD }T
\ T{      -2      -1 /MOD ->      -2      -1 T/MOD }T
\ T{       2       2 /MOD ->       2       2 T/MOD }T
\ T{      -1      -1 /MOD ->      -1      -1 T/MOD }T
\ T{      -2      -2 /MOD ->      -2      -2 T/MOD }T
\ T{       7       3 /MOD ->       7       3 T/MOD }T
\ T{       7      -3 /MOD ->       7      -3 T/MOD }T
\ T{      -7       3 /MOD ->      -7       3 T/MOD }T
\ T{      -7      -3 /MOD ->      -7      -3 T/MOD }T
\ T{ MAX-INT       1 /MOD -> MAX-INT       1 T/MOD }T
\ T{ MIN-INT       1 /MOD -> MIN-INT       1 T/MOD }T
\ T{ MAX-INT MAX-INT /MOD -> MAX-INT MAX-INT T/MOD }T
\ T{ MIN-INT MIN-INT /MOD -> MIN-INT MIN-INT T/MOD }T

\ F.6.1.0230 / TODO
\ IFFLOORED    : T/ T/MOD SWAP DROP ;
\ IFSYM        : T/ T/MOD SWAP DROP ;
\ T{       0       1 / ->       0       1 T/ }T
\ T{       1       1 / ->       1       1 T/ }T
\ T{       2       1 / ->       2       1 T/ }T
\ T{      -1       1 / ->      -1       1 T/ }T
\ T{      -2       1 / ->      -2       1 T/ }T
\ T{       0      -1 / ->       0      -1 T/ }T
\ T{       1      -1 / ->       1      -1 T/ }T
\ T{       2      -1 / ->       2      -1 T/ }T
\ T{      -1      -1 / ->      -1      -1 T/ }T
\ T{      -2      -1 / ->      -2      -1 T/ }T
\ T{       2       2 / ->       2       2 T/ }T
\ T{      -1      -1 / ->      -1      -1 T/ }T
\ T{      -2      -2 / ->      -2      -2 T/ }T
\ T{       7       3 / ->       7       3 T/ }T
\ T{       7      -3 / ->       7      -3 T/ }T
\ T{      -7       3 / ->      -7       3 T/ }T
\ T{      -7      -3 / ->      -7      -3 T/ }T
\ T{ MAX-INT       1 / -> MAX-INT       1 T/ }T
\ T{ MIN-INT       1 / -> MIN-INT       1 T/ }T
\ T{ MAX-INT MAX-INT / -> MAX-INT MAX-INT T/ }T
\ T{ MIN-INT MIN-INT / -> MIN-INT MIN-INT T/ }T

\ F.6.1.1890 MOD TODO
\ IFFLOORED    : TMOD T/MOD DROP ;
\ IFSYM        : TMOD T/MOD DROP ;
\ T{       0       1 MOD ->       0       1 TMOD }T
\ T{       1       1 MOD ->       1       1 TMOD }T
\ T{       2       1 MOD ->       2       1 TMOD }T
\ T{      -1       1 MOD ->      -1       1 TMOD }T
\ T{      -2       1 MOD ->      -2       1 TMOD }T
\ T{       0      -1 MOD ->       0      -1 TMOD }T
\ T{       1      -1 MOD ->       1      -1 TMOD }T
\ T{       2      -1 MOD ->       2      -1 TMOD }T
\ T{      -1      -1 MOD ->      -1      -1 TMOD }T
\ T{      -2      -1 MOD ->      -2      -1 TMOD }T
\ T{       2       2 MOD ->       2       2 TMOD }T
\ T{      -1      -1 MOD ->      -1      -1 TMOD }T
\ T{      -2      -2 MOD ->      -2      -2 TMOD }T
\ T{       7       3 MOD ->       7       3 TMOD }T
\ T{       7      -3 MOD ->       7      -3 TMOD }T
\ T{      -7       3 MOD ->      -7       3 TMOD }T
\ T{      -7      -3 MOD ->      -7      -3 TMOD }T
\ T{ MAX-INT       1 MOD -> MAX-INT       1 TMOD }T
\ T{ MIN-INT       1 MOD -> MIN-INT       1 TMOD }T
\ T{ MAX-INT MAX-INT MOD -> MAX-INT MAX-INT TMOD }T
\ T{ MIN-INT MIN-INT MOD -> MIN-INT MIN-INT TMOD }T

\ F.6.1.0100 */ TODO
\ IFFLOORED    : T*/ T*/MOD SWAP DROP ;
\ IFSYM        : T*/ T*/MOD SWAP DROP ;
\ T{       0 2       1 */ ->       0 2       1 T*/ }T
\ T{       1 2       1 */ ->       1 2       1 T*/ }T
\ T{       2 2       1 */ ->       2 2       1 T*/ }T
\ T{      -1 2       1 */ ->      -1 2       1 T*/ }T
\ T{      -2 2       1 */ ->      -2 2       1 T*/ }T
\ T{       0 2      -1 */ ->       0 2      -1 T*/ }T
\ T{       1 2      -1 */ ->       1 2      -1 T*/ }T
\ T{       2 2      -1 */ ->       2 2      -1 T*/ }T
\ T{      -1 2      -1 */ ->      -1 2      -1 T*/ }T
\ T{      -2 2      -1 */ ->      -2 2      -1 T*/ }T
\ T{       2 2       2 */ ->       2 2       2 T*/ }T
\ T{      -1 2      -1 */ ->      -1 2      -1 T*/ }T
\ T{      -2 2      -2 */ ->      -2 2      -2 T*/ }T
\ T{       7 2       3 */ ->       7 2       3 T*/ }T
\ T{       7 2      -3 */ ->       7 2      -3 T*/ }T
\ T{      -7 2       3 */ ->      -7 2       3 T*/ }T
\ T{      -7 2      -3 */ ->      -7 2      -3 T*/ }T
\ T{ MAX-INT 2 MAX-INT */ -> MAX-INT 2 MAX-INT T*/ }T
\ T{ MIN-INT 2 MIN-INT */ -> MIN-INT 2 MIN-INT T*/ }T

\ F.6.1.0110 */MOD TODO
\ IFFLOORED    : T*/MOD >R M* R> FM/MOD ;
\ IFSYM        : T*/MOD >R M* R> SM/REM ;
\ T{       0 2       1 */MOD ->       0 2       1 T*/MOD }T
\ T{       1 2       1 */MOD ->       1 2       1 T*/MOD }T
\ T{       2 2       1 */MOD ->       2 2       1 T*/MOD }T
\ T{      -1 2       1 */MOD ->      -1 2       1 T*/MOD }T
\ T{      -2 2       1 */MOD ->      -2 2       1 T*/MOD }T
\ T{       0 2      -1 */MOD ->       0 2      -1 T*/MOD }T
\ T{       1 2      -1 */MOD ->       1 2      -1 T*/MOD }T
\ T{       2 2      -1 */MOD ->       2 2      -1 T*/MOD }T
\ T{      -1 2      -1 */MOD ->      -1 2      -1 T*/MOD }T
\ T{      -2 2      -1 */MOD ->      -2 2      -1 T*/MOD }T
\ T{       2 2       2 */MOD ->       2 2       2 T*/MOD }T
\ T{      -1 2      -1 */MOD ->      -1 2      -1 T*/MOD }T
\ T{      -2 2      -2 */MOD ->      -2 2      -2 T*/MOD }T
\ T{       7 2       3 */MOD ->       7 2       3 T*/MOD }T
\ T{       7 2      -3 */MOD ->       7 2      -3 T*/MOD }T
\ T{      -7 2       3 */MOD ->      -7 2       3 T*/MOD }T
\ T{      -7 2      -3 */MOD ->      -7 2      -3 T*/MOD }T
\ T{ MAX-INT 2 MAX-INT */MOD -> MAX-INT 2 MAX-INT T*/MOD }T
\ T{ MIN-INT 2 MIN-INT */MOD -> MIN-INT 2 MIN-INT T*/MOD }T

\ F.3.11 Memory
\ As with the other sections, the tests for the memory access words build on previously tested words and
\ thus require an order to the testing.

\ The first test (F.6.1.0150 , (comma)) tests HERE, the single cell memory access words @, ! and CELL+
\ as well as the double cell access words 2@ and 2!. The tests F.6.1.0130 +! and F.6.1.0890 CELLS should
\ then be performed.

\ F.6.1.0150 ,
HERE 1 ,
HERE 2 ,
CONSTANT 2ND
CONSTANT 1ST
T{       1ST 2ND U< -> <TRUE> }T \ HERE MUST GROW WITH ALLOT
T{       1ST CELL+  -> 2ND }T \ ... BY ONE CELL
T{   1ST 1 CELLS +  -> 2ND }T
T{     1ST @ 2ND @  -> 1 2 }T
T{         5 1ST !  ->     }T
T{     1ST @ 2ND @  -> 5 2 }T
T{         6 2ND !  ->     }T
T{     1ST @ 2ND @  -> 5 6 }T
T{           1ST 2@ -> 6 5 }T
T{       2 1 1ST 2! ->     }T
T{           1ST 2@ -> 2 1 }T
T{ 1S 1ST !  1ST @  -> 1S  }T    \ CAN STORE CELL-WIDE VALUE

\ F.6.1.0130 +!
T{  0 1ST !        ->   }T
T{  1 1ST +!       ->   }T
T{    1ST @        -> 1 }T
T{ -1 1ST +! 1ST @ -> 0 }T

\ F.6.1.0890 CELLS
: BITS ( X -- U )
   0 SWAP BEGIN DUP WHILE
     DUP MSB AND IF >R 1+ R> THEN 2*
   REPEAT DROP ;
( CELLS >= 1 AU, INTEGRAL MULTIPLE OF CHAR SIZE, >= 16 BITS )
T{ 1 CELLS 1 <         -> <FALSE> }T
T{ 1 CELLS 1 CHARS MOD ->    0    }T
T{ 1S BITS 10 <        -> <FALSE> }T

\ The test (F.6.1.0860 C,) also tests the single character memory words C@, C!, and CHAR+, leaving the
\ test F.6.1.0898 CHARS to be performed separately.

\ F.6.1.0860 C,
HERE 1 C,
HERE 2 C,
CONSTANT 2NDC
CONSTANT 1STC
T{    1STC 2NDC U< -> <TRUE> }T \ HERE MUST GROW WITH ALLOT
T{      1STC CHAR+ ->  2NDC  }T \ ... BY ONE CHAR
T{  1STC 1 CHARS + ->  2NDC  }T
T{ 1STC C@ 2NDC C@ ->   1 2  }T
T{       3 1STC C! ->        }T
T{ 1STC C@ 2NDC C@ ->   3 2  }T
T{       4 2NDC C! ->        }T
T{ 1STC C@ 2NDC C@ ->   3 4  }T

\ F.6.1.0898 CHARS
( CHARACTERS >= 1 AU, <= SIZE OF CELL, >= 8 BITS )
T{ 1 CHARS 1 <       -> <FALSE> }T
T{ 1 CHARS 1 CELLS > -> <FALSE> }T
( TBD: HOW TO FIND NUMBER OF BITS? )

\ Finally, the memory access alignment test F.6.1.0705 ALIGN includes a test of ALIGNED, leaving
\ F.6.1.0710 ALLOT as the final test in this group.

\ F.6.1.0705 ALIGN
ALIGN 1 ALLOT HERE ALIGN HERE 3 CELLS ALLOT
CONSTANT A-ADDR CONSTANT UA-ADDR
T{ UA-ADDR ALIGNED -> A-ADDR }T
T{       1 A-ADDR C!         A-ADDR       C@ ->       1 }T
T{    1234 A-ADDR !          A-ADDR       @  ->    1234 }T
T{ 123 456 A-ADDR 2!         A-ADDR       2@ -> 123 456 }T
T{       2 A-ADDR CHAR+ C!   A-ADDR CHAR+ C@ ->       2 }T
T{       3 A-ADDR CELL+ C!   A-ADDR CELL+ C@ ->       3 }T
T{    1234 A-ADDR CELL+ !    A-ADDR CELL+ @  ->    1234 }T
T{ 123 456 A-ADDR CELL+ 2!   A-ADDR CELL+ 2@ -> 123 456 }T

\ F.6.1.0710 ALLOT
HERE 1 ALLOT
HERE
CONSTANT 2NDA
CONSTANT 1STA
T{ 1STA 2NDA U< -> <TRUE> }T    \ HERE MUST GROW WITH ALLOT
T{      1STA 1+ ->   2NDA }T    \ ... BY ONE ADDRESS UNIT
( MISSING TEST: NEGATIVE ALLOT )

\ F.3.12 Characters
\ Basic character handling: F.6.1.0770 BL, F.6.1.0895 CHAR, F.6.1.2520 [CHAR], F.6.1.2500 [ which
\ also tests ], and F.6.1.2165 S".

\ F.6.1.0770 BL TODO hex
\ T{ BL -> 20 }T TODO hex
T{ BL -> 32 }T

\ F.6.1.0895 CHAR TODO hex
\ T{ CHAR X     -> 58 }T TODO hex
\ T{ CHAR HELLO -> 48 }T TODO hex
T{ CHAR X     -> 88 }T
T{ CHAR HELLO -> 72 }T

\ F.6.1.2520 [CHAR] TODO hex
T{ : GC1 [CHAR] X     ; -> }T
T{ : GC2 [CHAR] HELLO ; -> }T
\ T{ GC1 -> 58 }T TODO hex
\ T{ GC2 -> 48 }T TODO hex
T{ GC1 -> 88 }T
T{ GC2 -> 72 }T

\ F.6.1.2500 [ TODO hex
T{ : GC3 [ GC1 ] LITERAL ; -> }T
\ T{ GC3 -> 58 }T TODO hex
T{ GC3 -> 88 }T

\ F.6.1.2165 S" TODO hex
T{ : GC4 S" XY" ; ->   }T
T{ GC4 SWAP DROP  -> 2 }T
\ T{ GC4 DROP DUP C@ SWAP CHAR+ C@ -> 58 59 }T \ TODO hex
T{ GC4 DROP DUP C@ SWAP CHAR+ C@ -> 88 89 }T \ TODO hex
: GC5 S" A String"2DROP ; \ There is no space between the " and 2DROP
T{ GC5 -> }T

\ F.3.13 Dictionary
\ The dictionary tests define a number of words as part of the test, these are included in the appropriate
\ test: F.6.1.0070 ', F.6.1.2510 ['] both of which also test EXECUTE, F.6.1.1550 FIND, F.6.1.1780 LITERAL,
\ F.6.1.0980 COUNT, F.6.1.2033 POSTPONE, F.6.1.2250 STATE

\ F.6.1.0070 '
T{ : GT1 123 ;   ->     }T
T{ ' GT1 EXECUTE -> 123 }T

\ F.6.1.2510 [']
T{ : GT2 ['] GT1 ; IMMEDIATE -> }T
T{ GT2 EXECUTE -> 123 }T

\ F.6.1.1550 FIND
HERE 3 C, CHAR G C, CHAR T C, CHAR 1 C, CONSTANT GT1STRING
HERE 3 C, CHAR G C, CHAR T C, CHAR 2 C, CONSTANT GT2STRING
T{ GT1STRING FIND -> ' GT1 -1 }T
T{ GT2STRING FIND -> ' GT2 1  }T
( HOW TO SEARCH FOR NON-EXISTENT WORD? )

\ F.6.1.1780 LITERAL
T{ : GT3 GT2 LITERAL ; -> }T
T{ GT3 -> ' GT1 }T

\ F.6.1.0980 COUNT
T{ GT1STRING COUNT -> GT1STRING CHAR+ 3 }T

\ F.6.1.2033 POSTPONE
\ T{ : GT4 POSTPONE GT1 ; IMMEDIATE -> }T \ TODO BUG
\ T{ : GT5 GT4 ; -> }T
\ T{ GT5 -> 123 }T \ TODO BUG
\ T{ : GT6 345 ; IMMEDIATE -> }T
\ T{ : GT7 POSTPONE GT6 ; -> }T
\ T{ GT7 -> 345 }T

\ F.6.1.2250 STATE
T{ : GT8 STATE @ ; IMMEDIATE -> }T
T{ GT8 -> 0 }T
T{ : GT9 GT8 LITERAL ; -> }T
T{ GT9 0= -> <FALSE> }T

\ F.3.14 Flow Control
\ The flow control words have to be tested in matching groups. First test F.6.1.1700 IF, ELSE, THEN group.
\ Followed by the BEGIN, F.6.1.2430 WHILE, REPEAT group, and the BEGIN, F.6.1.2390 UNTIL pairing. Finally
\ the F.6.1.2120 RECURSE function should be tested.

\ F.6.1.1700 IF
T{ : GI1 IF 123 THEN ; -> }T
T{ : GI2 IF 123 ELSE 234 THEN ; -> }T
T{  0 GI1 ->     }T
T{  1 GI1 -> 123 }T
T{ -1 GI1 -> 123 }T
T{  0 GI2 -> 234 }T
T{  1 GI2 -> 123 }T
T{ -1 GI1 -> 123 }T
\ Multiple ELSEs in an IF statement
: melse IF 1 ELSE 2 ELSE 3 ELSE 4 ELSE 5 THEN ;
T{ <FALSE> melse -> 2 4 }T
T{ <TRUE>  melse -> 1 3 5 }T

\ F.6.1.2430 WHILE
T{ : GI3 BEGIN DUP 5 < WHILE DUP 1+ REPEAT ; -> }T
T{ 0 GI3 -> 0 1 2 3 4 5 }T
T{ 4 GI3 -> 4 5 }T
T{ 5 GI3 -> 5 }T
T{ 6 GI3 -> 6 }T
T{ : GI5 BEGIN DUP 2 > WHILE 
      DUP 5 < WHILE DUP 1+ REPEAT 
      123 ELSE 345 THEN ; -> }T
T{ 1 GI5 -> 1 345 }T
T{ 2 GI5 -> 2 345 }T
T{ 3 GI5 -> 3 4 5 123 }T
T{ 4 GI5 -> 4 5 123 }T
T{ 5 GI5 -> 5 123 }T

\ F.6.1.2390 UNTIL
T{ : GI4 BEGIN DUP 1+ DUP 5 > UNTIL ; -> }T
T{ 3 GI4 -> 3 4 5 6 }T
T{ 5 GI4 -> 5 6 }T
T{ 6 GI4 -> 6 7 }T

\ F.6.1.2120RECURSE
T{ : GI6 ( N -- 0,1,..N ) 
     DUP IF DUP >R 1- RECURSE R> THEN ; -> }T
T{ 0 GI6 -> 0 }T
T{ 1 GI6 -> 0 1 }T
T{ 2 GI6 -> 0 1 2 }T
T{ 3 GI6 -> 0 1 2 3 }T
T{ 4 GI6 -> 0 1 2 3 4 }T
DECIMAL
\ T{ :NONAME ( n -- 0, 1, .., n ) \ TODO :noname
\      DUP IF DUP >R 1- RECURSE R> THEN 
\    ; 
\    CONSTANT rn1 -> }T
\ T{ 0 rn1 EXECUTE -> 0 }T
\ T{ 4 rn1 EXECUTE -> 0 1 2 3 4 }T

\ :NONAME ( n -- n1 ) \ TODO :noname
\    1- DUP
\    CASE 0 OF EXIT ENDOF
\      1 OF 11 SWAP RECURSE ENDOF
\      2 OF 22 SWAP RECURSE ENDOF
\      3 OF 33 SWAP RECURSE ENDOF
\      DROP ABS RECURSE EXIT
\    ENDCASE
\ ; CONSTANT rn2

\ T{  1 rn2 EXECUTE -> 0 }T
\ T{  2 rn2 EXECUTE -> 11 0 }T
\ T{  4 rn2 EXECUTE -> 33 22 11 0 }T
\ T{ 25 rn2 EXECUTE -> 33 22 11 0 }T

\ F.3.15 Counted Loops
\ Counted loops have a set of special condition that require testing. As with the flow control words,
\ these words have to be tested as a group. First the basic counted loop: DO; I; F.6.1.1800 LOOP,
\ followed by loops with a non regular increment: F.6.1.0140 +LOOP, loops within loops: F.6.1.1730 J,
\ and aborted loops: F.6.1.1760 LEAVE; F.6.1.2380 UNLOOP which includes a test for EXIT.

\ F.6.1.1800 LOOP
T{ : GD1 DO I LOOP ; -> }T
T{          4        1 GD1 ->  1 2 3   }T
T{          2       -1 GD1 -> -1 0 1   }T
T{ MID-UINT+1 MID-UINT GD1 -> MID-UINT }T

\ F.6.1.0140 +LOOP
T{ : GD2 DO I -1 +LOOP ; -> }T
\ T{        1          4 GD2 -> 4 3 2  1 }T \ TODO BUG
\ T{       -1          2 GD2 -> 2 1 0 -1 }T \ TODO BUG
\ T{ MID-UINT MID-UINT+1 GD2 -> MID-UINT+1 MID-UINT }T \ TODO BUG
VARIABLE gditerations
VARIABLE gdincrement

: gd7 ( limit start increment -- )
   gdincrement !
   0 gditerations !
   DO
     1 gditerations +!
     I
     gditerations @ 6 = IF LEAVE THEN
     gdincrement @
   +LOOP gditerations @
;

\ T{    4  4  -1 gd7 ->  4                  1  }T \ TODO BUG
\ T{    1  4  -1 gd7 ->  4  3  2  1         4  }T \ TODO BUG
T{    4  1  -1 gd7 ->  1  0 -1 -2  -3  -4 6  }T
T{    4  1   0 gd7 ->  1  1  1  1   1   1 6  }T
\ T{    0  0   0 gd7 ->  0  0  0  0   0   0 6  }T \ TODO BUG
\ T{    1  4   0 gd7 ->  4  4  4  4   4   4 6  }T \ TODO BUG
\ T{    1  4   1 gd7 ->  4  5  6  7   8   9 6  }T \ TODO BUG
T{    4  1   1 gd7 ->  1  2  3            3  }T
\ T{    4  4   1 gd7 ->  4  5  6  7   8   9 6  }T \ TODO BUG
T{    2 -1  -1 gd7 -> -1 -2 -3 -4  -5  -6 6  }T
\ T{   -1  2  -1 gd7 ->  2  1  0 -1         4  }T \ TODO BUG
T{    2 -1   0 gd7 -> -1 -1 -1 -1  -1  -1 6  }T
\ T{   -1  2   0 gd7 ->  2  2  2  2   2   2 6  }T \ TODO BUG
\ T{   -1  2   1 gd7 ->  2  3  4  5   6   7 6  }T \ TODO BUG
T{    2 -1   1 gd7 -> -1 0 1              3  }T
\ T{  -20 30 -10 gd7 -> 30 20 10  0 -10 -20 6  }T \ TODO BUG
\ T{  -20 31 -10 gd7 -> 31 21 11  1  -9 -19 6  }T \ TODO BUG
\ T{  -20 29 -10 gd7 -> 29 19  9 -1 -11     5  }T \ TODO BUG

\ With large and small increments

MAX-UINT 8 RSHIFT 1+ CONSTANT ustep
ustep NEGATE CONSTANT -ustep
MAX-INT 7 RSHIFT 1+ CONSTANT step
step NEGATE CONSTANT -step

VARIABLE bump

T{  : gd8 bump ! DO 1+ bump @ +LOOP ; -> }T

\ T{  0 MAX-UINT 0 ustep gd8 -> 256 }T \ TODO BUG
\ T{  0 0 MAX-UINT -ustep gd8 -> 256 }T \ TODO BUG
\ T{  0 MAX-INT MIN-INT step gd8 -> 256 }T \ TODO BUG
\ T{  0 MIN-INT MAX-INT -step gd8 -> 256 }T \ TODO BUG

\ F.6.1.1730 J
T{ : GD3 DO 1 0 DO J LOOP LOOP ; -> }T
T{          4        1 GD3 ->  1 2 3   }T
T{          2       -1 GD3 -> -1 0 1   }T
T{ MID-UINT+1 MID-UINT GD3 -> MID-UINT }T
T{ : GD4 DO 1 0 DO J LOOP -1 +LOOP ; -> }T
\ T{        1          4 GD4 -> 4 3 2 1             }T \ TODO BUG
\ T{       -1          2 GD4 -> 2 1 0 -1            }T \ TODO BUG
\ T{ MID-UINT MID-UINT+1 GD4 -> MID-UINT+1 MID-UINT }T \ TODO BUG

\ F.6.1.1760 LEAVE
T{ : GD5 123 SWAP 0 DO 
     I 4 > IF DROP 234 LEAVE THEN 
   LOOP ; -> }T
T{ 1 GD5 -> 123 }T
T{ 5 GD5 -> 123 }T
T{ 6 GD5 -> 234 }T

\ F.6.1.2380 UNLOOP
T{ : GD6 ( PAT: {0 0},{0 0}{1 0}{1 1},{0 0}{1 0}{1 1}{2 0}{2 1}{2 2} ) 
      0 SWAP 0 DO 
         I 1+ 0 DO 
           I J + 3 = IF I UNLOOP I UNLOOP EXIT THEN 1+ 
         LOOP 
      LOOP ; -> }T
T{ 1 GD6 -> 1 }T
T{ 2 GD6 -> 3 }T
T{ 3 GD6 -> 4 1 2 }T

\ F.3.16 Defining Words
\ Although most of the defining words have already been used within the test suite, they still need to be
\ tested fully. The tests include F.6.1.0450 : which also tests ;, F.6.1.0950 CONSTANT, F.6.1.2410 VARIABLE,
\ F.6.1.1250 DOES> which includes tests CREATE, and F.6.1.0550 >BODY which also tests CREATE.

\ F.6.1.0450 :
T{ : NOP : POSTPONE ; ; -> }T
T{ NOP NOP1 NOP NOP2 -> }T
T{ NOP1 -> }T
T{ NOP2 -> }T

\ The following tests the dictionary search order:
\ T{ : GDX   123 ;    : GDX   GDX 234 ; -> }T \ TODO BUG need to support definition in terms of same name
\ T{ GDX -> 123 234 }T \ TODO BUG

\ F.6.1.0950 CONSTANT
T{ 123 CONSTANT X123 -> }T
T{ X123 -> 123 }T
T{ : EQU CONSTANT ; -> }T
T{ X123 EQU Y123 -> }T
T{ Y123 -> 123 }T

\ F.6.1.2410 VARIABLE
T{ VARIABLE V1 ->     }T
T{    123 V1 ! ->     }T
T{        V1 @ -> 123 }T

\ F.6.1.1250 DOES>
T{ : DOES1 DOES> @ 1 + ; -> }T
T{ : DOES2 DOES> @ 2 + ; -> }T
T{ CREATE CR1 -> }T
T{ CR1   -> HERE }T
T{ 1 ,   ->   }T
T{ CR1 @ -> 1 }T
T{ DOES1 ->   }T
T{ CR1   -> 2 }T
T{ DOES2 ->   }T
T{ CR1   -> 3 }T
T{ : WEIRD: CREATE DOES> 1 + DOES> 2 + ; -> }T
T{ WEIRD: W1 -> }T
T{ ' W1 >BODY -> HERE }T
T{ W1 -> HERE 1 + }T
T{ W1 -> HERE 2 + }T

\ F.6.1.0550 >BODY
T{  CREATE CR0 ->      }T
T{ ' CR0 >BODY -> HERE }T

\ F.3.17 Evaluate
\ As with the defining words, F.6.1.1360 EVALUATE has already been used, but it must still be tested fully.

\ F.6.1.1360 EVALUATE
: GE1 S" 123" ; IMMEDIATE
: GE2 S" 123 1+" ; IMMEDIATE
: GE3 S" : GE4 345 ;" ;
: GE5 EVALUATE ; IMMEDIATE
\ T{ GE1 EVALUATE -> 123 }T ( TEST EVALUATE IN INTERP. STATE ) \ TODO BUG
\ T{ GE2 EVALUATE -> 124 }T \ TODO BUG
\ T{ GE3 EVALUATE ->     }T \ TODO BUG
\ T{ GE4          -> 345 }T \ TODO BUG

\ T{ : GE6 GE1 GE5 ; -> }T ( TEST EVALUATE IN COMPILE STATE ) \ TODO BUG
\ T{ GE6 -> 123 }T \ TODO BUG
\ T{ : GE7 GE2 GE5 ; -> }T \ TODO BUG
\ T{ GE7 -> 124 }T \ TODO BUG

\ F.3.18 Parser Input Source Control
\ Testing of the input source can be quit dificult. The tests require line breaks within the test:
\ F.6.1.2216 SOURCE, F.6.1.0560 >IN, and F.6.1.2450 WORD.

\ F.6.1.2216 SOURCE
: GS1 S" SOURCE" 2DUP EVALUATE >R SWAP >R = R> R> = ;
\ T{ GS1 -> <TRUE> <TRUE> }T \ TODO BUG
: GS4 SOURCE >IN ! DROP ;
T{ GS4 123 456 
    -> }T

\ F.6.1.0560 >IN
VARIABLE SCANS
: RESCAN? -1 SCANS +! SCANS @ IF 0 >IN ! THEN ;
T{   2 SCANS ! 
345 RESCAN? 
-> 345 345 }T

: GS2 5 SCANS ! S" 123 RESCAN?" EVALUATE ;
\ T{ GS2 -> 123 123 123 123 123 }T \ TODO BUG

\ These tests must start on a new line
DECIMAL
\ T{ 123456 DEPTH OVER 9 < 35 AND + 3 + >IN !
\ -> 123456 23456 3456 456 56 6 }T \ TODO BUG
T{ 14145 8115 ?DUP 0= 34 AND >IN +! TUCK MOD 14 >IN ! GCD calculation
-> 15 }T

\ F.6.1.2450 WORD
: GS3 WORD COUNT SWAP C@ ;
T{ BL GS3 HELLO -> 5 CHAR H }T
T{ CHAR " GS3 GOODBYE" -> 7 CHAR G }T
\ T{ BL GS3 
\    DROP -> 0 }T \ Blank lines return zero-length strings \ TODO BUG

.( Tests complete.)
bye
