#!/usr/bin/env bash

echo "Building machine..."
rm -f ./machine
gcc -Wall -O3 -std=c99 -o ./machine ./machine.c

echo "Building image..."
rm -f ./block0.bin
echo "write-boot-block bye" | cat bootstrap.fs - | gforth debugger.fs

cat kernel-meta.fs - | ./machine

# ./machine