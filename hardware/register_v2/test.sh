#!/usr/bin/env bash

echo "Testing"
cat ttester.fs tests.fs - | ./machine
#cat bootstrap.fs ttester.fs tests.fs - | gforth debugger.fs