#!/usr/bin/env bash

gforth -e "require kernel.fs write-boot-block bye"
./machine.sh