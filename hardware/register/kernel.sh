#!/usr/bin/env bash

. ./machine.sh
echo "Building boot image..."
rm -f block0.bin
cat ./assembler.f ./kernel.f | python ../../interpreter/interpreter.py # build kernel image