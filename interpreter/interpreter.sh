#!/usr/bin/env bash

cat ../../library/prelude-interpreter.f - | python $(dirname $0)/interpreter.py
