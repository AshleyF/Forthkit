# Braille Pixel Library

Console pixel graphics library using [Unicode braille characters (`0x2800`-`0x28FF`)](http://www.unicode.org/charts/PDF/U2800.pdf).

The canvas is 160×160. You may `clear` it, `set` and `reset` pixels, and `show` it.

To test the [`pixels` library](./pixels.4th), try: [`cat ../prelude.4th ./pixels.4th ./test.4th | python ../../interpreter/interpreter.py`](./test.sh)

You should see this little guy (assuming Unicode font supporting Braille and UTF-8 terminal):

    ⠀⠀⠀ ⠀⣀⣠⠤⢤⠤⣀⠀⠀ ⢀⠔⠊⡉⠑⡄
    ⠀⠀⢀⠔⡫⡊⠉⡢⡊⠉⡢⡋⣢⣀⡎⠀⠀⠠⡤⠃
    ⠀⡔⠫⡹⡀⡸⡹⡀⡸⣉⠔⠉⢀⡰⠁⢀⠔⠉
    ⠀⠑⣄⣈⣉⣉⣉⣉⣉⣀⢤⠪⢅⣀⢔⠏
    ⠀⢎⡠⠚⠢⠤⠤⡤⠃⢀⠮⠤⠤⢖⠑⢢
     ⠀⠀⠀⠀⠀⠀⠑⠒⠁⠀⠀⠀ ⠉⠁
