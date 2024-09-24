#!/usr/bin/env bash

echo "Building boot image..."
rm -f image.bin
cat ./assembler.f ./interpreter.f | python ../../interpreter/interpreter.py
. ./machine.sh