# Turtle Graphics

This is (or will be) a Turtle Graphics implementation in Forth, meant to run on the [interpreter](../interpreter/) and future VMs.

To test the [`pixels` library](../library/pixels.4th), try: [`cat ./library/prelude.4th ./library/pixels.4th ./turtle/plot.4th | python ./interpreter/interpreter.py`](../demos/plot.sh)

You should see this little guy (assuming Unicode font supporting Braille and UTF-8 terminal):

    ⠀⠀⠀⠀ ⠀⣀⣠⠤⢤⠤⣀⠀⠀ ⢀⠔⠊⡉⠑⡄
    ⠀⠀⢀⠔⡫⡊⠉⡢⡊⠉⡢⡋⣢⣀⡎⠀⠀⠠⡤⠃
    ⠀⡔⠫⡹⡀⡸⡹⡀⡸⣉⠔⠉⢀⡰⠁⢀⠔⠉
    ⠀⠑⣄⣈⣉⣉⣉⣉⣉⣀⢤⠪⢅⣀⢔⠏
    ⠀⢎⡠⠚⠢⠤⠤⡤⠃⢀⠮⠤⠤⢖⠑⢢
     ⠀⠀⠀⠀⠀⠀⠀⠑⠒⠁⠀⠀⠀⠀ ⠉⠁
