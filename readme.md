# Forthkit

Inspired by Lispkit, build your own Forth from scratch.

1) [Write an interpreter (in Python)](./interpreter/). Similar to Chuck Moore's first punch card interpreter.
2) Play with it!
    * Write a [console Pixel library](./library/pixels/) (in Forth) using Unicode Braille characters
    * Use this to do [Turtle Graphics](./library/turtle/) (in Forth)!
3) Make [register-based "hardware"](./hardware/register/) (VM in C) to target
4) TODO: Make an assembler for this "hardware" (in Forth)
5) TODO: Use this assembler to build an outer interpreter (a bytecode image using Forth)
6) TODO: Abandon the Python interpreter and bootstrap Forth to the new "hardware"
    * TODO: Port Turtle Graphics to this
7) TODO: Build an inner interpreter and take away `call`/`ret` from the "hardware"
    * TODO: Experiment with direct/indirect threading
    * TODO: Experiment with using the return stack for locals and loop counters
    * TODO: Implement remaining Forth control structures
8) TODO: Make more new "hardware" - a stack machine this time
9) TODO: Port our Forth to this - see how the inner interpreter goes away
    * TODO: Use block-style disk I/O
10) TODO: Move to a colorForth style varient (less syntax, no immediate words, etc.)
11) TODO: Build a block editor and stop using Vim
    * TODO: We've now bootstrapped a whole "OS" for ourselves!
