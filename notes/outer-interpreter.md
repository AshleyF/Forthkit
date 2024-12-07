# Outer Interpreter

- Read
  - WORD:
    - skip-whitespace (<= 32)
    - apppend chars from input (key - skipping -1)
    - stop on whitespace
- EVAL
  - search dictionary:
    - starting with latest
    - skip current if compiling (for redefinition)
    - compare:
      - remove immediate flag
      - compare lengths
      - compare characters
      - continues with next work if no match
      - not found if start of dictionary
    - if found:
      - execute (code field) if immediate or not compiling
      - otherwise compile call
    - if not found:
      - number?
        - if starts with '-', track sign (-1)
        - error if non-digit
          - print token followed by '?'
        - multiply by base (shift left digit)
        - add digit
        - continue until end
        - set sign (multiply)
      - push if not comiling, otherwise compile literal
  - PRINT ("ok"?)  
  - LOOP
            
## Primitive Words

- here
- allot
- word
- create (at least header)
- immediate
- [ (compile)
- ] (interactive)
- ; (return - append ret, and interactive mode)
- literal
- , (comma)
- c, (c-comma)
- ' (tick)
- ( (comment)
- recurse (TCO?)
- base (variable)

## Definitions

: QUIT
   ( empty the return stack and set the input source to the user input device )
   POSTPONE [ \ leave compilation
     REFILL
   WHILE
     ['] INTERPRET CATCH
     CASE
      0 OF STATE @ 0= ( interpreting? ) IF ." OK" THEN CR ENDOF
     -1 OF ( Aborted ) ENDOF
     -2 OF ( display message from ABORT" ) ENDOF
     ( default ) DUP ." Exception # " .
     ENDCASE
   REPEAT BYE
;

- QUIT Empty the return stack, store zero in SOURCE-ID if it is present, make the user input device the input source, and enter interpretation state. Do not display a message. Repeat the following: Accept a line from the input source into the input buffer, set >IN to zero, and interpret. Display the implementation-defined system prompt if in interpretation state, all processing has been completed, and no ambiguous condition exists.
- POSTPONE Skip leading space delimiters. Parse name delimited by a space. Find name. Append the compilation semantics of name to the current definition. An ambiguous condition exists if name is not found.
- [ Enter interpretation state. [ is an immediate word.
- REFILL (-b) Attempt to fill the input buffer from the input source, returning a true flag if successful. When the input source is the user input device, attempt to receive input into the terminal input buffer. If successful, make the result the input buffer, set >IN to zero, and return true. Receipt of a line containing no characters is considered successful. If there is no input available from the current input source, return false. When the input source is a string from EVALUATE, return false and perform no other action.
- ['] Skip leading space delimiters. Parse name delimited by a space. Find name. Append the run-time semantics given below to the current definition.
- INTERPRET System-implementation word INTERPRET that embodies the text interpreter semantics
- >IN (-a) a-addr is the address of a cell containing the offset in characters from the start of the input buffer to the start of the parse area.
- ABORT Empty the data stack and perform the function of QUIT, which includes emptying the return stack, without displaying a message
