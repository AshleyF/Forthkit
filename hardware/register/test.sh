#!/usr/bin/env bash

echo "Assembling capitalize..."
cat ./assembler.4th ./test.4th | python ../../interpreter/interpreter.py

if [ ! -f ./machine.exe ]; then
    echo "Building machine..."
    . ./machine.sh
fi

echo "Running machine..."
./machine.exe
