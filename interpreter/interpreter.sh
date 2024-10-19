#!/usr/bin/env bash

cat ../../library/prelude.f - | python $(dirname $0)/interpreter.py
