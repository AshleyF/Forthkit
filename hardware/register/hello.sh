#!/usr/bin/env bash

gforth -e "require hello.fs write-boot-block bye"
./machine.sh