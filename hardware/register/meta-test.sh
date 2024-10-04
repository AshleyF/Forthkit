#!/usr/bin/env bash

rm ./image.bin
rm ./kernel.bin

. ./kernel.sh
mv ./image.bin ./kernel.bin

. ./meta.sh

diff -s <(xxd image.bin) <(xxd kernel.bin)

rm ./kernel.bin

echo "Running turtle test..."
cat ./bootstrap.f ../../library/prelude.f pixels-adapter.f ../../library/pixels/pixels.f turtle-fixed-point.f ../../library/turtle/test.f - | ./machine