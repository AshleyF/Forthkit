#!/usr/bin/env bash

echo "Building boot image..."
rm -f image.bin
cat ./assembler.f ./kernel.f | python ../../interpreter/interpreter.py # build kernel image