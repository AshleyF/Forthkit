#!/usr/bin/env bash

echo "Running simulator"
cat bootstrap.fs pixels.fs turtle-fixed.fs turtle-geometry-book.fs - | gforth debugger.fs