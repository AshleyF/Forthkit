#!/usr/bin/env bash

cat ../prelude.f ../pixels/pixels.f ./turtle.f - | python ../../interpreter/interpreter.py
