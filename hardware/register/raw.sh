#!/usr/bin/env bash

. ./machine.sh
stty raw -echo # immediate key relay mode
cat | ./machine
stty sane # normal terminal mode
