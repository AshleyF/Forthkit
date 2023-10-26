# Forthkit

Inspired by [Lispkit](https://github.com/AshleyF/Lispkit), build your own Forth from scratch.

0) [Write an interpreter (in Python)](./interpreter/). Similar to Chuck Moore's first punch card interpreter.
1) Play with it!
    * Write a [console Pixel library](./library/pixels/) (in Forth) using Unicode Braille characters
    * Use this to do [Turtle Graphics](./library/turtle/) (in Forth)!
2) Make [register-based "hardware"](./hardware/register/) (VM in C) and [assembler](./hardware/register/assembler.4th) (in Forth)
3) Use the assembler to [build an outer interpreter](./hardware/register/outer.4th) (a bytecode image using Forth)
4) TODO: Abandon the Python interpreter and bootstrap Forth to the new "hardware"
    * TODO: Port Turtle Graphics to this
5) TODO: Build an inner interpreter and take away `call`/`ret` from the "hardware"
    * TODO: Experiment with direct/indirect threading
    * TODO: Experiment with using the return stack for locals and loop counters
    * TODO: Implement remaining Forth control structures
6) TODO: Make more new "hardware" - a stack machine this time
7) TODO: Port our Forth to this - see how the inner interpreter goes away
    * TODO: Use block-style disk I/O
8) TODO: Move to a colorForth style varient (less syntax, no immediate words, etc.)
9) TODO: Build a block editor and stop using Vim
    * TODO: We've now bootstrapped a whole "OS" for ourselves!
