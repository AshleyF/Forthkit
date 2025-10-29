#!/usr/bin/env bash

echo "== TESTING MACHINE ============================================================="
echo
gforth machine-test.fs

echo
echo
echo "== HELLO WORLD ================================================================="
echo
./hello.sh

echo
echo "== RUN KERNEL (manually test, then bye) ========================================"
echo
./kernel.sh

echo
echo "== RUN BOOTSTRAP (manually test, then bye) ====================================="
echo
./bootstrap.sh
