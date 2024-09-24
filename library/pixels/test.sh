#!/usr/bin/env bash

cat ../prelude.f ./pixels.f ./test.f | python ../../interpreter/interpreter.py
