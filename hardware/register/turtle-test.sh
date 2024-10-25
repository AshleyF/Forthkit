#!/usr/bin/env bash

. ./kernel.sh
echo "Running turtle test..."
cat ./bootstrap.f ../../library/prelude-machine.f pixels-adapter.f ../../library/pixels/pixels.f turtle-fixed-point.f ../../library/turtle/test.f - | ./machine
