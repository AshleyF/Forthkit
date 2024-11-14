( snake game )

   32 constant blank
 9632 constant square
 9650 constant up
 9664 constant left
 9660 constant down
 9654 constant right
11044 constant circle

variable hx 40 hx !
variable hy 20 hy !

variable dx 0 dx !
variable dy 0 dy !
variable head up head !

variable tx 40 tx !
variable ty 20 ty !

variable len 8 len !
variable count 0 count !

32768 constant rndmod
25173 constant rndmult
13849 constant rndinc

variable rndseed

: rand
  rndseed @ rndmult * rndinc + rndmod mod
  dup rndseed !
  dup 8 2/ xor ( Add some bit mixing )
;

: init
  clear
  vtclear vthide
  vtblue vtblack vtcolors
  vtbright vtattrib
;

: border
  80 0 do square i 0 set square i 39 set loop
  40 0 do square 0 i set square 79 i set loop
;

: touching
  2dup      1+      get blank <> if      1+      true else
  2dup      1-      get blank <> if      1-      true else
  2dup swap 1+ swap get blank <> if swap 1+ swap true else
  2dup swap 1- swap get blank <> if swap 1- swap true else
  2drop false
  then then then then
;

: food
  rand 80 mod rand 40 mod 2dup
  touching if 2drop recurse
  else circle -rot set then
;

variable speed 10 speed !
: delay speed @ 0 do 10000 0 do loop loop ;

: snake
  square hx @ hy @ set ( make head into body )
  hx @ dx @ + hx !
  hy @ dy @ + hy !
  count @ 1+ count !
  hx @ hy @ get circle = if len @ 2 * len ! food speed @ 1- speed ! then ( eat food )
  head @ hx @ hy @ set ( draw head )
;

: tail
  count @ len @ >= if
    count @ 1- count !
    blank tx @ ty @ set ( erase tail )
    tx @ ty @ touching if ( should always be true! )
      ty ! tx ! ( make new tail )
    then
  then
;

: input
  key
  dup char x = if halt else
  dup char w = if  0 dx ! -1 dy !    up head ! else
  dup char a = if -1 dx !  0 dy !  left head ! else
  dup char r = if  0 dx !  1 dy !  down head ! else
  dup char s = if  1 dx !  0 dy ! right head ! else
  then then then then then drop
  dx @ dy @ or 0 <> if
    snake tail update delay
  then
  recurse-tail
;

: start init border update vtgreen vtfg food update input ;

start
