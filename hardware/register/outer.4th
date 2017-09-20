( outer interpreter - reads tokens, compiles headers, literals, ... )
( requires assembler )

( --- register allocation/init ----------------------------------------- )

  0 const x                     ( temp )
  1 const y                     ( temp )

  2 const zero                  ( constant 0 )
  3 const two 2 two ldc,        ( constant 2 )

  4 const true -1 true ldc,     ( truth value - all bits set )
  5 const false                 ( false value )

  6 const zeroch 48 zeroch ldc, ( '0' ASCII )
  7 const rparch 41 rparch ldc, ( right parenthesis ASCII )

  8 const c                     ( char last read )
  9 const sp 32 sp ldc,         ( space ASCII )
 10 const tib                   ( terminal input buffer )
 11 const len                   ( token length )
 12 const len'                  ( name length )
 13 const d                     ( dictionary pointer )
 14 const nm                    ( match flag for search )
 15 const p                     ( pointer )
 16 const c                     ( char being compared )
 17 const p'                    ( pointer )
 18 const c'                    ( char being compared )
 19 const cur                   ( cursor )
 20 const lnk                   ( link pointer )
 21 const base 10 base ldc,     ( number base decimal by default )
 22 const s 32767 s ldc,        ( stack pointer )
 23 const ldc                   ( ldc instruction [0] )
 24 const call 27 call ldc,     ( call instruction )
 25 const ret 28 ret ldc,       ( ret instruction )
 26 const comp                  ( compiling flag )

leap,

( --- tokenization ----------------------------------------------------- )

            label &skipws   ( skip until non-whitespace )
                c in,       ( read char )
     c sp &skipws ble,      ( keep skipping whitespace )
                  ret,
      
            label &name     ( read input name into buffer )
            c tib st,       ( append char )
          tib tib inc,      ( advance tib )
          len len inc,      ( increment length )
                c in,       ( read char )
       c sp &name bgt,      ( continue until whitespace )
          len tib st,       ( append length )
                  ret,
      
            label &token    ( read token into buffer )
            d tib cp,       ( end of dictionary as buffer )
         zero len cp,       ( initial zero length )
          &skipws call,     ( skip initial whitespace )
            &name jump,     ( append name )

( --- dictionary search ------------------------------------------------ )
      
            label &nomatch  ( set no-match flag )
          true nm cp,
                  ret,
      
            label &compcs   ( compare chars to input word )
              p c ld,       ( get char of input word )
            p' c' ld,       ( get char of dictionary word )
    c c' &nomatch bne,      ( don't match? )
            p' p' inc,      ( next char of dictionary word )
              p p inc,      ( next char of input word )
    p tib &compcs blt,      ( not end of word? continue... )
                  ret,      ( we have a match! )
      
            label &nextw    ( advance to next word )
          cur cur ld,       ( follow link address )
                            ( fall through to &comp )
      
            label &comp
zero cur &nomatch beq,      ( no match if start of dict )
        tib len p sub,      ( point p at start of token )
           cur p' dec,      ( point p' at length field )
          p' len' ld,       ( get length )
  len' len &nextw bne,      ( lengths don't match? )
        p' len p' sub,      ( move to beginning of word )
         false nm cp,       ( reset no-match flag )
          &compcs call,     ( compare characters )
   true nm &nextw beq,      ( if no match, try next word )
                  ret,      ( we have a match! )
                  
            label &find     ( find word [tib within dictionary] )
          lnk cur cp,       ( initialize cursor to last )
            &comp jump,     ( compare with tib )
      
( --- number processing ------------------------------------------------ )

            label &digits   ( parse digits )
              p c ld,       ( get char )
       c zeroch c sub,      ( convert to digit )
         x base x mul,      ( base shift left )
            x c x add,      ( add in one's place )
              p p inc,      ( next char )
    p tib &digits blt,      ( not end? continue... )
                  ret,
      
            label &parsenum ( parse token as number )
        tib len p sub,      ( point p at start of word )
           zero x cp,       ( init number )
          &digits jump,
      
            label &pushx    ( push interactive number )
              x s st,       ( store x at stack pointer )
              s s dec,      ( adjust stack pointer )
                  ret,
      
            label &popx     ( pop number )
              s s inc,      ( adjust stack pointer )
              s x ld,       ( load value )
                  ret,

( --- literals --------------------------------------------------------- )

: append, d st, d d inc, ;  ( macro: append and advance d )

            label &litx     ( compile literal )
              ldc append,   ( append ldc instruction )
             zero append,   ( append x register number [0] )
                x append,   ( append value )
             call append,   ( append call instruction )
         &pushx y ldc,      ( load address of &pushx )
                y append,   ( append pushx address )
                  ret,
      
            label &num      ( process token as number )
        &parsenum call,     ( parse number )
  true comp &litx beq,      ( if compiling, compile literal )
           &pushx jump,     ( else, push literal )
      
( --- word processing --------------------------------------------------- )

            label &exec     ( execute word )
        two cur y add,      ( point to code field )
                y exec,     ( exec word )
                  ret,
      
            label &compw    ( compile word )
            cur x inc,      ( point to immediate flag )
              x y ld,       ( read immediate flag )
     true y &exec beq,      ( execute if immediate )
             call append,   ( append call instruction )
              x x inc,      ( point to code field )
                x append,   ( append code field address )
                  ret,
      
            label &word     ( process potential word token )
 true comp &compw beq,      ( if compiling, compile word )
            &exec jump,     ( else, execute word )
      
            label &eval     ( process input tokens )
            &find call,     ( try to find in dictionary )
     true nm &num beq,      ( if not found, assume number )
            &word jump,     ( else, process as a word )
      
( --- REPL ------------------------------------------------------------- )

            label &repl     ( loop forever )
           &token call,     ( read a token )
            &eval call,     ( evaluate it )
            &repl jump,     ( forever )
      
( --- initial dictionary ----------------------------------------------- )

var link
: header, dup 0 do swap , loop , link @ here link ! , , ;

     0 sym create header,   ( word to create words )
           &token call,     ( read a token )
            tib d cp,       ( move dict ptr to end of name )
              d d inc,      ( move past length field )
            lnk d st,       ( append link address )
            d lnk cp,       ( update link to here )
              d d inc,      ( advance dictionary pointer )
             zero append,   ( append 0 immediate flag ) 
                  ret,

  0 sym immediate header,   ( set immediate flag )
            lnk x inc,      ( point to immediate flag )
           true x st,       ( set immediate flag )
                  ret,

    0 sym compile header,   ( switch to compiling mode )
        true comp cp,       ( set comp flag )
                  ret,

   0 sym interact header,   ( switch to interactive mode )
            label &interact
       false comp ldc,      ( reset comp flag )
                  ret,

         -1 sym ; header,   ( return )
              ret append,   ( append ret instruction )
        &interact jump,     ( switch out of compiling mode )
       
      0 sym pushx header,   ( push number to stack from x )
           &pushx jump,     ( jump to push )
       
       0 sym popx header,   ( pop number from stack to x )
            &popx jump,     ( jump pop )
       
          0 sym , header,   ( append value from stack )
            &popx call,     ( pop value from stack )
                x append,   ( append x ) 
                  ret,
       
       0 sym find header,   ( find word )
           &token call,     ( read a token )
            &find call,     ( find token )
            cur x cp,       ( prep to push cursor )
          two x x add,      ( address of code field )
           &pushx jump,     ( push cursor )
       
       0 sym dump header,   ( dump core to boot.bin )
                  dump,     ( TODO: build outside of outer interpreter )

          -1 40 1 header,   ( skip comment - 40=left paren ASCII )
            label &comment
                x in,       ( next char )
rparch x &comment bne,      ( continue to skip until right-paren )
                  ret,

ahead,

( --- set `lnk` to within last header and `d` to just past this code --- )

link @ lnk ldc, ( compile-time link to runtime )
here 5 + d ldc, ( compile-time dict to runtime [advance over code below] )

&repl jump,     ( start the REPL )

assemble

