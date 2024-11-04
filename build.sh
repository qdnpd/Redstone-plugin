#!/bin/bash

csc /target:library /r:MCGalaxy_.dll /out:bin/Redstone.dll src/*.cs

for file in dat/*
do
    if [ "$file" = "dat/config.json" ]
    then
        cp $file bin/Redstone/
    else
        cp $file bin/Redstone/Data
    fi
done
