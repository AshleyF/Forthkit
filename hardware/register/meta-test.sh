#!/usr/bin/env bash

rm ./block0.bin
rm ./kernel.bin

. ./kernel.sh # build kernel using Python
mv ./block0.bin ./kernel.bin

. ./meta.sh # build kernel using machine

diff -s <(xxd block0.bin) <(xxd kernel.bin)

# go around again!
rm ./block0.bin
cat ./bootstrap.f ./assembler.f ./kernel.f | ./machine

diff -s <(xxd block0.bin) <(xxd kernel.bin)

rm ./kernel.bin
 
# echo "Running turtle test..."
# cat ./bootstrap.f ../../library/prelude-machine.f pixels-adapter.f ../../library/pixels/pixels.f turtle-fixed-point.f ../../library/turtle/test.f - | ./machine