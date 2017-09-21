#!/usr/bin/env bash

if [ ! -f ./machine ]; then
  echo "Building machine..."
  gcc -Wall -O3 -std=c99 -o ./machine ./machine.c
  echo "Done"
fi
