# Notes

## Bugs

- Can't define words in terms of prior versions (infinite recursion!)
- Machine build seems to not work on Mac (clang compiler)
- u. doesn't work for signed values
  - Need to rationalize signed/unsigned behavior in general

## Bootstrapping

- Make dot word (`.`) -- even simple single-digit
- Make `header` word (at least input)
- Do number parsing and echo
- Make `find` word and search dictionary
- Make `execute` word and, well, execute words
- Add `state` word
- Compile/push numbers depending on `state`
  - Start with pushing numbers (`: foo 42 ;`)
- Compile/execute words depending on `state` and immediate flag
- Make `create` and `immediate`
- Make colon (`:`) and semicolon (`;`)
- Away you go!

## Learnings

- Building a VM in Forth makes debugging *much* easier
- Stacks that maintain a pointer to the top are useful (move/push rather than push/move)
- Tricky! `bl word find` stomps parsed word and finds "find" when interpreted! (fine compiled)
- Making code relocatable with relative jumps would be useful!
- Writing the outer interpreter in assembly is easier to get started than with standard words
- Making the machine too primitive (e.g. 16 instructions) leads to bloat
- Machine in Forth (rather than C) is much easier to debug

## Koans

- `create mybuffer 100 allot` (instead `100 buffer: mybuffer`)

## Pick

```forth
: pick ( xu...x1 x0 u -- xu...x1 x0 xu )
  dup 0= if drop dup exit then  swap >r 1- recurse r> swap
;
```

Remove u. Copy the xu to the top of the stack. An ambiguous condition exists if there are less than u+2 items on the stack before PICK is executed.

- `: dup 0 pick ;`
- `: over 1 pick ;`

## If/Else/Then

```forth
: if, 0branch, ;              ( compile branch if TOS is 0, push address of branch address )
: then, here swap ! ;         ( patch previous branch to here )
: else, branch, swap then, ;  ( patch previous branch to here and start unconditional branch over false condition )
```

## Books

- Dr. Ting's [Systems Guide to figForth](https://www.forth.org/OffeteStore/1010_SystemsGuideToFigForth.pdf)

## Links

- [The Evolution of Forth](https://www.forth.com/resources/forth-programming-language/)
- A little [treasure trove](https://www.complang.tuwien.ac.at/projects/forth.html)
- [The FORTH Approach to Operating Systems](https://dl.acm.org/doi/pdf/10.1145/800191.805586)