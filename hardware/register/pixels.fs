\ pixel graphics library using Unicode Braille characters

: buffer: create allot ; \ TODO: not defined in gforth?!

160 constant width
160 constant height

width 2 / constant columns
width height * 8 / constant size

size buffer: screen
create mask-table 1 c, 8 c, 2 c, 16 c, 4 c, 32 c, 64 c, 128 c,

: clear ( -- )
  screen size + screen do 0 i c! loop ;

: char-cell ( x y -- cell )
  4 / columns * swap 2 / + ;

: mask ( x y -- mask )
  4 mod 2 * swap 2 mod + mask-table + c@ ;

: char-cell-mask ( x y -- cell mask char )
  2dup char-cell -rot mask over screen + c@ ;

: set ( x y -- )
  char-cell-mask or swap screen + c! ;

: get ( x y -- b )
  char-cell-mask and swap drop 0<> ;

: reset ( x y -- )
  char-cell-mask swap invert and swap screen + c! ;

: u>= 2dup > -rot = or ; \ TODO: U

: utf8-emit ( c -- )
    dup 128 < if emit exit then
    0 swap 63 begin 2dup > while 2/ >r dup 63 and 128 or swap 6 rshift r> repeat
    127 xor 2* or begin dup 128 u>= while emit repeat drop ;

: show
  size 0 do
    i columns mod 0= if 10 emit then \ newline as appropriate
    i screen + c@ 10240 or utf8-emit
  loop ;

27 constant esc
\ : show-sixel
\   esc emit ." P;1q"
\   [char] " emit ." 1;1" \ 1:1 pad:pan ratio (square pixels)
\   height 0 do \ TODO: missed bottom rows
\     width 0 do
\       0
\       i j get if 7 or then
\       j height 1 - < if
\         i j 1 + get if 56 or then
\       then
\       63 + dup emit dup emit emit
\     loop
\     [char] - emit
\   2 +loop
\   esc emit [char] \ emit cr ;

\ : show-sixel-tiny
\   esc emit ." P;1q" \ transparent zero pixel value (also, default 0 aspect ratio -- override below)
\   [char] " emit ." 1;1" \ 1:1 pad:pan ratio (square pixels)
\   height 0 do \ TODO: missed bottom rows
\     width 0 do
\       0
\       i j     get if  1 or then
\       j height 1 - < if
\         i j 1 + get if  2 or then
\         j height 2 - < if
\           i j 2 + get if  4 or then
\           j height 3 - < if
\             i j 3 + get if  8 or then
\             j height 4 - < if
\               i j 4 + get if 16 or then
\               j height 5 - < if
\                 i j 5 + get if 32 or then
\               then
\             then
\           then
\         then
\       then
\       63 + emit
\     loop
\     [char] - emit
\   6 +loop
\   esc emit [char] \ emit cr ;
\ 
\ : show show-sixel-tiny ;

variable line 0 line !
: pixels parse-name 0 do dup c@ [char] * = if i line @ set then 1+ loop drop 1 line +! ;

clear
pixels ```````````````````````````````****``
pixels `````````````````````````````**````*`
pixels ```````````*******``````````*```````*
pixels ````````****```*``**```````*````*```*
pixels ``````**`***```***``**`````*````````*
pixels `````**`*```*`*```*`*`*```*`````````*
pixels ````*``*`````*`````*```*``*``````***`
pixels ```*``*`*```*`*```*`*`*****```````*``
pixels ``****```***```***``**````*`````**```
pixels `**``*```*`*```*```*`````*`````*`````
pixels *``*`*```*`*```*``*``````*````*``````
pixels *```*`*`*`*`*`*`**`````**````*```````
pixels *````***********`````**`````**```````
pixels `*``````````````````*``````**````````
pixels ``*```````````````**`**```*`*````````
pixels ``****************`*```***`*`````````
pixels `*```*````````*````*``````*``````````
pixels *```***```````*```*`````**`**````````
pixels *``*```*******````*******````*```````
pixels `**`````````*````*```````*```*```````
pixels ````````````*```*`````````***````````
pixels `````````````***`````````````````````
show

clear
pixels *************************************``******```********************************
pixels ***********************************```````````````******************************
pixels *********************************``````````````````*****************************
pixels *******************************`````````````````````****************************
pixels ****************************`````````````````````````***************************
pixels *************************```````````````````````````*`**************************
pixels ***********************````````````````````````````````*************************
pixels **********************``````````````````````````````````************************
pixels **********************```````````````````````````````````***********************
pixels *********************`````````````````````````````````````**********************
pixels *********************``````````````````````````````````````*********************
pixels ********************```````````````````***`*````````````````********************
pixels ********************````````````````**********``````````````********************
pixels ********************`````````````****************`````````````******************
pixels *******************````````````********************```````````******************
pixels ******************```````````************************``````````*****************
pixels *****************```````````**************************``````````****************
pixels *****************```````````***************************````````*****************
pixels *****************```````````***************************`````````****************
pixels ****************```````````*****************************````````****************
pixels ****************``````````******************************````````****************
pixels ****************``````````*******************************```````****************
pixels ****************`````````********************************```````****************
pixels ****************`````````********************************``````*****************
pixels ****************``````````********************************`````*****************
pixels *****************`````````********************************`````*****************
pixels *****************`````````********************************````******************
pixels *****************``````````*******************************`````*****************
pixels ******************````````********************************`````*****************
pixels ******************`````````*********```*******************````******************
pixels ******************``````````*****``````********````*******````******************
pixels *****************````````**`**`````````********````````***````******************
pixels ******************`````````*```````````********`````````**````******************
pixels ******************```````**`````******```*****``**```*```*````******************
pixels ******************```````*``````***`***``****``***`****```````******************
pixels ******************``````***```````````**``****````````*``**```******************
pixels ******************``````***`````````*``*``****```````````**```******************
pixels ******************``````****````*``*****``********``**``***``*******************
pixels ******************``````*****``````*****`********```*```***``*******************
pixels *****************```````*****`*****************************``*******************
pixels *****************````````*****`****************************``*******************
pixels *****************````````**********************************``*******************
pixels *****************````````**********************************``*******************
pixels ******************```````**************`*******************`********************
pixels ******************```````*************`*********`**********`********************
pixels ******************`*``````************`**``******`*********`********************
pixels ******************`*``````************`*````**``*`*********`********************
pixels ********************``````************``````````**`********`********************
pixels *******************`*`````************``````````***********`********************
pixels ********************```````**********````````````**`*******`********************
pixels *********************````````*******```````````````*`******`********************
pixels *********************```````*****`*```````****```````*****``********************
pixels *********************````````****````````****`````````***```********************
pixels **********************```````****````````````***``````***``*********************
pixels *********************````````****```````**********```****``*********************
pixels **********************```````****``````************`****```*********************
pixels **********************```````*****```````````````*******``**********************
pixels **********************`````````******````````````***`***``**********************
pixels ***********************````````*****`*`````**````*`*`**```**********************
pixels ************************````````*``*`***`*`***`**`***````***********************
pixels ***********************```````````````*`````****`````````***********************
pixels ******************```````````````````````*``````````````************************
pixels ******************``````````````````````````````*``````*************************
pixels *******************```````````````````````````*`*``````*************************
pixels ********************``````````````````````````````````**************************
pixels *********************````````````````````````````````***************************
pixels **********************``````````````````````````````****************************
pixels ***********************`````````````````````````````****************************
pixels ************************````````````````````````````****************************
pixels *************************``````````````````````````*****************************
pixels **************************`````````````````````````*****************************
pixels ***************************````````````````````````*****************************
pixels ****************************`````````````````*`*``******************************
pixels *****************************```````````````***`*`******************************
pixels ******************************```````````````*`*``******************************
pixels *******************************`````***``````*```*******************************
pixels ********************************```*******``````**`*****************************
pixels *********************************`*********`***`*``*****************************
pixels **********************************`***************`*****************************
pixels ***********************************``*************`*****************************
show

\ 1062 constant bytes
\ create face
\ hex
\ 4a c, 04 c, 0b c, 06 c, 88 c, 09 c, 04 c, 0e c, 83 c, 1e c, 80 c, 21 c, 7e c, 23 c, 7b c, 27 c,
\ 77 c, 2a c, 72 c, 2f c, 6e c, 30 c, 01 c, 02 c, 6a c, 36 c, 67 c, 37 c, 01 c, 02 c, 64 c, 3b c,
\ 01 c, 01 c, 62 c, 3f c, 60 c, 40 c, 5f c, 43 c, 5d c, 45 c, 5a c, 46 c, 59 c, 48 c, 57 c, 4b c,
\ 56 c, 4b c, 54 c, 4d c, 53 c, 26 c, 02 c, 26 c, 51 c, 26 c, 06 c, 02 c, 02 c, 20 c, 50 c, 23 c,
\ 0f c, 1e c, 4f c, 21 c, 14 c, 1c c, 4f c, 1e c, 1b c, 19 c, 4e c, 1b c, 20 c, 19 c, 4b c, 19 c,
\ 26 c, 17 c, 49 c, 18 c, 29 c, 16 c, 48 c, 17 c, 2d c, 14 c, 47 c, 17 c, 2f c, 14 c, 46 c, 16 c,
\ 31 c, 13 c, 45 c, 16 c, 33 c, 14 c, 42 c, 17 c, 34 c, 12 c, 43 c, 16 c, 36 c, 11 c, 42 c, 18 c,
\ 35 c, 11 c, 43 c, 17 c, 36 c, 11 c, 41 c, 16 c, 39 c, 10 c, 41 c, 16 c, 39 c, 10 c, 41 c, 15 c,
\ 3b c, 0f c, 40 c, 15 c, 3c c, 0f c, 40 c, 15 c, 3d c, 0e c, 40 c, 15 c, 3d c, 0f c, 3f c, 14 c,
\ 3e c, 0f c, 3f c, 13 c, 40 c, 0e c, 3f c, 13 c, 40 c, 0e c, 3f c, 12 c, 41 c, 0c c, 41 c, 12 c,
\ 41 c, 0c c, 42 c, 13 c, 40 c, 0b c, 42 c, 13 c, 40 c, 0b c, 43 c, 12 c, 40 c, 0b c, 43 c, 14 c,
\ 3e c, 0a c, 44 c, 13 c, 3f c, 09 c, 45 c, 13 c, 3f c, 09 c, 46 c, 13 c, 3e c, 0a c, 45 c, 12 c,
\ 3f c, 0a c, 46 c, 11 c, 3f c, 0a c, 46 c, 12 c, 3e c, 09 c, 47 c, 13 c, 11 c, 06 c, 26 c, 09 c,
\ 47 c, 10 c, 01 c, 03 c, 0d c, 09 c, 10 c, 07 c, 0f c, 09 c, 47 c, 14 c, 0b c, 0c c, 10 c, 08 c,
\ 0d c, 09 c, 47 c, 10 c, 02 c, 01 c, 0b c, 0d c, 0f c, 0b c, 0b c, 09 c, 46 c, 10 c, 03 c, 02 c,
\ 05 c, 12 c, 10 c, 0f c, 07 c, 08 c, 46 c, 11 c, 01 c, 01 c, 05 c, 14 c, 0e c, 01 c, 01 c, 11 c,
\ 05 c, 08 c, 47 c, 12 c, 02 c, 17 c, 0d c, 01 c, 01 c, 13 c, 04 c, 07 c, 47 c, 0f c, 01 c, 02 c,
\ 02 c, 0e c, 07 c, 05 c, 0d c, 15 c, 02 c, 07 c, 48 c, 0e c, 02 c, 01 c, 02 c, 09 c, 0d c, 05 c,
\ 0a c, 05 c, 03 c, 06 c, 02 c, 07 c, 02 c, 07 c, 47 c, 0f c, 05 c, 08 c, 0f c, 05 c, 08 c, 04 c,
\ 0e c, 06 c, 02 c, 07 c, 48 c, 0e c, 03 c, 0c c, 05 c, 02 c, 07 c, 04 c, 08 c, 03 c, 06 c, 02 c,
\ 08 c, 06 c, 01 c, 07 c, 48 c, 0d c, 04 c, 09 c, 03 c, 0a c, 05 c, 04 c, 07 c, 02 c, 05 c, 09 c,
\ 03 c, 05 c, 03 c, 06 c, 48 c, 0d c, 05 c, 17 c, 03 c, 04 c, 08 c, 11 c, 02 c, 04 c, 03 c, 06 c,
\ 48 c, 0d c, 05 c, 12 c, 02 c, 05 c, 01 c, 04 c, 08 c, 12 c, 02 c, 02 c, 04 c, 05 c, 49 c, 0d c,
\ 05 c, 12 c, 03 c, 04 c, 01 c, 04 c, 09 c, 05 c, 01 c, 07 c, 01 c, 05 c, 01 c, 02 c, 03 c, 06 c,
\ 48 c, 0c c, 06 c, 12 c, 04 c, 02 c, 03 c, 03 c, 0b c, 01 c, 01 c, 01 c, 01 c, 06 c, 01 c, 08 c,
\ 05 c, 04 c, 49 c, 0c c, 09 c, 07 c, 03 c, 03 c, 0b c, 03 c, 10 c, 04 c, 05 c, 03 c, 07 c, 04 c,
\ 49 c, 0c c, 0a c, 08 c, 0e c, 04 c, 0e c, 02 c, 08 c, 04 c, 07 c, 04 c, 49 c, 0c c, 0a c, 0d c,
\ 0a c, 02 c, 10 c, 05 c, 02 c, 07 c, 06 c, 03 c, 49 c, 0f c, 0a c, 08 c, 0e c, 01 c, 24 c, 02 c,
\ 01 c, 01 c, 48 c, 0e c, 0a c, 02 c, 31 c, 01 c, 08 c, 04 c, 48 c, 0c c, 01 c, 02 c, 0a c, 02 c,
\ 39 c, 04 c, 48 c, 0d c, 01 c, 01 c, 0b c, 02 c, 38 c, 04 c, 48 c, 0f c, 45 c, 03 c, 49 c, 0f c,
\ 45 c, 03 c, 49 c, 0f c, 45 c, 03 c, 49 c, 0f c, 45 c, 03 c, 49 c, 04 c, 01 c, 0a c, 1d c, 02 c,
\ 26 c, 03 c, 4a c, 0e c, 1c c, 03 c, 26 c, 02 c, 4b c, 0f c, 1a c, 04 c, 26 c, 02 c, 4b c, 0f c,
\ 1a c, 01 c, 13 c, 01 c, 15 c, 02 c, 4b c, 10 c, 18 c, 02 c, 14 c, 01 c, 14 c, 02 c, 4c c, 02 c,
\ 01 c, 04 c, 01 c, 07 c, 18 c, 02 c, 04 c, 04 c, 0c c, 02 c, 13 c, 02 c, 4c c, 02 c, 01 c, 04 c,
\ 01 c, 07 c, 18 c, 02 c, 05 c, 03 c, 0d c, 02 c, 11 c, 03 c, 4c c, 01 c, 02 c, 0c c, 18 c, 03 c,
\ 01 c, 06 c, 01 c, 01 c, 04 c, 04 c, 03 c, 02 c, 11 c, 03 c, 4c c, 02 c, 01 c, 0c c, 19 c, 0c c,
\ 02 c, 05 c, 04 c, 02 c, 10 c, 03 c, 4d c, 01 c, 02 c, 0b c, 19 c, 13 c, 04 c, 02 c, 10 c, 03 c,
\ 4d c, 02 c, 03 c, 09 c, 19 c, 13 c, 05 c, 01 c, 10 c, 03 c, 4d c, 03 c, 02 c, 09 c, 19 c, 14 c,
\ 05 c, 01 c, 0f c, 03 c, 4f c, 0e c, 16 c, 16 c, 04 c, 01 c, 0f c, 03 c, 4f c, 0e c, 15 c, 18 c,
\ 03 c, 02 c, 0e c, 03 c, 51 c, 0a c, 01 c, 01 c, 14 c, 1b c, 02 c, 02 c, 0d c, 03 c, 51 c, 0e c,
\ 01 c, 01 c, 0f c, 1d c, 02 c, 02 c, 0c c, 02 c, 4f c, 01 c, 03 c, 0b c, 01 c, 03 c, 0a c, 01 c,
\ 03 c, 22 c, 0b c, 03 c, 50 c, 01 c, 02 c, 0e c, 01 c, 01 c, 08 c, 01 c, 02 c, 0f c, 07 c, 02 c,
\ 01 c, 0c c, 0a c, 03 c, 50 c, 11 c, 09 c, 02 c, 01 c, 0f c, 0d c, 0b c, 02 c, 02 c, 04 c, 04 c,
\ 52 c, 10 c, 09 c, 0f c, 08 c, 13 c, 06 c, 05 c, 53 c, 0f c, 09 c, 2a c, 06 c, 05 c, 54 c, 0e c,
\ 08 c, 18 c, 07 c, 0c c, 06 c, 04 c, 54 c, 0f c, 08 c, 0d c, 17 c, 05 c, 08 c, 04 c, 54 c, 0f c,
\ 08 c, 0f c, 14 c, 01 c, 01 c, 03 c, 09 c, 03 c, 55 c, 10 c, 07 c, 0c c, 02 c, 01 c, 16 c, 02 c,
\ 09 c, 04 c, 56 c, 0f c, 08 c, 0b c, 19 c, 02 c, 08 c, 05 c, 56 c, 10 c, 08 c, 0d c, 17 c, 01 c,
\ 08 c, 05 c, 56 c, 0e c, 0b c, 1d c, 0e c, 05 c, 58 c, 10 c, 0a c, 01 c, 02 c, 18 c, 0e c, 05 c,
\ 58 c, 0f c, 01 c, 01 c, 0c c, 19 c, 06 c, 02 c, 05 c, 05 c, 59 c, 11 c, 08 c, 01 c, 01 c, 01 c,
\ 02 c, 12 c, 01 c, 03 c, 02 c, 01 c, 03 c, 02 c, 06 c, 04 c, 5b c, 10 c, 0a c, 01 c, 02 c, 0b c,
\ 04 c, 08 c, 01 c, 02 c, 03 c, 02 c, 04 c, 05 c, 5c c, 12 c, 03 c, 02 c, 02 c, 01 c, 04 c, 09 c,
\ 05 c, 02 c, 04 c, 02 c, 06 c, 02 c, 02 c, 06 c, 5c c, 11 c, 02 c, 04 c, 02 c, 02 c, 05 c, 03 c,
\ 01 c, 02 c, 06 c, 02 c, 04 c, 03 c, 01 c, 01 c, 03 c, 09 c, 5d c, 11 c, 01 c, 04 c, 02 c, 04 c,
\ 04 c, 06 c, 02 c, 01 c, 03 c, 01 c, 06 c, 05 c, 02 c, 09 c, 5c c, 1d c, 03 c, 01 c, 01 c, 03 c,
\ 01 c, 03 c, 04 c, 01 c, 03 c, 12 c, 54 c, 04 c, 05 c, 1d c, 02 c, 02 c, 01 c, 02 c, 02 c, 03 c,
\ 01 c, 02 c, 01 c, 01 c, 02 c, 13 c, 52 c, 2f c, 01 c, 1d c, 53 c, 2f c, 01 c, 1c c, 55 c, 0f c,
\ 01 c, 2b c, 02 c, 0d c, 57 c, 38 c, 04 c, 0c c, 58 c, 37 c, 02 c, 02 c, 01 c, 0c c, 59 c, 13 c,
\ 01 c, 03 c, 01 c, 1f c, 01 c, 0d c, 5c c, 14 c, 01 c, 2f c, 5e c, 40 c, 61 c, 3f c, 62 c, 3d c,
\ 64 c, 3c c, 65 c, 3b c, 65 c, 3a c, 67 c, 39 c, 68 c, 38 c, 69 c, 36 c, 6b c, 35 c, 6c c, 2f c,
\ 01 c, 03 c, 6e c, 32 c, 6f c, 31 c, 70 c, 30 c, 72 c, 27 c, 01 c, 05 c, 74 c, 22 c, 01 c, 03 c,
\ 01 c, 05 c, 75 c, 21 c, 01 c, 02 c, 01 c, 02 c, 01 c, 03 c, 76 c, 1e c, 01 c, 01 c, 04 c, 02 c,
\ 01 c, 03 c, 77 c, 1d c, 01 c, 01 c, 04 c, 07 c, 77 c, 1d c, 03 c, 01 c, 02 c, 05 c, 79 c, 04 c,
\ 01 c, 06 c, 04 c, 0d c, 06 c, 05 c, 7a c, 07 c, 01 c, 01 c, 07 c, 0c c, 02 c, 06 c, 01 c, 01 c,
\ 7b c, 04 c, 01 c, 01 c, 0d c, 08 c, 02 c, 05 c, 02 c, 02 c, 7b c, 06 c, 0d c, 0c c, 04 c, 03 c,
\ 7b c, 05 c, 0e c, 07 c, 03 c, 02 c, 02 c, 04 c, 7c c, 02 c, 11 c, 02 c, 07 c, 01 c, 03 c, 04 c,
\ 7d c, 02 c, 11 c, 02 c, 0b c, 03 c, 7e c, 02 c, 1d c, 03 c, 7f c, 03 c, 1b c, 03 c, 80 c, 03 c,
\ 1a c, 03 c, 82 c, 01 c, 1b c, 03 c, 39 c,
\ decimal
\ 
\ variable point 0 point !
\ variable draw true draw !
\ : draw-face
\   clear
\   face bytes + face do
\     i c@
\     draw @ if
\       dup point @ + point @ do
\         i 160 /mod set
\       loop
\     then
\     draw @ invert draw !
\     point +!
\   loop
\   show ;
\ draw-face

\ --- incremental sixels ---------------------------------------------------------

\ if (r >= 255 && b <= 0 && g < 255) g += 5;
\ else if (g >= 255 && b <= 0 && r > 0) r -= 5;
\ else if (r <= 0 && g >= 255 && b < 255) b += 5;
\ else if (r <= 0 && b >= 255 && g > 0) g -= 5;
\ else if (g <= 0 && b >= 255 && r < 255) r += 5;
\ else if (r >= 255 && g <= 0 && b > 0) b -= 5;

\ : power ( y x -- ) \ non-standard
\   1 -rot
\   begin
\     dup 0= if 2drop exit then
\     1- -rot swap over * swap rot
\   again ;

variable r  100 r !
variable g    0 g !
variable b    0 b !
: next-color
       r @ 100 =  b @   0 =  and  g @ 100 <  and if  1 g +!
  else g @ 100 =  b @   0 =  and  r @   0 >  and if -1 r +!
  else r @    0=  g @ 100 =  and  b @ 100 <  and if  1 b +!
  else r @    0=  b @ 100 =  and  g @   0 >  and if -1 g +!
  else g @    0=  b @ 100 =  and  r @ 100 <  and if  1 r +!
  else r @ 100 =  g @   0 =  and  b @   0 >  and if -1 b +!
  then then then then then then ;
: emit-num s>d <# #s #> type ;
: set-color 
  ." #0;2;"
  r @ emit-num [char] ; emit
  g @ emit-num [char] ; emit
  b @ emit-num
  next-color ;

: home-cursor esc emit ." [H" ;
: clear esc emit ." [40m" esc emit ." [2J" home-cursor ;
: set ( x y -- )
  home-cursor
  esc emit ." P;1q"
  [char] " emit ." 1;1" \ 1:1 pad:pan ratio (square pixels)
  set-color
  dup 6 / 0 ?do [char] - emit loop \ to line
  6 mod 2 swap power 63 + \ sixel
  \ swap [char] ! emit emit-num 63 emit
  swap 0 ?do 63 emit loop \ to column \ TODO: use repeat protocol
  emit
  esc emit [char] \ emit
  ; \ 100 0 do loop ;

: show 100 0 do 10000 0 do loop loop ; \ pause
\ : show ;
