#!/usr/bin/env bash

. ./kernel.sh
echo "Running snack with raw terminal input..."
stty raw -echo # immediate key relay mode
cat ./bootstrap.f ./vt100.f ./snake.f - | ./machine
stty sane # normal terminal mode