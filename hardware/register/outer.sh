#!/usr/bin/env bash

echo "Building and running outer interpreter boot image..."
cat ./assembler.4th ./outer.4th | python ../../interpreter/interpreter.py
. ./machine.sh
./machine
