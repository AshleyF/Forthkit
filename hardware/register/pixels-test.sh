#!/usr/bin/env bash

. ./machine.sh
. ./kernel.sh
echo "Running pixels test..."
cat ./bootstrap.f ../../library/prelude.f pixels-adapter.f ../../library/pixels/pixels.f ../../library/pixels/test.f - | ./machine