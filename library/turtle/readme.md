# Turtle Graphics

Turtle Graphics implementation in Forth. It's built on the [`pixels`](../pixels/) library and is meant to run on the [interpreter](../../interpreter/) and future VMs.

The Turtle begins at the center of the canvas, heading "North". You may `go` to another point or `head` in another direction (or use `pose` to accomplish both at once). `begin` to clear the canvas and reset the pose. Use `turn` and `move` to draw. There is no pen-up/down, but instead a `jump` which moves without plotting.

To play with it interactively: [`sh ./play.sh`](./play.sh)

To test it with some demos: [`sh ./test.sh`](./test.sh):

![Turtle Graphics Demo](./demo.png)
