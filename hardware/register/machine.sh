#!/usr/bin/env bash

echo "-- NATIVE MACHINE --------------------------------------------------------------"
echo
./build.sh && ./machine

echo
echo "-- FORTH MACHINE ---------------------------------------------------------------"
echo
gforth -e "require machine.fs reboot"