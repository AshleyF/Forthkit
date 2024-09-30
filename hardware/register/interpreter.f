( interpreter - reads tokens, compiles headers, literals, ... )
( requires assembler )

( --- register allocation/init ----------------------------------- )

 0 constant n                      ( number parsing - assumed 0 below )
 1 constant x                      ( shared by bootstrap )
 2 constant d                      ( dictionary pointer )
 3 constant m

 4 constant zero                   ( constant 0 )
 5 constant one         1 one ldc, ( constant 1 )                        ( LDC one 1       0000 0500 0100 )
 6 constant two         2 two ldc, ( constant 2 )                        ( LDC two 2       0000 0600 0200 )
 7 constant ten        10 ten ldc, ( number ten decimal by default )     ( LDC ten 10      0000 0700 0A00 )

 8 constant true      -1 true ldc, ( truth value - all bits set )        ( LDC true -1     0000 0800 FFFF )
 9 constant false                  ( false value )

10 constant zeroch  48 zeroch ldc, ( '0' ASCII )                         ( LDC zeroch 48   0000 0A00 3000 )
11 constant rparch  41 rparch ldc, ( right parenthesis ASCII )           ( LDC rparch 41   0000 0B00 2900 )
12 constant spch    32 spch   ldc, ( space ASCII )                       ( LDC spch   32   0000 0C00 2000 )
13 constant negch   45 negch  ldc, ( '-' ASCII )                         ( LDC negch  45   0000 0D00 2D00 )

14 constant c                      ( char last read )
15 constant tib                    ( terminal input buffer )
16 constant len                    ( token length )
17 constant len'                   ( name length )
18 constant nm                     ( match flag for search )
19 constant p                      ( pointer )
20 constant c                      ( char being compared )
21 constant p'                     ( pointer )
22 constant c'                     ( char being compared )
23 constant cur                    ( cursor )
24 constant lnk                    ( link pointer )
25 constant s         32767 s ldc, ( stack pointer )                     ( LDC s 32767     0000 1900 FF7F )
26 constant ldc         1 ldc ldc, ( ldc instruction [0] )
27 constant call      29 call ldc, ( call instruction )                  ( LDC call 26     0000 1B00 1A00 )
28 constant ret        31 ret ldc, ( ret instruction, 28 by luck )       ( LDC ret  28     0000 1C00 1C00 )
29 constant comp                   ( compiling flag )
30 constant sign                   ( number sign while parsing )

ahead,                             ( jump over dictionary )              ( JUMP <addr>     1A00 ADDR )

( --- tokenization ----------------------------------------------- )

            label &skipws          ( skip until non-whitespace )
                c in,              ( read char )                         ( IN c            0400 1400 )
   c spch &skipws ble,             ( keep skipping whitespace )          ( BLE <addr> . .  1800 ADDR 1400 0C00 )
                  ret,                                                   ( RET             1C00 )
      
            label &name            ( read input name into buffer )
            c tib st,              ( append char )                       ( ST tib c        0200 0F00 1400 )
      two tib tib add, ( * )       ( advance tib )                       ( INC tib tib     0600 0F00 0F00 )
          len len inc,             ( increment length )                  ( INC len len     0600 1000 1000 )
                c in,              ( read char )                         ( IN c            0400 1400 )
     c spch &name bgt,             ( continue until whitespace )         ( BGT <addr> . .  1500 ADDR 1400 0C00 )
          len tib st,              ( append length )                     ( ST tib len      0200 0F00 1000 )
                  ret,                                                   ( RET             1C00 )
      
            label &token           ( read token into buffer )
            d tib cp,              ( end of dictionary as buffer )       ( CP tib d        0300 0F00 0200 )
         zero len cp,              ( initial zero length )               ( CP len zero     0300 1000 0400 )
          &skipws call,            ( skip initial whitespace )           ( CALL <addr>     1B00 ADDR )
            &name jump,            ( append name )                       ( JUMP <addr>     1A00 ADDR )

( --- dictionary search ------------------------------------------ )
      
            label &nomatch         ( set no-match flag )
          true nm cp,                                                    ( CP nm true      0300 1200 0800 )
                  ret,                                                   ( RET             1C00 )
      
            label &compcs          ( compare chars to input word )
              p c ld,              ( get char of input word )            ( LD c  p         0100 1400 1300 )
            p' c' ld,              ( get char of dictionary word )       ( LD c' p'        0100 1600 1500 )
    c c' &nomatch bne,             ( don't match? )                      ( BNE <addr> . .  1400 ADDR 1600 1400 )
        two p' p' add, ( * )       ( next char of dictionary word )      ( INC p' p'       0600 1500 1500 )
          two p p add, ( * )       ( next char of input word )           ( INC p  p        0600 1300 1300 )
    p tib &compcs blt,             ( not end of word? continue... )      ( BLT <addr> . .  1700 ADDR 1300 0F00 )
                  ret,             ( we have a match! )                  ( RET             1C00 )
      
            label &nextw           ( advance to next word )
          cur cur ld,              ( follow link address )               ( LD cur cur      0100 1700 1700 )
                                   ( fall through to &comp )
      
            label &comp
zero cur &nomatch beq,             ( no match if start of dict )         ( BEQ <addr> . .  1300 ADDR 1700 0400 )
        tib len p sub,             ( point p at start of token )         ( SUB p tib len   0900 1300 0F00 1000 )
          p len p sub, ( * )       ( point p at start of token )         ( SUB p tib len   0900 1300 0F00 1000 )
           cur p' dec,             ( point p' at length field )          ( DEC p' cur      0700 1500 1700 )
            p' p' dec, ( * )       ( point p' at length field )          ( DEC p' cur      0700 1500 1700 )
          p' len' ld,              ( get length )                        ( LD len' p'      0100 1100 1500 )
  len' len &nextw bne,             ( lengths don't match? )              ( BNE <addr> . .  1400 ADDR 1000 1100 )
        p' len p' sub,             ( move to beginning of word )         ( SUB p' p' len   0900 1500 1500 1000 )
        p' len p' sub, ( * )       ( move to beginning of word )         ( SUB p' p' len   0900 1500 1500 1000 )
         false nm cp,              ( reset no-match flag )               ( CP nm false     0300 1200 0900 )
          &compcs call,            ( compare characters )                ( CALL <addr>     1B00 ADDR )
   true nm &nextw beq,             ( if no match, try next word )        ( BEQ <addr> . .  1300 ADDR 1200 0800 )
                  ret,             ( we have a match! )                  ( RET             1C00 )
                  
            label &find            ( find word [tib within dictionary] )
          lnk cur cp,              ( initialize cursor to last )         ( CP cur lnk      0300 1700 1800 )
 true comp &nextw beq,             ( skip current word if compiling )
            &comp jump,            ( compare with tib )                  ( JUMP <addr>     1A00 ADDR )
      
( --- number processing ------------------------------------------ )

            label &error           ( print invalid word and halt! )
              d c ld,              ( load char of word )                 ( LD c d          0100 1400 0200 )
                c out,             ( output char )                       ( OUT c           0500 1400 )
              d d inc,             ( advance to next char )              ( INC d d         0600 0200 0200 )
              d d inc, ( * )       ( advance to next char )              ( INC d d         0600 0200 0200 )
          len len dec,             ( decrement length )                  ( DEC len len     0700 1000 1000 )
  len zero &error bgt,             ( if more, continue )                 ( BGT <addr> . .  1500 ADDR 1000 0400 )
             63 c ldc,             ( load question mark [63] )           ( LDC c 63        0000 1400 3F00 )
                c out,             ( output question mark )              ( OUT c           0500 1400 )
                  halt,            ( halt! )                             ( HALT            1D00 )

            label &negate          ( set negative sign )
        true sign cp,              ( note: `true` is -1 )                ( CP sign true    0300 1E00 0800 )
              p p inc,             ( next char )                         ( INC p p         0600 1300 1300 )
              p p inc, ( * )       ( next char )                         ( INC p p         0600 1300 1300 )
                                   ( fall through to &digits )

            label &digits          ( parse digits )
              p c ld,              ( get char )                          ( LD c p          0100 1400 1300 )
       c zeroch c sub,             ( convert to digit )                  ( SUB c c zeroch  0900 1400 1400 0A00 )
     c ten &error bge,             ( error if non-digit )                ( BGE <addr> . .  1600 ADDR 1400 0700 )
    c zero &error blt,             ( error if non-digit )                ( BGE <addr> . .  1600 ADDR 1400 0700 )
          n ten n mul,             ( base ten shift left )               ( MUL n ten n     0A00 0000 0700 0000 )
            n c n add,             ( add in one's place )                ( ADD n c n       0800 0000 1400 0000 )
              p p inc,             ( next char )                         ( INC p p         0600 1300 1300 )
              p p inc, ( * )       ( next char )                         ( INC p p         0600 1300 1300 )
    p tib &digits blt,             ( not end? continue... )              ( BLT <addr> . .  1700 ADDR 1300 0F00 )
         sign n n mul,             ( multiply by sign )                  ( MUL n n sign    0A00 0000 0000 1E00 )
                  ret,                                                   ( RET             1C00 )
      
            label &parsenum        ( parse token as number )
        tib len p sub,             ( point p at start of word )          ( SUB p tib len   0900 1300 0F00 1000 )
          p len p sub, ( * )       ( point p at start of word )          ( SUB p tib len   0900 1300 0F00 1000 )
           zero n cp,              ( init number )                       ( CP n zero       0300 0000 0400 )
         one sign cp,              ( init sign )                         ( CP sign one     0300 1E00 0500 )
              p c ld,              ( get char )                          ( LD c p          0100 1400 1300 )
  c negch &negate beq,             ( set negative sign )                 ( BEQ <addr> . .  1300 ADDR 0D00 1400 )
          &digits jump,            ( parse digits )                      ( JUMP <addr>     1A00 ADDR )
      
            label &pushn           ( push interactive number )
              n s st,              ( store n at stack pointer )          ( ST s n          0200 1900 0000 )
              s s dec,             ( adjust stack pointer )              ( DEC s s         0700 1900 1900 )
              s s dec, ( * )       ( adjust stack pointer )              ( DEC s s         0700 1900 1900 )
                  ret,                                                   ( RET             1C00 )
      
            label &popn            ( pop number )
              s s inc,             ( adjust stack pointer )              ( INC s s         0600 1900 1900 )
              s s inc, ( * )       ( adjust stack pointer )              ( INC s s         0600 1900 1900 )
              s n ld,              ( load value )                        ( LD n s          0100 0000 1900 )
                  ret,                                                   ( RET             1C00 )

( --- literals --------------------------------------------------- )

: append, d st, d d inc, d d inc, ( * ) ;         ( macro: append and advance d )

            label &litn            ( compile literal )
              ldc append,          ( append ldc instruction )            ( ST d ldc        0200 0200 1A00 )
                                                                         ( INC d d         0600 0200 0200 )
             zero append,          ( append n register number [0] )      ( ST d zero       0200 0200 0400 )
                                                                         ( INC d d         0600 0200 0200 )
                n append,          ( append value )                      ( ST d n          0200 0200 0000 )
                                                                         ( INC d d         0600 0200 0200 )
             call append,          ( append call instruction )           ( ST d call       0200 0200 1B00 )
                                                                         ( INC d d         0600 0200 0200 )
         &pushn m ldc,             ( load address of &pushn )            ( LDC m <addr>    0000 0300 ADDR )
                m append,          ( append pushn address )              ( ST d m          0200 0200 0300 )
                                                                         ( INC d d         0600 0200 0200 )
                  ret,                                                   ( RET             1C00 )
      
            label &num             ( process token as number )
        &parsenum call,            ( parse number )                      ( CALL <addr>     1B00 ADDR )
  true comp &litn beq,             ( if compiling, compile literal )     ( BEQ <addr> . .  1300 ADDR 1D00 0800 )
           &pushn jump,            ( else, push literal )                ( JUMP <addr>     1A00 ADDR )
      
( --- word handling ----------------------------------------------- )

            label &exec            ( execute word )
        two cur m add,             ( point to code field )               ( ADD m cur two   0800 0300 1700 0600 )
          two m m add, ( * )       ( point to code field )               ( ADD m cur two   0800 0300 1700 0600 )
                m exec,            ( exec word )                         ( EXEC m          1900 0300 )
                  ret,                                                   ( RET             1C00 )
      
            label &compw           ( compile word )
            cur n inc,             ( point to immediate flag )           ( INC n cur       0600 0000 1700 )
              n n inc, ( * )       ( point to immediate flag )           ( INC n cur       0600 0000 1700 )
              n m ld,              ( read immediate flag )               ( LD m n          0100 0300 0000 )
     true m &exec beq,             ( execute if immediate )              ( BEQ <addr> . .  1300 ADDR 0300 0800 )
             call append,          ( append call instruction )           ( ST d call       0200 0200 1B00 )
                                                                         ( INC d d         0600 0200 0200 )
              n n inc,             ( point to code field )               ( INC n n         0600 0000 0000 )
              n n inc, ( * )       ( point to code field )               ( INC n n         0600 0000 0000 )
                n append,          ( append code field address )         ( ST d n          0200 0200 0000 )
                                                                         ( INC d d         0600 0200 0200 )
                  ret,                                                   ( RET             1C00 )
      
            label &word            ( process potential word token )
 true comp &compw beq,             ( if compiling, compile word )        ( BEQ <addr> . .  1300 ADDR 1D00 0800 )
            &exec jump,            ( else, execute word )                ( JUMP <addr>     1A00 F400 )
      
            label &eval            ( process input tokens )
            &find call,            ( try to find in dictionary )         ( CALL <addr>     1B00 ADDR )
     true nm &num beq,             ( if not found, assume number )       ( BEQ <addr> . .  1300 EC00 1200 0800 )
            &word jump,            ( else, process as a word )           ( JUMP <addr>     1A00 1501 )
      
( --- REPL ------------------------------------------------------- )

            label &repl            ( loop forever )
           &token call,            ( read a token )                      ( CALL <addr>     1B00 ADDR )
            &eval call,            ( evaluate it )                       ( CALL <addr>     1B00 ADDR )
            &repl jump,            ( forever )                           ( JUMP <addr>     1A00 2301 )
      
( --- initial dictionary ----------------------------------------- )

variable link
: header, dup 0 do swap , loop , link @ here link ! , , ;

     0 sym create header,          ( word to create words )              ( create6 . 0     6300 7200 6500 6100 7400 6500 0600 0000 0000 )
           &token call,            ( read a token )                      ( CALL <addr>     1B00 ADDR )
            tib d cp,              ( move dict ptr to end of name )      ( CP d tib        0300 0200 0F00 )
              d d inc,             ( move past length field )            ( INC d d         0600 0200 0200 )
              d d inc, ( * )       ( move past length field )            ( INC d d         0600 0200 0200 )
            lnk d st,              ( append link address )               ( ST d lnk        0200 0200 1800 )
            d lnk cp,              ( update link to here )               ( CP lnk d        0300 1800 0200 )
              d d inc,             ( advance dictionary pointer )        ( INC d d         0600 0200 0200 )
              d d inc, ( * )       ( advance dictionary pointer )        ( INC d d         0600 0200 0200 )
             zero append,          ( append 0 immediate flag )           ( ST d zero       0200 0200 0400 )
                                                                         ( INC d d         0600 0200 0200 )
                  ret,                                                   ( RET             1C00 )

  0 sym immediate header,          ( set immediate flag )                ( immediate9 . 0  6900 6D00 6D00 6500 6400 6900 6100 7400 6500 0900 LINK 0000 )
            lnk n inc,             ( point to immediate flag )           ( INC n lnk       0600 0000 1800 )
              n n inc, ( * )       ( point to immediate flag )           ( INC n lnk       0600 0000 1800 )
           true n st,              ( set immediate flag )                ( ST n true       0200 0000 0800 )
                  ret,                                                   ( RET             1C00 )

    0 sym compile header,          ( switch to compiling mode )          ( compile7 . 0    6300 6F00 6D00 7000 6900 6C00 6500 0700 LINK 0000 )
        true comp cp,              ( set comp flag )                     ( CP comp true    0300 1D00 0800 )
                  ret,                                                   ( RET             1C00 )

   0 sym interact header,          ( switch to interactive mode )        ( interact8 . 0   6900 6E00 7400 6500 7200 6100 6300 7400 0800 LINK 0000 )
            label &interact
       false comp cp,              ( reset comp flag )                   ( CP comp false   0300 1D00 0900 )
                  ret,                                                   ( RET             1C00 )

         -1 sym ; header,          ( return )                            ( ;1 . -1         3B00 0100 LINK FFFF )
              ret append,          ( append ret instruction )            ( ST d ret        0200 0200 1C00 )
                                                                         ( INC d d         0600 0200 0200 )
        &interact jump,            ( switch out of compiling mode )      ( JUMP <addr>     1A00 ADDR )
       
      0 sym pushx header,          ( push number to stack from x )       ( pushx5 . 0      7000 7500 7300 6800 7800 0500 LINK 0000 )
              x n cp,              ( bootstrap to interpreter reg )      ( CP n x          0300 0000 0100 )
           &pushn jump,            ( jump to push )                      ( JUMP <addr>     1A00 ADDR )
       
       0 sym popx header,          ( pop number from stack to x )        ( popx4 . 0       7000 6F00 7000 7800 0400 LINK 0000 )
            &popn call,            ( jump pop )                          ( CALL <addr>     1B00 ADDR )
              n x cp,              ( interpreter to bootstrap reg )      ( CP x n          0300 0100 0000 )
                  ret,                                                   ( RET             1C00 )

    0 sym literal header,          ( compile literal from stack )        ( literal7 . 0    6C00 6900 7400 6500 7200 6100 6C00 0700 LINK 0000 )
            &popn call,            ( pop into n )                        ( CALL <addr>     1B00 ADDR )
            &litn jump,            ( compile literal n )                 ( JUMP <addr>     1A00 ADDR )
       
          0 sym , header,          ( append value from stack )           ( ,1 . 0          2C00 0100 LINK 0000 )
            &popn call,            ( pop value from stack )              ( CALL <addr>     1B00 ADDR )
                n append,          ( append n )                          ( ST d n          0200 0200 0000 )
                                                                         ( INC d d         0600 0200 0200 )
                  ret,                                                   ( RET             1C00 )
       
          0 sym ' header,          ( find word - nm set if not found )   ( '1 . 0          2700 0100 LINK 0000 )
           &token call,            ( read a token )                      ( CALL <addr>     1B00 ADDR )
            &find call,            ( find token )                        ( CALL <addr>     1B00 ADDR )
            cur n cp,              ( prep to push cursor )               ( CP n cur        0300 0000 1700 )
          two n n add,             ( address of code field )             ( ADD n n two     0800 0000 0000 0600 )
          two n n add, ( * )       ( address of code field )             ( ADD n n two     0800 0000 0000 0600 )
           &pushn jump,            ( push cursor )                       ( JUMP <addr>     1A00 ADDR )

          -1 40 1 header,          ( skip comment, 40=left paren ASCII ) ( 40 1 0 -1       2800 0100 LINK FFFF )
            label &comment
                n in,              ( next char )                         ( IN n            0400 0000 )
rparch n &comment bne,             ( continue until right-paren )        ( BNE <addr> . .  1400 ADDR 0000 0B00 )
                  ret,                                                   ( RET             1C00 )

   -1 sym recurse header,
             call append,          ( append call instruction )
          two lnk n add,           ( point to code field )
            two n n add,           ( point to code field )
                n append,          ( append code field address )
                  ret,

continue,                          ( patch jump ahead, )

       link @ lnk ldc,             ( compile-time link to runtime lnk )  ( LDC lnk link    0000 1800 E601 )
      here 10 + d ldc,             ( runtime d just past this code )     ( LDC d here+10   0000 0200 F701 )

            &repl jump,            ( start the REPL )                    ( JUMP <addr>     1A00 ADDR )

assemble
