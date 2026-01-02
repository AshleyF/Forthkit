#!/usr/bin/env bash

rm -f ./machine
gcc -Wall -O3 -std=c99 -fno-common -o ./machine ./machine.c
