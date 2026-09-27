# Redstone Textures (tiles-only overlay)

This folder adds the **missing redstone block textures** to the
[Redstone plugin](https://github.com/qdnpd/Redstone-plugin) for
MCGalaxy / ClassiCube. The plugin ships block *definitions* (the JSONs in
`dat/`) but **no textures**, so redstone torches and levers render as
missing-texture blocks unless your texture pack already provides them.

This overlay provides the **full set of redstone tiles** the plugin
references, on a fully transparent atlas, so it can be pasted into
**any** pack without disturbing the rest.

## What's here

| File | Atlas | Tile size | Covers |
|------|-------|-----------|--------|
| `16x/terrain.png`  | 256×256 | 16 px | Standard / CC-default style packs |
| `32x/terrain.png`  | 512×512 | 32 px | Faithful-style 32x packs (e.g. Royalty³) |
| `16x/clear-mask.png` / `32x/clear-mask.png` | — | — | Helper for the install below: opaque everywhere *except* the redstone tiles |
| `install.sh` | — | — | One-command installer: finds your pack, detects 16x/32x, backs up, and flattens the overlay in place |

Both overlays contain **only** these twenty tiles (everything else is
fully transparent, alpha 0):

| Tile(s) | Block face |
|---------|-----------|
| 87–95   | Redstone wire / dust (9 connection states) |
| 114     | Redstone block |
| 115     | Redstone lamp — off |
| 116     | Redstone lamp — on |
| 117     | Lever (switch) — off |
| 118     | Lever (switch) — on |
| 119     | Redstone torch cap — off |
| 120     | Redstone torch cap — on |
| 121     | Redstone torch side — off |
| 122     | Redstone torch side — on |
| 123     | Repeater — off |
| 124     | Repeater — on |

## How to install

**Option A** (the `install.sh` script) is recommended if you have
`unzip`, `zip`, and ImageMagick; **Option B** is the manual GUI route.

### Option A — `install.sh` (no GUI)

```sh
sh install.sh
```

Run it from the directory that holds your pack zip (`default.zip` or
`classicube.zip`). It:

1. finds your pack zip in the current directory — if neither
   `default.zip` nor `classicube.zip` is there, it asks for the path;
2. finds the overlay folder (the `packs/` directory containing `16x/`
   and `32x/`) — if it's not the current directory, it asks for the
   path;
3. detects 16x vs 32x from your pack's own atlas size (256 px → 16x,
   512 px → 32x);
4. backs the pack up to `<pack>.bak`, then replaces the redstone tiles
   in `terrain.png` in place — clearing whatever was there before, so
   old/garbled redstone art from the base pack can't ghost through the
   overlay's transparent pixels.

You can also run it from anywhere: `sh /path/to/packs/install.sh` —
it will prompt for anything it can't find. Needs `unzip`, `zip`,
ImageMagick (`convert`; use `magick` on ImageMagick 7), and a POSIX
shell.

### Option A' — the one-liner (what the script does, by hand)

```sh
# from the directory holding your pack zip; S=16x or 32x
P=default.zip; S=16x

T=$(mktemp -d) && unzip -p "$P" terrain.png \
  | convert - "$S/clear-mask.png" -compose DstIn -composite \
            "$S/terrain.png" -compose over -composite "$T/terrain.png" \
  && zip -jq "$P" "$T/terrain.png" && rm -rf "$T"
```

(`-j` stores the file as `terrain.png` so `zip` *replaces* the existing
entry instead of adding a second one; `-q` is quiet. `convert` is
ImageMagick 6 — on ImageMagick 7 use `magick` instead.)

The mask is what makes this a true *replace* rather than a plain
overlay: without it, pixels the overlay leaves transparent keep the
pack's old redstone art underneath.

### Option B — manual (image editor)

1. Pick the file that matches your pack's tile size
   (16 px or 32 px).
2. Open your pack's `terrain.png` (inside `default.zip` /
   `classicube.zip`, or your custom pack folder) in an image editor
   that supports layers / alpha.
3. Open the matching overlay `terrain.png` here as a new layer on top.
4. Because the overlay is transparent everywhere except the redstone
   tiles, you can simply **merge / flatten** it onto your atlas — only
   the redstone tiles change, every other tile is untouched.
5. Save your pack's `terrain.png` **as PNG with alpha** (do not save as
   RGB / JPEG, or the transparency is destroyed).
6. Restart MCGalaxy / ClassiCube.

> Tip: in GIMP, *File → Open as Layers* the overlay onto your atlas,
> then *Image → Flatten Image*. In Photoshop, drag the overlay in as a
> layer and merge.

## Where the textures come from

All twenty tiles are based on **vanilla Minecraft 1.6.4** textures
(verified by comparison against the original 1.6.4 client assets):

| Tile(s) | Source |
|---------|--------|
| 87–95   | `blocks/redstone_dust_line.png` — the vanilla dust shape, recolored red (vanilla ships it white/gray and tints it red at runtime; the mod's wire needs the red baked in). The nine tiles are the same art at different brightness levels for the connection states. |
| 114     | `blocks/redstone_block.png` — pixel-for-pixel |
| 115     | `blocks/redstone_lamp_off.png` — pixel-for-pixel |
| 116     | `blocks/redstone_lamp_on.png` — pixel-for-pixel |
| 117/118 | `blocks/lever.png` — pixel-for-pixel |
| 119/120 | `blocks/redstone_torch_off.png` / `redstone_torch_on.png` (the 4×4 top-cap detail) — pixel-for-pixel |
| 121/122 | `blocks/redstone_torch_off.png` / `redstone_torch_on.png` (the stick) |
| 123/124 | `blocks/repeater_off.png` / `repeater_on.png` — pixel-for-pixel |

Tile 121 (torch off side) is the only tile with an addition: the top
row of the stick (2 px at 16x, 8 px at 32x) is filled with the stick's
own brown so the torch top is not see-through. No other pixels differ
from vanilla anywhere in the set.

Per the [Minecraft Usage Guidelines](https://www.minecraft.net/en-us/usage-guidelines),
this is a non-commercial fan work.

---

NOT AN OFFICIAL MINECRAFT PRODUCT. NOT APPROVED BY OR ASSOCIATED WITH
MOJANG OR MICROSOFT.

Textures © Mojang / Microsoft (vanilla Minecraft 1.6.4), used as a
non-commercial fan work.
