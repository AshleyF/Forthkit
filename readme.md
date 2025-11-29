# Forthkit

Inspired by [Lispkit](https://github.com/AshleyF/Lispkit), build your own Forth from scratch.

0) Learn what Forth is by building a pixel library and turtle graphics in gforth
    * Write a [console Pixel library](./library/pixels/) using Unicode Braille characters
    * Use this to do [turtle graphics](./library/turtle/)
2) Make [register-based "hardware"](./hardware/register/) (VM in Forth) and [assembler](./hardware/register/assembler.fs)
3) Use the assembler to [build a kernel](./hardware/register/kernel.fs) (and boot image)
4) Rewrite the VM in C and abandon gforth
5) TODO: [Bootstrap](./hardware/register/bootstrap.fs) the rest of the language
    * TODO: [Port pixel library](./hardware/register/pixels-adapter.fs)
    * TODO: [Port turtle graphics](./hardware/register/turtle-fixed-point.fs) (using fixed point)
    * TODO: [Port kernel itself](./hardware/register/kernel-adapter.fs)
    * TODO: Achieve meta-circularity! That is, build the kernel using gforth, then use that kernel to build a new (identical for now) kernel. Iterate.
    * TODO: Re-write kernel in more natural Forth syntax
5) TODO: Build an inner interpreter and replace calls
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