#!/bin/bash

if [[ "$1" = "survival" ]]
then
    DIR="survival"
    OPTIONS="/r:simplesurvival.dll /r:door.dll /out:bin/$DIR/SurvivalRedstone.dll /define:SURVIVAL "
else
    DIR="default"
    OPTIONS="/out:bin/$DIR/Redstone.dll"
fi

csc /target:library /r:MCGalaxy_.dll $OPTIONS src/*.cs

for file in dat/$DIR/*
do
    if [ "$file" = "dat/$DIR/config.json" ]
    then
        rsync -a $file bin/$DIR/Redstone/
    else
        rsync -a $file bin/$DIR/Redstone/Data
    fi
done
