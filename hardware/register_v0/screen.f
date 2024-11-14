( screen buffer )

80 constant width
40 constant height
width height * constant size

here size 2 * allot constant buffer0
here size 2 * allot constant buffer1

: clear size 0 do 32 buffer1 i 2 * + ! loop ;

: update
  vthide
  height 0 do
    width 0 do
      width j * i + 2 *
      dup buffer0 +
      swap buffer1 +
      2dup @ swap @ <> if
        @ dup i j vthome emit
        swap !
      else
        drop drop
      then
    loop
  loop
  0 height 1 + vthome vtshow ;

: coord width * + 2 * buffer1 + ;
: set coord ! ;
: get coord @ ;
