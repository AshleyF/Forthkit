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