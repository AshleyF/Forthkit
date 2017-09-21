#!/usr/bin/env bash

echo "Building outer interpreter boot image..."
cat ./assembler.4th ./outer.4th | python ../../interpreter/interpreter.py
. ./machine.sh
echo "Running bootstrap..."
cat ./bootstrap.4th - | ./machine
