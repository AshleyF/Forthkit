#!/usr/bin/env bash

. ./kernel.sh
. ./machine.sh
echo "Running snake with raw terminal input..."
stty raw -echo # immediate key relay mode
cat ./bootstrap.f ../../library/prelude-machine.f ./vt100.f ./screen.f ./snake.f - | ./machine
stty sane # normal terminal mode