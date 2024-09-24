#!/usr/bin/env bash

. ./image.sh
echo "Running bootstrap..."
cat ./bootstrap.f ../../library/prelude.f pixels-adapter.f ../../library/pixels/pixels.f ../../library/pixels/test.f - | ./machine