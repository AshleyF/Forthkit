#!/usr/bin/env bash

cat ../prelude-interpreter.f ./pixels.f ./test.f | python ../../interpreter/interpreter.py
