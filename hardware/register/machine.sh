#!/usr/bin/env bash

echo "Building machine..."
rm -f machine
gcc -Wall -O3 -std=c99 -o ./machine ./machine.c
echo "Done"
