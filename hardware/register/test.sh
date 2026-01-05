#!/usr/bin/env bash

echo "Testing"
./build.sh && cat bootstrap.fs ttester.fs - | ./machine
#cat tests.fs ttester.fs bootstrap.fs - | ./machine