#!/usr/bin/env bash

. ./machine.sh
. ./kernel.sh
echo "Building meta-circular image..."
cat ./bootstrap.f assembler-adapter.f ./assembler.f test.f | ./machine