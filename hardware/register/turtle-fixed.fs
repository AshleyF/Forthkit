here
255 c, 255 c, 255 c, 255 c, 254 c, 254 c, 254 c, 253 c, 253 c, 252 c,
251 c, 250 c, 249 c, 248 c, 247 c, 246 c, 245 c, 244 c, 242 c, 241 c,
240 c, 238 c, 236 c, 235 c, 233 c, 231 c, 229 c, 227 c, 225 c, 223 c,
221 c, 218 c, 216 c, 214 c, 211 c, 209 c, 206 c, 203 c, 201 c, 198 c,
195 c, 192 c, 189 c, 186 c, 183 c, 180 c, 177 c, 174 c, 170 c, 167 c,
164 c, 160 c, 157 c, 153 c, 149 c, 146 c, 142 c, 138 c, 135 c, 131 c,
127 c, 123 c, 119 c, 115 c, 111 c, 107 c, 103 c,  99 c,  95 c,  91 c,
 87 c,  82 c,  78 c,  74 c,  70 c,  65 c,  61 c,  57 c,  52 c,  48 c,
 43 c,  39 c,  35 c,  30 c,  26 c,  21 c,  17 c,  12 c,   8 c,   3 c,   0 c,  ( TODO <-- should be -1 )
constant table
table .
: cos
  abs 360 mod dup 180 >= if
    360 swap -
  then
  dup 90 >= if
    -1 180 rot -
  else
    1 swap
  then
  table + c@ 1+ *
;

: sin 90 - cos ;

variable x variable y variable theta
variable dx variable dy

\ : point-x x @ 8 rshift width 2/ + ; / TODO: rshift doesn't handle sign extension
\ : point-y y @ 8 rshift height 2/ swap - ; / TODO: rshift doesn't handle sign extension
: point-x x @ 128 + 256 / width 2/ + ;
: point-y y @ 128 + 256 / height 2/ swap - ;
: valid-x? point-x 0 width 1- within ;
: valid-y? point-y 0 height 1- within ;
: valid? valid-x? valid-y? and ;
: plot valid? if point-x point-y set then ;

: go 8 lshift y ! 8 lshift x ! ;
\ : go 256 * y ! 256 * x ! ;
: head dup theta ! dup cos dx ! sin dy ! ;
: pose head go ;
: home 0 0 90 pose ;

: start clear home ;
: turn theta @ + head ;
: move 0 do dx @ x +! dy @ y +! plot loop ;
: jump dup dx @ * x +! dy @ * y +! ;

( drawing things! )

: angle ( sides -- angle ) 360 swap / ;
: draw ( len angle sides -- ) 0 do 2dup turn move loop 2drop ;
: polygon ( len sides -- ) dup angle swap draw ;

: triangle ( len -- )  3 polygon ;
: square   ( len -- )  4 polygon ;
: pentagon ( len -- )  5 polygon ;
: hexagon  ( len -- )  6 polygon ;
: circle   ( len -- ) 36 polygon ;

: shapes start 30 30 go 50 hexagon 50 pentagon 50 square 50 triangle show ;

: star ( len -- ) 144 5 draw ;

: burst start 60 60 go 60 0 do i 6 * head 0 0 go 80 move loop show ;

: squaral start 35 -70 go 20 0 do 140 move 126 turn loop show ;

: rose start 0 54 0 do 2 + dup move 84 turn loop drop show ;

: spiral-rec 1 + dup move 92 turn dup 110 < if tail-recurse then drop ;
: spiral start 1 spiral-rec show ;

\ shim for old version of gforth (e.g. apt install gforth gives 0.7.3)
\ : [: ( -- ) postpone ahead :noname ; immediate compile-only
\ : ;] ( -- xt ) postpone ; ] postpone then latestxt postpone literal ; immediate compile-only

: spin dup angle swap 0 do 2dup turn execute loop 2drop ;
\ : stars start [: 80 star ;] 3 spin show ;

\ : spiro start [: 6 circle ;] 20 spin show ;

: arc 0 do 2dup turn move loop 2drop ;
: petal 2 0 do 4 6 16 arc 1 -6 16 arc 180 turn loop ;
\ : flower start [: petal ;] 15 spin show ;

\ : demo burst shapes squaral spiro stars rose flower spiral ;
: demo burst shapes squaral rose spiral ;
\ demo

( Kock curve experiment )

variable 'koch
: curve dup 0 > if 2dup 1 - swap 3 / swap 'koch @ execute else drop move then ;
: koch 2dup curve -60 turn 2dup curve 120 turn 2dup curve -60 turn 2dup curve 2drop ;
' koch 'koch !

\ start
\ -80 0 go
\ 50 1 curve
\ show

\ start
\ -80 0 go
\ 100 2 curve
\ show

\ start
\ -80 0 go
\ 200 3 curve
\ show

\ start
\ -80 0 go
\ 400 4 curve
\ show

: snowflake 3 0 do 2dup curve 120 turn loop 2drop ;

\ start
\ -80 0 go
\ 80 0 snowflake
\ show

\ start
\ -80 0 go
\ 50 1 snowflake
\ show

\ start
\ -80 0 go
\ 50 2 snowflake
\ show
