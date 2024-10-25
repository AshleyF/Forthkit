#!/usr/bin/env bash

. ./kernel.sh
echo "Building meta-circular image..."
cat ./bootstrap.f assembler-adapter.f ./assembler.f kernel-adapter.f kernel.f | ./machine