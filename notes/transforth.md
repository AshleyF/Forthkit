# [TransForth](https://github.com/AshleyF/TransForth)

: HERE   H @ ;
: LATEST   L @ ;
: SP@   S @ ;
: NEGATE   -1 * ;
: -  ( a b -- diff)  NEGATE + ;
: 1+ 1 + ;
: 1- 1 - ;
: DEPTH  ( -- n)  S0 SP@ - ;
: CLEAR   ( --)  S0 1+ S ! ;
: DROP   ( a -- )  SP@ 1+ S ! ;
: , ( v --) HERE ! HERE 1+ H ! ;
: BEGIN   HERE ; IMMEDIATE
: UNTIL   ' 0BRANCH , , ; IMMEDIATE
: PICK   SP@ + 1+ @ ;
: OVER  ( a b -- a b a)  1 PICK ;
: 2DUP  ( a b -- a b a b)  OVER OVER ;
: 2+ 2 + ;
: 2- 2 - ;
: 2* 2 * ;
: 2/ 2 / ;
: DUP  ( a -- a a)  0 PICK ;
: >R   R @ DUP DUP 1- R !  @ R @ !  ! ;
: R>   R @ 1+ @  R @ @ R @ 1+ !  R @ 1+ R ! ;
: R@   R @ 1+ @ ;
: ROLL   SP@ 1+ + DUP @ >R BEGIN DUP >R 1- DUP @ R> ! DUP SP@ 2+ = UNTIL DROP R> SP@ 1+ ! ;
: ?   @ . ;
: ROT  ( a b c -- b c a)  2 ROLL ;
: SWAP  ( a b -- b a)  1 ROLL ;
: +!  ( add a -- )  DUP @  ROT +  SWAP ! ;
: ++! ( a -- a++) DUP @ 1+ SWAP ! ;
: COUNTER   2* 3 + R @ + @ ;
: I   0 COUNTER ;
: J   1 COUNTER ;
: K   2 COUNTER ;
: -ROT  ( a b c -- c a b)  ROT ROT ;
: NIP  ( a b -- b)  SWAP DROP ;
: TUCK  ( a b -- b a b)  SWAP OVER ;
: 2DROP  ( a b -- )  DROP DROP ;
: 3DROP  ( a b c -- ) 2DROP DROP ;
: 2OVER  ( a b c d -- a b c d a b)  3 PICK 3 PICK ;
: 3DUP  ( a b c -- a b c a b c)  DUP 2OVER ROT ;
: SQUARE  ( a -- a^2)  DUP * ;
: CUBE  ( a -- a^3)  DUP DUP * * ;
: /MOD  ( a b -- rem quot)  2DUP MOD -ROT / ;
: TRUE  ( -- t)  -1 ; \ normally constant
: FALSE  ( -- f)  0 ; \ normally constant
: NOT  ( a -- ~a)  DUP NAND ;
: AND  ( a b -- a&b)  NAND NOT ;
: OR  ( a b -- a|b)  NOT SWAP NOT NAND ;
: NOR  ( a b -- ~a|b)  OR NOT ;
: XOR  ( a b -- a^b)  2DUP AND -ROT NOR NOR ;
: XNOR ( a b -- ~a^b)  XOR NOT ;
: <  ( a b -- a<b)  2DUP > -ROT = OR NOT ;
: <=  ( a b -- a<=b)  2DUP < -ROT = OR ;
: >=  ( a b -- a>=b)  2DUP > -ROT = OR ;
: <>  ( a b -- ?)  = NOT ;
: 0>   0 > ;
: 0=   0 = ;
: 0<   0 < ;
: 0<>   0 <> ;
: IF   ' 0BRANCH , HERE 0 , ; IMMEDIATE
: ELSE   ' BRANCH , HERE 0 , SWAP HERE SWAP ! ; IMMEDIATE
: THEN   HERE SWAP ! ; IMMEDIATE
: ABS  ( n -- |n|)  DUP 0< IF NEGATE THEN ;
: MIN   2DUP > IF SWAP THEN DROP ;
: MAX   2DUP < IF SWAP THEN DROP ;
: WHILE   ' 0BRANCH , HERE 0 , ; IMMEDIATE
: REPEAT   ' BRANCH , HERE 1+ SWAP ! , ; IMMEDIATE
: LEAVE   ' BRANCH , HERE SWAP 0 , ; IMMEDIATE
: DO   HERE ' >R , ' >R , ; IMMEDIATE
: LOOP   ' R> , ' R> , ' 1+ , ' 2DUP , ' = , ' 0BRANCH , , ' 2DROP , ; IMMEDIATE
: +LOOP   ' R> , ' R> , ' ROT , ' + , ' 2DUP , ' = , ' 0BRANCH , , ' 2DROP , ; IMMEDIATE
: .S   SP@ 1- S0 2DUP < IF DO I @ . -1 +LOOP ELSE 2DROP THEN ;
: CRLF 13 ECHO 10 ECHO ;
: SP 32 ;
: DUMP ( m n -- ) DO I . I @ . CRLF LOOP ;
: ?DELIM ( v d -- v ?) 2DUP SP = IF >= ELSE = THEN ;
: ?WS SP ?DELIM ;
: SKIPWS KEY ?WS IF DROP SKIPWS THEN ; \ leaves first non-whitespace char on stack
: TOKEN ( delim -- tok) >R HERE 1+ R@ SP =
    IF SKIPWS ELSE KEY THEN BEGIN
      OVER ! 1+ KEY R@ ?DELIM
    UNTIL R> 2DROP HERE - 1- HERE ! ;
: WORD SP TOKEN ;
: CFA ( addr -- c) 5 + ;
: LINKA ( addr -- l) 4 + ;
: HEADER  WORD   LATEST HERE LINKA !   HERE L !   HERE CFA H ! ;
: FORGET   WORD FIND DUP H !  LINKA @ L ! ;
: TOKENCHARS ( -- b a) HERE HERE @ + 1+ HERE 1+ ;
: 0-ASCII 48 ;
: 9-ASCII 57 ;
: ?DIGIT ( c -- c ?) DUP 0-ASCII >= OVER 9-ASCII <= AND ;
: ?NUMBER 0 TRUE TOKENCHARS DO I @ ?DIGIT SWAP >R AND SWAP 10 * R> + 0-ASCII - SWAP LOOP DUP NOT IF SWAP DROP THEN ;
: ?FOUND ( w -- ?) DUP 0 >= ;
: HIGHBIT -2147483648 ;
: ISIMMEDIATE ( addr -- ?) @ HIGHBIT AND HIGHBIT = ;
: OUTER WORD FIND ?FOUND IF
    DUP ISIMMEDIATE ISINTERACTIVE OR
    IF EXEC ELSE CFA , THEN
  ELSE
    DROP ?NUMBER IF
      ISINTERACTIVE NOT IF LITADDR , , THEN
    ELSE
      63 ECHO SP ECHO \ ?
    THEN
  THEN
  OUTER ;