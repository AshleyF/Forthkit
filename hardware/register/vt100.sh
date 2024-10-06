#!/usr/bin/env bash

. ./kernel.sh
echo "Running bootstrap with VT100..."
cat ./bootstrap.f ./vt100.f - | ./machine