#!/usr/bin/env bash

. ./image.sh
echo "Running bootstrap..."
cat ./bootstrap.4th ../../library/prelude.4th ../../library/pixels/pixels.4th - | ./machine