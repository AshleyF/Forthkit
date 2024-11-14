#!/usr/bin/env bash

. ./kernel.sh
. ./machine.sh
echo "Running bootstrap..."
cat ./bootstrap.f - | ./machine