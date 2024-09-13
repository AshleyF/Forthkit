# Braille Pixel Library

Console pixel graphics library using [Unicode braille characters (`0x2800`-`0x28FF`)](http://www.unicode.org/charts/PDF/U2800.pdf).

The canvas is 160×160. You may `clear` it, `set` and `reset` pixels, and `show` it.

To test the [`pixels`](./pixels.4th) library: [`sh ./test.sh`](./test.sh)

You should see this little guy (assuming Unicode font supporting Braille and UTF-8 terminal):

```text
    ⠀⠀⠀ ⠀⣀⣠⠤⢤⠤⣀⠀⠀   ⢀⠔⠊⡉⠑⡄
    ⠀⠀⢀⠔⡫⡊⠉⡢⡊⠉⡢⡋⣢⣀⡎⠀⠀⠠⡤⠃
    ⠀⡔⠫⡹⡀⡸⡹⡀⡸⣉⠔⠉⢀⡰⠁⢀⠔⠉
    ⠀⠑⣄⣈⣉⣉⣉⣉⣉⣀⢤⠪⢅⣀⢔⠏
    ⠀⢎⡠⠚⠢⠤⠤⡤⠃⢀⠮⠤⠤⢖⠑⢢
     ⠀⠀⠀⠀⠀ ⠀⠑⠒⠁⠀⠀ ⠀ ⠉⠁
```

## Walkthrough

The idea is to use a range of Braille Unicode characters to each represent 2×4 pixels. We'll have a 160×160 pixel canvas, made from 80×20 characters.

```forth
160 const width
160 const height

width 2 / const columns
width height * 8 / const size
```

We define the canvas `width` and `height`, and can compute the `columns` and total number of characters (`size`). These constants are computed once at _compile time_ as opposed to say `: columns width 2 / ;`.

```forth
( init dot masks )
128 64 32 4 16 2 8 1  8 0 do size i + m! loop
```

Here we've stored a table of dot mask values just beyond the canvas buffer (at `size`). We push the table values, then iterate eight times, poking them into memory. These values will be used ask masks to build each of the eight dots in a single Braille character.

```forth
: clear size times 10240 i m! loop ;
```

A word to `clear` the canvas fill each cell with the Unicode value of an empty Braille cell (`10240`). This should be called before drawing.

The `times` word comes from the [prelude](../prelude.4th) and merely starts a loop for n-times with `0 do` (that is, `: times 0 do ;`).

```forth
: cell 4 / floor columns * swap 2 / floor + ;
```

Each Braille character cell contains 2×4 dots. We can compute the memory `cell` in which a dot on the canvas falls, given the x and y coordinates, by dividing y by 4 and adding the number of `columns` (jumping by _rows_), then add to this x divided by 2 (the column of x).

For example, `1 3 cell` returns `0` because the dot falls on the bottom right corner of the first cell. However if we move to the right, `2 3 cell` returns `1`; the bottom left corner of the 2nd cell. Moving down, `2 4 cell` returns `81`; the top left corner of the second cell on the second 80-character row.

```forth
: mask 4 mod 2 * swap 2 mod + size + m@ ;
```

To look up the `mask` value we mod the x coordinate by 2 and the y coordinate by 4 (2×4 dots per cell), and look in the table we built just past the canvas memory (`size +`).

```forth
: cell-mask 2dup cell -rot mask over m@ ;
```

To get the cell and mask value, we can duplicate the pair of x and y coordinates with `2dup` (defined in the prelude as simply `over over`), get the `cell` of one pair, rotate that out of the way and get the `mask` of the duplicate pair. Finally we fetch the current value at the cell with `over m@`. Maybe confusing, but `cell-mask` takes an x, y pair and returns the cell, the mask and the current value at the cell.

```forth
: set cell-mask or swap m! ;
: reset cell-mask swap not and swap m! ;
````

Using `cell-mask` we can `set` or `reset` individual dots. To `set`, we `or` the mask with the current value. To `reset`, we invert the mask (`not`), then `and` it with the current value. In both cases we then store the value in the cell.

```forth
: show
  size 0 do
    i columns mod 0 = if 10 emit then  ( newline as appropriate )
    i m@ emit
  loop ;
```

The above `clear`, `set` and `reset` words don't display anything on the screen. They just manipulate the buffer. To `show` the buffer, we walk it and emit the values, while emitting a newline (`10`) after each 80-character column.

## Turtle Turtle

Before we get into proper [turtle graphics](../turtle/), let's at least draw a graphic of a turtle. We'll start by making a mechanism to draw from bitmaps in code.

```forth
var x var y

: start clear 0 x ! 0 y ! ;
: | 0 do 35 = if x @ y @ set then 1 x +! loop 0 x ! 1 y +! ;
```

The `+1` used above comes from the prelude. It adds and stores a value in a variable (defined as `: +! dup @ rot + swap ! ;`). For example, `1 x +!` increments the value stored in `x`.

The `start` word clears the canvas and initializes the `x`/`y` coordinates. The `|` word expects to have a sequence of numbers on the stack and sets dots for each `35` encountered, which is the ASCII for a `#` character. The sequence should be followed by a number indicating its length.

Remember the `sym` word that deconstructs a token into its ASCII values followed by the length? Perfect! `sym _#_#_` places `95 35 95 35 95 5` on the stack; a `35` for each `#` and the length we need. The `|` work takes this and sets the dots accordingly.

```forth
start
sym _#_#_ |
sym _#_#_ |
sym #___# |
sym _###_ |
show
```

Showing this tiny happy face in a few Braille characters.

```text
⢜⣘⠄
```

In [test.4th](./test.4th) is a our turtle.

```forth
: turtle start
  sym ```````````````````````````````####`` |
  sym `````````````````````````````##````#` |
  sym ```````````#######``````````#```````# |
  sym ````````####```#``##```````#````#```# |
  sym ``````##`###```###``##`````#````````# |
  sym `````##`#```#`#```#`#`#```#`````````# |
  sym ````#``#`````#`````#```#``#``````###` |
  sym ```#``#`#```#`#```#`#`#####```````#`` |
  sym ``####```###```###``##````#`````##``` |
  sym `##``#```#`#```#```#`````#`````#````` |
  sym #``#`#```#`#```#``#``````#````#`````` |
  sym #```#`#`#`#`#`#`##`````##````#``````` |
  sym #````###########`````##`````##``````` |
  sym `#``````````````````#``````##```````` |
  sym ``#```````````````##`##```#`#```````` |
  sym ``################`#```###`#````````` |
  sym `#```#````````#````#``````#`````````` |
  sym #```###```````#```#`````##`##```````` |
  sym #``#```#######````#######````#``````` |
  sym `##`````````#````#```````#```#``````` |
  sym ````````````#```#`````````###```````` |
  sym `````````````###````````````````````` |
  show ;
```

## Next

Next let's build a [turtle graphics](../turtle/) library with this.