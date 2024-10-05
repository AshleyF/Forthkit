#!/usr/bin/env bash

stty raw # immediate key relay mode
cat | ./machine
stty sane # normal terminal mode