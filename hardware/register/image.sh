#!/usr/bin/env bash

echo "Building boot image..."
rm -f image.bin
cat ./assembler.4th ./interpreter.4th | python ../../interpreter/interpreter.py
. ./machine.sh