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