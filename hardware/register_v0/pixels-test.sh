#!/usr/bin/env bash

. ./kernel.sh
. ./machine.sh
echo "Running pixels test..."
cat ./bootstrap.f ../../library/prelude-machine.f pixels-adapter.f ../../library/pixels/pixels.f ../../library/pixels/test.f - | ./machine
