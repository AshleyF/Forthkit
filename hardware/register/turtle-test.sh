#!/usr/bin/env bash

. ./image.sh
echo "Running bootstrap..."
cat ./bootstrap.f ../../library/prelude.f pixels-adapter.f ../../library/pixels/pixels.f turtle-fixed-point.f ../../library/turtle/test.f - | ./machine