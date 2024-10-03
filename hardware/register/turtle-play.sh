#!/usr/bin/env bash

. ./machine.sh
. ./kernel.sh
echo "Running turtle play..."
cat ./bootstrap.f ../../library/prelude.f pixels-adapter.f ../../library/pixels/pixels.f turtle-fixed-point.f - | ./machine