#!/usr/bin/env bash

. ./kernel.sh
. ./machine.sh
echo "Building meta-circular image..."
cat ./bootstrap.f ./assembler.f kernel.f | ./machine