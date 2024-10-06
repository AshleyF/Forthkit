#!/usr/bin/env bash

stty raw -echo # immediate key relay mode
cat | ./machine
stty sane # normal terminal mode
