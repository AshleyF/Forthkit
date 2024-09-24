#!/usr/bin/env bash

echo "Building test image..."
cat ./assembler.f ./test.f | python ../../interpreter/interpreter.py
. ./machine.sh
echo "Running test image (type something in lowercase and press ENTER)"
./machine
