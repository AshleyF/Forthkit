#!/usr/bin/env bash

echo "Building and running test image..."
cat ./assembler.4th ./test.4th | python ../../interpreter/interpreter.py
. ./machine.sh
./machine
