#!/usr/bin/env bash

echo "Building outer interpreter boot image..."
. ./outer.sh

if [ -f ./machine ]; then
  echo "Building machine..."
  . ./machine.sh
fi

echo "Bootstrapping..."
cat ./bootstrap.4th - | ./machine
