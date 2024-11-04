# [Standard Words](https://forth-standard.org)

## ALU/Math
- [`+`](https://forth-standard.org/standard/core/Plus)
- [`-`](https://forth-standard.org/standard/core/Minus) (negate +)
- [`*`](https://forth-standard.org/standard/core/Times)
- [`/MOD`](https://forth-standard.org/standard/core/DivMOD)
- [`/`](https://forth-standard.org/standard/core/Div) (/mod swap drop)
- [`MOD`](https://forth-standard.org/standard/core/MOD) (/mod drop)
- [`2*`](https://forth-standard.org/standard/core/TwoTimes)
- [`2/`](https://forth-standard.org/standard/core/TwoDiv)
- [`LSHIFT`](https://forth-standard.org/standard/core/LSHIFT)
- [`RSHIFT`](https://forth-standard.org/standard/core/RSHIFT)
- [`AND`](https://forth-standard.org/standard/core/AND)
- [`OR`](https://forth-standard.org/standard/core/OR)
- [`XOR`](https://forth-standard.org/standard/core/XOR)
- [`INVERT`](https://forth-standard.org/standard/core/INVERT)
- [`1+`](https://forth-standard.org/standard/core/OnePlus)
- [`1-`](https://forth-standard.org/standard/core/OneMinus)
- [`NEGATE`](https://forth-standard.org/standard/core/NEGATE) (0 swap -) (invert 1 +)
- [`ABS`](https://forth-standard.org/standard/core/ABS) (dup 0< if negate then)
- [`UM*`](https://forth-standard.org/standard/core/UMTimes)
- [`UM/MOD`](https://forth-standard.org/standard/core/UMDivMOD)
- [`MIN`](https://forth-standard.org/standard/core/MIN) (2dup < if drop else nip then)
- [`MAX`](https://forth-standard.org/standard/core/MAX) (2dup > if drop else nip then)
- [`*/`](https://forth-standard.org/standard/core/TimesDiv)
- [`*/MOD`](https://forth-standard.org/standard/core/TimesDivMOD)
- [`WITHIN`](https://forth-standard.org/standard/core/WITHIN) (extension) (-rot over <= if > if true else false then else 2drop false then) (over - under - u<)
- [`FM/MOD`](https://forth-standard.org/standard/core/FMDivMOD)
- [`M*`](https://forth-standard.org/standard/core/MTimes)
- [`SM/REM`](https://forth-standard.org/standard/core/SMDivREM)

## Stack
- [`SWAP`](https://forth-standard.org/standard/core/SWAP)
- [`DROP`](https://forth-standard.org/standard/core/DROP)
- [`2DROP`](https://forth-standard.org/standard/core/TwoDROP)
- [`DUP`](https://forth-standard.org/standard/core/DUP)
- [`?DUP`](https://forth-standard.org/standard/core/qDUP)
- [`OVER`](https://forth-standard.org/standard/core/OVER)
- [`2DUP`](https://forth-standard.org/standard/core/TwoDUP)
- [`ROT`](https://forth-standard.org/standard/core/ROT)
- [`2OVER`](https://forth-standard.org/standard/core/TwoOVER)
- [`2SWAP`](https://forth-standard.org/standard/core/TwoSWAP)
- [`NIP`](https://forth-standard.org/standard/core/NIP) (extension) (swap drop)
- [`TUCK`](https://forth-standard.org/standard/core/TUCK) (extension) (swap over)
- [`PICK`](https://forth-standard.org/standard/core/PICK) (extension) (1+ 4 * dsp@ + @)
- [`ROLL`](https://forth-standard.org/standard/core/ROLL) (extension)
- [`DEPTH`](https://forth-standard.org/standard/core/DEPTH) (s0 @ dsp@ - 4-)
- [`>R`](https://forth-standard.org/standard/core/toR)
- [`R>`](https://forth-standard.org/standard/core/Rfrom)
- [`R@`](https://forth-standard.org/standard/core/RFetch)
- [`2>R`](https://forth-standard.org/standard/core/TwotoR) (extension)
- [`2R>`](https://forth-standard.org/standard/core/TwoRfrom) (extension)
- [`2R@`](https://forth-standard.org/standard/core/TwoRFetch) (extension)

## Memory
- [`!`](https://forth-standard.org/standard/core/Store)
- [`+!`](https://forth-standard.org/standard/core/PlusStore)
- [`,`](https://forth-standard.org/standard/core/Comma) (here ! cell allot)
- [`@`](https://forth-standard.org/standard/core/Fetch)
- [`C!`](https://forth-standard.org/standard/core/CStore)
- [`C,`](https://forth-standard.org/standard/core/CComma) (here @ c! 1 here +!) (here c! 1 allot)
- [`C@`](https://forth-standard.org/standard/core/CFetch)
- [`2!`](https://forth-standard.org/standard/core/TwoStore)
- [`2@`](https://forth-standard.org/standard/core/TwoFetch)
- [`ALLOT`](https://forth-standard.org/standard/core/ALLOT) (here @ swap here +!) (dp +!)
- [`HERE`](https://forth-standard.org/standard/core/HERE) (dp @)
- [`UNUSED`](https://forth-standard.org/standard/core/UNUSED) (extension)
- [`ALIGN`](https://forth-standard.org/standard/core/ALIGN) (here @ aligned here !)
- [`ALIGNED`](https://forth-standard.org/standard/core/ALIGNED) (3 + 3 invert and)
- [`MOVE`](https://forth-standard.org/standard/core/MOVE)
- [`CELL+`](https://forth-standard.org/standard/core/CELLPlus)
- [`CELLS`](https://forth-standard.org/standard/core/CELLS) (4 *)

## Numbers
- [`<#`](https://forth-standard.org/standard/core/num-start)
- [`#`](https://forth-standard.org/standard/core/num)
- [`#>`](https://forth-standard.org/standard/core/num-end)
- [`#S`](https://forth-standard.org/standard/core/numS)
- [`SIGN`](https://forth-standard.org/standard/core/SIGN)
- [`>NUMBER`](https://forth-standard.org/standard/core/toNUMBER)
- [`BASE`](https://forth-standard.org/standard/core/BASE)
- [`DECIMAL`](https://forth-standard.org/standard/core/DECIMAL) (10 base !)
- [`HEX`](https://forth-standard.org/standard/core/HEX) (extension) (16 base !)

## I/O
- [`KEY`](https://forth-standard.org/standard/core/KEY) (TODO: Interactive keys)
- [`EMIT`](https://forth-standard.org/standard/core/EMIT)
- [`CR`](https://forth-standard.org/standard/core/CR) (10 emit -- or 13 or 13 10?)
- [`BL`](https://forth-standard.org/standard/core/BL) (32 constant bl)
- [`SPACE`](https://forth-standard.org/standard/core/SPACE) (bl emit)
- [`SPACES`](https://forth-standard.org/standard/core/SPACES) (begin dup 0> while space 1- repeat drop)
- [`.`](https://forth-standard.org/standard/core/d) (0 .r space)
- [`.s`](https://forth-standard.org/standard/tools/DotS) (tools) (dsp@ begin dup s0 @ < while dup @ u. space 4+ repeat drop)
- [`?`](https://forth-standard.org/standard/tools/q) (tools) (@ .)
- [`.R`](https://forth-standard.org/standard/core/DotR) (extension) (swap dup 0< if negate 1 swap rot 1- else 0 swap rot then swap dup uwidth rot swap - spaces swap if '-' emit then u.)
- [`U.R`](https://forth-standard.org/standard/core/UDotR) (extension) (swap dup uwidth rot swap - spaces u.)
- [`."`](https://forth-standard.org/standard/core/Dotq) (immediate state @ if [compile] s" ' tell , else begin key dup '"' = if drop exit then emit again then)
- [`.(](https://forth-standard.org/standard/`core/Dotp) (extension)
- [`U.`](https://forth-standard.org/standard/core/Ud) (base @ /mod ?dup if recurse then dup 10 < if '0' else 10 - 'a' then + emit space)
- [`ACCEPT`](https://forth-standard.org/standard/core/ACCEPT)
- [`TYPE`](https://forth-standard.org/standard/core/TYPE)

## Strings

## Comparison
- [`=`](https://forth-standard.org/standard/core/Equal)
- [`<`](https://forth-standard.org/standard/core/less)
- [`U<`](https://forth-standard.org/standard/core/Uless)
- [`>`](https://forth-standard.org/standard/core/more)
- [`U>`](https://forth-standard.org/standard/core/Umore) (extension)
- [`0<`](https://forth-standard.org/standard/core/Zeroless)
- [`0=`](https://forth-standard.org/standard/core/ZeroEqual)
- [`<>`](https://forth-standard.org/standard/core/ne) (extension)
- [`0<>`](https://forth-standard.org/standard/core/Zerone) (extension)
- [`0>`](https://forth-standard.org/standard/core/Zeromore) (extension)

## Flow
- [`RECURSE`](https://forth-standard.org/standard/core/RECURSE) (immediate latest @ >cfa ,)
- [`TRUE`](https://forth-standard.org/standard/core/TRUE) (extension) (-1)
- [`FALSE`](https://forth-standard.org/standard/core/FALSE) (extension) (0)
- [`IF`](https://forth-standard.org/standard/core/IF) (immediate ' 0branch , here @ 0 ,)
- [`ELSE`](https://forth-standard.org/standard/core/ELSE) (immediate ' branch , here @ 0 , swap dup here @ swap - swap !)
- [`THEN`](https://forth-standard.org/standard/core/THEN) (immediate dup here @ swap - swap !)
- [`BEGIN`](https://forth-standard.org/standard/core/BEGIN) (immediate here @)
- [`UNTIL`](https://forth-standard.org/standard/core/UNTIL) (immediate ' 0branch , here @ - ,)
- [`AGAIN`](https://forth-standard.org/standard/core/AGAIN) (extension) (immediate ' branch , here @ - ,)
- [`WHILE`](https://forth-standard.org/standard/core/WHILE) (immediate ' 0branch , here @ 0 ,)
- [`REPEAT`](https://forth-standard.org/standard/core/REPEAT) (immediate ' branch , swap here @ - , dup here @ swap - swap !)
- [`DO`](https://forth-standard.org/standard/core/DO)
- [`?DO`](https://forth-standard.org/standard/core/qDO) (extension)
- [`LOOP`](https://forth-standard.org/standard/core/LOOP)
- [`+LOOP`](https://forth-standard.org/standard/core/PlusLOOP)
- [`UNLOOP`](https://forth-standard.org/standard/core/UNLOOP)
- [`LEAVE`](https://forth-standard.org/standard/core/LEAVE)
- [`I`](https://forth-standard.org/standard/core/I)
- [`J`](https://forth-standard.org/standard/core/J)

## Interpreter

- [`(`](https://forth-standard.org/standard/core/p) (immediate 1 begin key dup '(' = if drop 1+ else ')' = if 1- then then dup 0= until drop)
- [`\`](https://forth-standard.org/standard/core/bs) (extension)
- [`CONSTANT`](https://forth-standard.org/standard/core/CONSTANT) (word create docol , ' lit , , ' exit ,)
- [`VARIABLE`](https://forth-standard.org/standard/core/VARIABLE) (1 cells allot word create docol , ' lit , , ' exit ,)
- [`LITERAL`](https://forth-standard.org/standard/core/LITERAL) (immediate ' lit , ,)
- [`CREATE`](https://forth-standard.org/standard/core/CREATE) (parse-name header, dovar,)
- [`:`](https://forth-standard.org/standard/core/Colon)
- [`;`](https://forth-standard.org/standard/core/Semi)
- [`IMMEDIATE`](https://forth-standard.org/standard/core/IMMEDIATE)
- [`STATE`](https://forth-standard.org/standard/core/STATE)
- [`'`](https://forth-standard.org/standard/core/Tick)
- [`[`](https://forth-standard.org/standard/core/Bracket)
- [`\]`](https://forth-standard.org/standard/core/right-bracket)
- [`[COMPILE]`](https://forth-standard.org/standard/core/BracketCOMPILE) (extension) (immediate word find >cfa ,)
- [`COMPILE,`](https://forth-standard.org/standard/core/COMPILEComma) (extension)
- [`FIND`](https://forth-standard.org/standard/core/FIND)
- [`EXECUTE`](https://forth-standard.org/standard/core/EXECUTE)
- [`PARSE`](https://forth-standard.org/standard/core/PARSE) (extension)
- [`PARSE-NAME`](https://forth-standard.org/standard/core/PARSE-NAME) (extension)
- [`WORD`](https://forth-standard.org/standard/core/WORD)
- [`QUIT`](https://forth-standard.org/standard/core/QUIT)
- [`ABORT`](https://forth-standard.org/standard/core/ABORT) (0 1- throw)
- [`ABORT"`](https://forth-standard.org/standard/core/ABORTq)
- [`EVALUATE`](https://forth-standard.org/standard/core/EVALUATE)
- [`>BODY`](https://forth-standard.org/standard/core/toBODY)
- [`>IN`](https://forth-standard.org/standard/core/toIN)
- [`CHAR`](https://forth-standard.org/standard/core/CHAR)
- [`CHAR+`](https://forth-standard.org/standard/core/CHARPlus)
- [`CHARS`](https://forth-standard.org/standard/core/CHARS)
- [`COUNT`](https://forth-standard.org/standard/core/COUNT)
- [`DOES>`](https://forth-standard.org/standard/core/DOES)
- [`ENVIRONMENT?`](https://forth-standard.org/standard/core/ENVIRONMENTq)
- [`EXIT`](https://forth-standard.org/standard/core/EXIT)
- [`FILL`](https://forth-standard.org/standard/core/FILL)
- [`HOLD`](https://forth-standard.org/standard/core/HOLD)
- [`POSTPONE`](https://forth-standard.org/standard/core/POSTPONE)
- [`S"`](https://forth-standard.org/standard/core/Sq) (immediate state @ if ' litstring , here @ 0 , begin key dup '"' <> while c, repeat drop dup here @ swap - 4- swap ! align else here @ begin key dup '"' <> while over c! 1+ repeat drop here @ - here @ swap then)
- [`S>D`](https://forth-standard.org/standard/core/StoD)
- [`SOURCE`](https://forth-standard.org/standard/core/SOURCE)
- [`[']`](https://forth-standard.org/standard/core/BracketTick) (immediate ' lit ,)
- [`[CHAR]`](https://forth-standard.org/standard/core/BracketCHAR)
- [`:NONAME`](https://forth-standard.org/standard/core/ColonNONAME) (extension) (0 0 create here @ docol , ])
- [`ACTION-OF`](https://forth-standard.org/standard/core/ACTION-OF) (extension)
- [`BUFFER:`](https://forth-standard.org/standard/core/BUFFERColon) (extension)
- [`C"`](https://forth-standard.org/standard/core/Cq) (extension)
- [`CASE`](https://forth-standard.org/standard/core/CASE) (extension) (immediate 0)
- [`OF`](https://forth-standard.org/standard/core/OF) (extension) (immediate ' over , ' = , [compile] if ' drop ,)
- [`ENDOF`](https://forth-standard.org/standard/core/ENDOF) (extension) (immediate [compile] else)
- [`ENDCASE`](https://forth-standard.org/standard/core/ENDCASE) (extension) (immediate ' drop , begin ?dup while [compile] then repeat)
- [`DEFER`](https://forth-standard.org/standard/core/DEFER) (extension)
- [`DEFER!`](https://forth-standard.org/standard/core/DEFERStore) (extension)
- [`DEFER@`](https://forth-standard.org/standard/core/DEFERFetch) (extension)
- [`ERASE`](https://forth-standard.org/standard/core/ERASE) (extension)
- [`HOLDS`](https://forth-standard.org/standard/core/HOLDS) (extension)
- [`IS`](https://forth-standard.org/standard/core/IS) (extension)
- [`MARKER`](https://forth-standard.org/standard/core/MARKER) (extension)
- [`PAD`](https://forth-standard.org/standard/core/PAD) (extension)
- [`REFILL`](https://forth-standard.org/standard/core/REFILL) (extension)
- [`RESTORE-INPUT`](https://forth-standard.org/standard/core/RESTORE-INPUT) (extension)
- [`S\"`](https://forth-standard.org/standard/core/Seq) (extension)
- [`SAVE-INPUT`](https://forth-standard.org/standard/core/SAVE-INPUT) (extension)
- [`SOURCE-ID`](https://forth-standard.org/standard/core/SOURCE-ID) (extension)
- [`TO`](https://forth-standard.org/standard/core/TO) (extension) (immediate word find >dfa 4+ state @ if ' lit , , ' ! , else ! then)
- [`VALUE`](https://forth-standard.org/standard/core/VALUE) (extension) (word create docol , ' lit , , ' exit ,)


// Jones
: uwidth base @ / ?dup if recurse 1+ else 1 then ;
: +to immediate word find >dfa 4+ state @ if ' lit , , ' +! , else +! then ;
: id. 4+ dup c@ f_lenmask and begin dup 0> while swap 1+ dup c@ emit swap 1- repeat 2drop ;
: ?hidden 4+ c@ f_hidden and ;
: ?immediate 4+ c@ f_immed and ;
: forget word find dup @ latest ! here ! ;
: dump base @ -rot hex begin ?dup while over 8 u.r space 2dup 1- 15 and 1+ begin ?dup while swap dup c@ 2 .r space 1+ swap 1- repeat drop 2dup 1- 15 and 1+ begin ?dup while swap dup c@ dup 32 128 within if emit else drop '.' emit then 1+ swap 1- repeat drop cr dup 1- 15 and 1+ tuck - >r + r> repeat drop base ! ;
: cfa> latest @ begin ?dup while 2dup swap < if nip exit then @ repeat drop 0 ;
: exception-marker rdrop 0 ;
: catch dsp@ 4+ >r ' exception-marker 4+ >r execute ;
: throw ?dup if rsp@ begin dup r0 4- < while dup @ ' exception-marker 4+ = if 4+ rsp! dup dup dup r> 4- swap over ! dsp! exit then 4+ repeat drop case 0 1- of ." aborted" cr endof ." uncaught throw " dup . cr endcase quit then ;
: print-stack-trace rsp@ begin dup r0 4- < while dup @ case ' exception-marker 4+ of ." catch ( dsp=" 4+ dup @ u. ." ) " endof dup cfa> ?dup if 2dup id. [ char + ] literal emit swap >dfa 4+ - . then endcase 4+ repeat drop cr ;
: z" immediate state @ if ' litstring , here @ 0 , begin key dup '"' <> while here @ c! 1 here +! repeat 0 here @ c! 1 here +! drop dup here @ swap - 4- swap ! align ' drop , else here @ begin key dup '"' <> while over c! 1+ repeat drop 0 swap c! here @ then ;
: strlen dup begin dup c@ 0<> while 1+ repeat swap - ;
: cstring swap over here @ swap cmove here @ + 0 swap c! here @ ;
: argc s0 @ @ ;
: argv 1+ cells s0 @ + @ dup strlen	;
: environ argc 2 + cells s0 @ + ;
: get-brk 0 sys_brk syscall1 ;
: brk sys_brk syscall1 ;
: morecore cells get-brk + brk ;
: r/o o_rdonly ;
: r/w o_rdwr ;
: open-file	-rot cstring sys_open syscall2 dup dup 0< if negate else drop 0 then ;
: create-file o_creat or o_trunc or -rot cstring 420 -rot sys_open syscall3 dup dup 0< if negate else drop 0 then ;
: close-file sys_close syscall1 negate ;
: read-file >r swap r> sys_read syscall3 dup dup 0< if negate else drop 0 then ;
: perror tell ':' emit space ." errno=" . cr ;
: eax immediate 0 ;
: ecx immediate 1 ;
: edx immediate 2 ;
: ebx immediate 3 ;
: esp immediate 4 ;
: ebp immediate 5 ;
: esi immediate 6 ;
: edi immediate 7 ;
: push immediate 50 + c, ;
: pop immediate 58 + c, ;
: rdtsc immediate 0f c, 31 c, ;
: rdtsc rdtsc eax push edx push ;code
: =next dup c@ ad <> if drop false exit then 1+ dup c@ ff <> if drop false exit then 1+ c@ 20 <> if false exit then true ;
: welcome s" test-mode" find not if ." jonesforth version " version . cr unused . ." cells remaining" cr ." ok " then ;

: words latest @ begin ?dup while dup ?hidden not if dup id. space then @ repeat cr ;
: see word find here @ latest @ begin 2 pick over <> while nip dup @ repeat drop swap ':' emit space dup id. space dup ?immediate if ." immediate " then >dfa begin 2dup > while dup @ case ' lit of 4 + dup @ . endof ' litstring of [ char s ] literal emit '"' emit space 4 + dup @ swap 4 + swap 2dup tell '"' emit space + aligned 4 - endof ' 0branch of ." 0branch ( " 4 + dup @ . ." ) " endof ' branch of ." branch ( " 4 + dup @ . ." ) " endof ' ' of [ char ' ] literal emit space 4 + dup @ cfa> id. space endof ' exit of 2dup 4 + <> if ." exit " then endof dup cfa> id. space endcase 4 + repeat ';' emit cr 2drop ;
: bye 0 sys_exit syscall1 ;
: unused get-brk here @ - 4 / ;
: (inline) @ begin dup =next not while dup c@ c, 1+ repeat drop ;
: inline immediate word find >cfa dup @ docol = if ." cANNOT inline forth WORDS" cr abort then (inline) ;
: ;code immediate [compile] next align latest @ dup hidden dup >dfa swap >cfa ! [compile] [ ;