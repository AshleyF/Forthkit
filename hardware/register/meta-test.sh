#!/usr/bin/env bash

rm ./image.bin
rm ./kernel.bin

. ./kernel.sh # build kernel using Python
mv ./image.bin ./kernel.bin

. ./meta.sh # build kernel using machine

diff -s <(xxd image.bin) <(xxd kernel.bin)

# go around again!
cat ./bootstrap.f assembler-adapter.f ./assembler.f kernel-adapter.f kernel.f | ./machine

diff -s <(xxd image.bin) <(xxd kernel.bin)

rm ./kernel.bin

echo "Running turtle test..."
cat ./bootstrap.f ../../library/prelude.f pixels-adapter.f ../../library/pixels/pixels.f turtle-fixed-point.f ../../library/turtle/test.f - | ./machine