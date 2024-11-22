#!/usr/bin/env bash

rm -f block1.bin # TODO remove
. ./kernel.sh
. ./machine.sh
echo "Running bootstrap..."
cat ./bootstrap.f - | ./machine