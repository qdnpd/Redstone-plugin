#!/bin/sh
# install.sh — flatten the redstone texture overlay onto a ClassiCube pack.
#
# Usage:
#   sh install.sh                 # run from the directory holding your pack zip
#   sh /path/to/packs/install.sh  # run from anywhere; it asks for what it can't find
#
# What it does:
#   1. finds your pack zip (default.zip / classicube.zip in the current
#      directory, otherwise asks for the path)
#   2. finds the overlay folder (the one containing 16x/ and 32x/,
#      otherwise asks for the path)
#   3. detects 16x vs 32x from the pack's own atlas size
#   4. backs the pack up to <pack>.bak, then replaces the redstone tiles
#      in terrain.png in place
#
# Needs: unzip, zip, ImageMagick (convert), and a POSIX shell.

# --- 1. the pack zip ----------------------------------------------------------
if [ -f default.zip ]; then
  P=default.zip
elif [ -f classicube.zip ]; then
  P=classicube.zip
else
  printf "No default.zip or classicube.zip in %s\n" "$PWD"
  printf "Path to your pack zip: "
  read P || exit 1
fi
[ -f "$P" ] || { echo "error: no such file: $P" >&2; exit 1; }

# --- 2. the overlay folder (this repo's packs/, with 16x/ and 32x/) -----------
if [ -d 16x ] && [ -d 32x ]; then
  R=.
else
  printf "Path to the packs/ folder (the one containing 16x/ and 32x/): "
  read R || exit 1
fi
[ -d "$R/16x" ] && [ -d "$R/32x" ] || {
  echo "error: $R does not contain 16x/ and 32x/" >&2; exit 1; }

# --- 3. detect 16x vs 32x from the pack's own atlas ---------------------------
W=$(unzip -p "$P" terrain.png | identify -format "%w" - 2>/dev/null)
case "$W" in
  256) S=16x ;;
  512) S=32x ;;
  *) echo "error: atlas width '$W' is neither 256 (16x) nor 512 (32x)" >&2; exit 1 ;;
esac
echo "Pack: $P  ($S, ${W}px atlas)"

# --- 4. backup, flatten, update the zip ---------------------------------------
cp "$P" "$P.bak" || exit 1
echo "Backup: $P.bak"

T=$(mktemp -d) && unzip -p "$P" terrain.png \
  | convert - "$R/$S/clear-mask.png" -compose DstIn -composite \
            "$R/$S/terrain.png" -compose over -composite "$T/terrain.png" \
  && zip -jq "$P" "$T/terrain.png" && rm -rf "$T" \
  && echo "Done — redstone tiles installed into $P" \
  || { echo "error: flatten failed, $P was not modified" >&2; exit 1; }
