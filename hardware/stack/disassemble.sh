#!/usr/bin/env bash

echo "-- DISASSEMBLE -----------------------------------------------------------------"
echo
rm -f ./disassemble
gcc -Wall -O3 -std=c99 -fno-common -o ./disassemble ./disassemble.c
./disassemble