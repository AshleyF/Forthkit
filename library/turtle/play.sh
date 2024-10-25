#!/usr/bin/env bash

cat ../prelude-interpreter.f ../pixels/pixels.f ./turtle.f - | python ../../interpreter/interpreter.py
