#!/bin/bash

csc /target:library /r:MCGalaxy_.dll /r:simplesurvival.dll /r:door.dll /out:bin/XRedstone.dll /define:SURVIVAL src/*.cs

for file in dat/*
do
    if [ "$file" = "dat/config.json" ]
    then
        cp $file bin/Redstone/
    else
        cp $file bin/Redstone/Data
    fi
done

cp -r bin/* ~/server/MCGalaxy/plugins
