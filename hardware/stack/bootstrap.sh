#!/usr/bin/env bash

gforth -e "require kernel.fs write-boot-block bye"

echo "-- BOOTSTRAP NATIVE MACHINE ----------------------------------------------------"
echo
./build.sh && cat bootstrap.fs - | ./machine

# echo
# echo "-- BOOTSTRAP FORTH MACHINE -----------------------------------------------------"
# echo
# cat bootstrap.fs - | gforth -e "require machine.fs reboot"
