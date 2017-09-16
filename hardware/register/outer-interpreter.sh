#!/usr/bin/env bash

echo "Assembling outer interpreter..."
cat ./assembler.4th ./outer-interpreter.4th | python ../../interpreter/interpreter.py

if [ ! -f ./machine.exe ]; then
    echo "Building machine..."
    . ./machine.sh
fi

echo "Running machine..."
./machine.exe
