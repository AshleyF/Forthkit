( kernel - outer interpreter reads tokens, compiles headers, literals, ... )
( requires assembler )

( --- register allocation/init ----------------------------------- )

 0 constant n                      ( number parsing - assumed 0 below )
 1 constant m                      ( temp number )
 2 constant x                      ( shared by bootstrap )
 3 constant d             d 0 ldc, ( dictionary pointer - shared by bootstrap - magic address )
 4 constant lnk         lnk 0 ldc, ( link pointer - magic address - shared by bootstrap )
 
 ( magic addresses: d at 1, lnk at 5 )

 5 constant zero                   ( constant 0 - shared by bootstrap )
 6 constant one         one 1 ldc, ( constant 1 )
 7 constant two         two 2 ldc, ( constant 2 )
 8 constant three     three 3 ldc, ( constant 3 )
 9 constant ten        ten 10 ldc, ( number ten decimal by default )

10 constant true      true -1 ldc, ( truth value - all bits set )
11 constant false                  ( false value )

12 constant zeroch  zeroch 48 ldc, ( '0' ASCII )
13 constant rparch  rparch 41 ldc, ( right parenthesis ASCII )
14 constant spch    spch   32 ldc, ( space ASCII )
15 constant negch   negch  45 ldc, ( '-' ASCII )

16 constant tib                    ( terminal input buffer )
17 constant len                    ( token length )
18 constant len'                   ( name length )
19 constant nm                     ( match flag for search )
20 constant p                      ( pointer )
21 constant c                      ( char being compared )
22 constant p'                     ( pointer )
23 constant c'                     ( char being compared )
24 constant cur                    ( cursor )
25 constant s         s 32767 ldc, ( stack pointer )
26 constant ldc         ldc 1 ldc, ( ldc instruction [0] )
27 constant jump      jump 28 ldc, ( jump instruction )
28 constant call      call 29 ldc, ( call instruction )
29 constant ret        ret 31 ldc, ( ret instruction, 28 by luck )
30 constant comp                   ( compiling flag )
31 constant sign                   ( number sign while parsing )

ahead,                             ( jump over dictionary )

( --- tokenization ----------------------------------------------- )

            label 'skipws          ( skip until non-whitespace )
                c in,              ( read char )
   c spch 'skipws ble,             ( keep skipping whitespace )
                  ret,
      
            label 'name            ( read input name into buffer )
            c tib stb,             ( append char )
          tib tib inc,             ( advance tib )
          len len inc,             ( increment length )
            label 'skipnone
                c in,              ( read char )
 c zero 'skipnone blt,             ( skip no key values )
     c spch 'name bgt,             ( continue until whitespace )
          len tib stb,             ( append length )
                  ret,
      
            label 'token           ( read token into buffer )
            d tib cp,              ( end of dictionary as buffer )
         zero len cp,              ( initial zero length )
          'skipws call,            ( skip initial whitespace )
            'name jump,            ( append name )

( --- dictionary search ------------------------------------------ )
      
            label 'nomatch
          true nm cp,              ( set no-match flag )
                  ret,
      
            label 'compcs          ( compare chars to input word )
              p c ldb,             ( get char of input word )
            p' c' ldb,             ( get char of dictionary word )
    c c' 'nomatch bne,             ( don't match? )
            p' p' inc,             ( next char of dictionary word )
              p p inc,             ( next char of input word )
    p tib 'compcs blt,             ( not end of word? continue... )
                  ret,             ( we have a match! )
      
            label 'nextw           ( advance to next word )
          cur cur ld,              ( follow link address )
                                   ( fall through to 'comp )

            label 'comp
zero cur 'nomatch beq,             ( no match if start of dict )
        tib len p sub,             ( point p at start of token )
           cur p' dec,             ( point p' at length field )
          p' len' ldb,             ( get length )
  len' len 'nextw bne,             ( lengths don't match? )
        p' len p' sub,             ( move to beginning of word )
         false nm cp,              ( reset no-match flag )
          'compcs call,            ( compare characters )
   true nm 'nextw beq,             ( if no match, try next word )
                  ret,             ( we have a match! )
                  
            label 'find            ( find word [tib within dictionary] )
          lnk cur cp,              ( initialize cursor to last )
 true comp 'nextw beq,             ( skip current word if compiling )
            'comp jump,            ( compare with tib )
      
( --- number processing ------------------------------------------ )

            label 'error           ( print invalid word and halt! )
              d c ldb,             ( load char of word )
                c out,             ( output char )
              d d inc,             ( advance to next char )
          len len dec,             ( decrement length )
  len zero 'error bgt,             ( if more, continue )
             c 63 ldc,             ( load question mark [63] )
                c out,             ( output question mark )
                  halt,            ( halt! TODO: recover )

            label 'negate          ( set negative sign )
        true sign cp,              ( note: `true` is -1 )
              p p inc,             ( next char )
                                   ( fall through to 'digits )

            label 'digits          ( parse digits )
              p c ldb,             ( get char )
       c zeroch c sub,             ( convert to digit )
     c ten 'error bge,             ( error if non-digit )
    c zero 'error blt,             ( error if non-digit )
          n ten n mul,             ( base ten shift left )
            n c n add,             ( add in one's place )
              p p inc,             ( next char )
    p tib 'digits blt,             ( not end? continue... )
         sign n n mul,             ( multiply by sign )
                  ret,
      
            label 'parsenum        ( parse token as number )
        tib len p sub,             ( point p at start of word )
           zero n cp,              ( init number )
         one sign cp,              ( init sign )
              p c ldb,             ( get char )
  c negch 'negate beq,             ( set negative sign )
          'digits jump,            ( parse digits )
      
            label 'pushn           ( push interactive number )
              n s st,              ( store n at stack pointer )
          s two s sub,             ( adjust stack pointer )
                  ret,
      
            label 'popn            ( pop number )
          s two s add,             ( adjust stack pointer )
              s n ld,              ( load value )
                  ret,

( --- literals --------------------------------------------------- )

: append, d st, d two d add, ;     ( macro: append and advance d )
: appendc, d stb, d d inc, ;       ( macro: append byte and advance d )

            label 'litn            ( compile literal )
              ldc appendc,         ( append ldc instruction )
                n append,          ( append value )
             zero appendc,         ( append n register number [0] )
             call appendc,         ( append call instruction )
         m 'pushn ldc,             ( load address of 'pushn )
                m append,          ( append pushn address )
                  ret,
      
            label 'num             ( process token as number )
        'parsenum call,            ( parse number )
  true comp 'litn beq,             ( if compiling, compile literal )
           'pushn jump,            ( else, push literal )
      
( --- word handling ----------------------------------------------- )
            
            label 'exec            ( execute word )
      three cur m add,             ( point to code field )
                m exec,            ( exec word )
                  ret,
      
            label 'compw           ( compile word )
        cur two n add,             ( point to immediate flag )
              n m ldb,             ( read immediate flag )
    false m 'exec bne,             ( execute if immediate -- not 255 )
             call appendc,         ( append call instruction )
              n n inc,             ( point to code field )
                n append,          ( append code field address )
                  ret,
      
            label 'word            ( process potential word token )
 true comp 'compw beq,             ( if compiling, compile word )
            'exec jump,            ( else, execute word )
      
            label 'eval            ( process input tokens )
            'find call,            ( try to find in dictionary )
     true nm 'num beq,             ( if not found, assume number )
            'word jump,            ( else, process as a word )

( --- REPL ------------------------------------------------------- )

            label 'repl            ( loop forever )
           'token call,            ( read a token )
            'eval call,            ( evaluate it )
            'repl jump,            ( forever )
      
( --- initial dictionary ----------------------------------------- )

variable link
: header, dup 0 do swap c, loop c, ( length ) link @ here link ! , ( link ) c, ( flag ) ;

    -1 sym ascii header,           ( word to get ASCII value of next word )
           'token call,            ( read a word )
              d n ldb,             ( load first char into n )
            'litn jump,            ( compile literal n )

     0 sym create header,          ( word to create words )
           'token call,            ( read a token )
            tib d cp,              ( move dict ptr to end of name )
              d d inc,             ( move past length field )
            lnk d st,              ( append link address )
            d lnk cp,              ( update link to here )
          d two d add,             ( advance dictionary pointer )
             zero appendc,         ( append 0 immediate flag )
                  ret,

  0 sym immediate header,          ( set immediate flag )
        lnk two n add,             ( point to immediate flag )
           true n stb,             ( set immediate flag )
                  ret,

    0 sym compile header,          ( switch to compiling mode )
        true comp cp,              ( set comp flag )
                  ret,

   0 sym interact header,          ( switch to interactive mode )
            label 'interact
       false comp cp,              ( reset comp flag )
                  ret,

         -1 sym ; header,          ( return )
              ret appendc,         ( append ret instruction )
        'interact jump,            ( switch out of compiling mode )
       
      0 sym pushx header,          ( push number to stack from x )
              x n cp,              ( bootstrap to kernel reg )
           'pushn jump,            ( jump to push )
       
       0 sym popx header,          ( pop number from stack to x )
            'popn call,            ( call pop )
              n x cp,              ( kernel to bootstrap reg )
                  ret,

    0 sym literal header,          ( compile literal from stack )
            'popn call,            ( pop into n )
            'litn jump,            ( compile literal n )
       
          0 sym , header,          ( append value from stack )
            'popn call,            ( pop value from stack )
                n append,          ( append n )
                  ret,

         0 sym c, header,          ( append byte value from stack )
            'popn call,            ( pop value from stack )
               n appendc,          ( append n )
                  ret,
       
          0 sym ' header,          ( find word - nm set if not found )
           'token call,            ( read a token )
            'find call,            ( find token )
            cur n cp,              ( prep to push cursor )
        three n n add,             ( address of code field )
           'pushn jump,            ( push cursor )

          -1 40 1 header,          ( skip comment, 40=left paren ASCII )
            label 'comment
                n in,              ( next char )
rparch n 'comment bne,             ( continue until right-paren )
                  ret,

   -1 sym recurse header,
             call appendc,         ( append jump instruction )
      three lnk n add,             ( point to code field )
                n append,          ( append code field address )
                  ret,

-1 sym recurse-tail header,
             jump appendc,         ( append jump instruction -- TCO! )
      three lnk n add,             ( point to code field )
                n append,          ( append code field address )
                  ret,

continue,                          ( patch jump ahead, )

     d zero 'repl bne,             ( loading from image with dictionary in place )

       lnk link @ ldc,             ( compile-time link to runtime lnk )
      d here 13 + ldc,             ( runtime d just past this code )

            'repl jump,            ( start the REPL )

assemble
