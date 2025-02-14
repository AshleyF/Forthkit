# Forthkit

Inspired by [Lispkit](https://github.com/AshleyF/Lispkit), build your own Forth from scratch.

0) ~~~Write an interpreter (in Python). Similar to Chuck Moore's first punch card interpreter.~~~ Scratch that! We'll use gforth.
1) Play with it!
    * Write a [console Pixel library](./library/pixels/) (in Forth) using Unicode Braille characters
    * Use this to do [turtle graphics](./library/turtle/) (in Forth)!
2) Make [register-based "hardware"](./hardware/register/) (VM in ~~~C~~~ Forth) and [assembler](./hardware/register/assembler.f) (in Forth)
3) Use the assembler to [build a kernel](./hardware/register/kernel.f) (a bytecode image using Forth)
4) Abandon ~~~the Python interpreter~~~ gforth and [bootstrap](./hardware/register/bootstrap.f) Forth to the new "hardware"
    * [Port pixel library](./hardware/register/pixels-adapter.f) to this
    * [Port turtle graphics](./hardware/register/turtle-fixed-point.f) to this (using fixed point)
    * [Port kernel itself](./hardware/register/kernel-adapter.f) to this
    * [Achieve meta-circularity!](./hardware/register/meta.sh) That is, build the kernel using ~~~Python~~~ gforth, then use that kernel to build a new (identical for now) kernel. Iterate.
    * TODO: re-write kernel in more natural Forth syntax
    * TODO: Fill out the vocabulary to support the [standard core words](./core-words.md)
5) TODO: Build an inner interpreter and take away `call`/`ret` from the "hardware"
    * TODO: Experiment with direct/indirect threading
    * TODO: Experiment with using the return stack for locals and loop counters
    * TODO: Implement remaining Forth control structures
    * TODO: Explain `does>` in the context of direct/indirect threading
    * TODO: Discuss/implement token threading
    * TODO: Discuss separate headers (e.g. fall-through definitions, save memory, ...)
    * TODO: Discuss separate host/target (like Brief)
6) TODO: Make more new "hardware" - a stack machine this time
7) TODO: Port our Forth to this - see how the inner interpreter goes away
    * TODO: Use block-style disk I/O
8) TODO: Move to a colorForth style variant (less syntax, no immediate words, etc.)
9) TODO: Build a block editor and stop using Vim
    * TODO: We've now bootstrapped a whole "OS" for ourselves!
10) TODO: Discuss non-standard ideas:
    * Quotations (`[:` ... `:]`), combinators, (Factor, Joy, Brief, ...)
    * `if` as a combinator
    * Recursion as only control flow
11) TODO: Discuss other execution models (e.g. stack/continuation, XY, Brief, ...)
12) TODO: More ideas
    * VT100 library
    * Snake game
    * Tetris game
    * Discuss optimizing compilation