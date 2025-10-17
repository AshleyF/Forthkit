#!/usr/bin/env bash

echo "Running"
#cat pixels.fs turtle-fixed.fs turtle-geometry-book.fs - | ./machine
cat pixels.fs sixels.fs turtle-fixed.fs turtle-geometry-book.fs - | ./machine
#cat sixels-color.fs turtle-fixed.fs turtle-geometry-book.fs - | ./machine
