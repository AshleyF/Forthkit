#!/usr/bin/env bash

. ./kernel.sh
. ./machine.sh
echo "Running bootstrap with screen..."
cat ./bootstrap.f vt100.f ./screen.f - | ./machine