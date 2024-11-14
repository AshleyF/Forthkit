#!/usr/bin/env bash

. ./kernel.sh
. ./machine.sh
echo "Running turtle play..."
cat ./bootstrap.f ../../library/prelude-machine.f pixels-adapter.f ../../library/pixels/pixels.f turtle-fixed-point.f - | ./machine