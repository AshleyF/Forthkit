#!/usr/bin/env bash

. ./kernel.sh
echo "Running bootstrap..."
cat ./bootstrap.f ../../library/prelude.f - | ./machine