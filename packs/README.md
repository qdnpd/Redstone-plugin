# Redstone Textures (tiles-only overlay)

This folder adds the **missing redstone block textures** to the
[Redstone plugin](https://github.com/qdnpd/Redstone-plugin) for
MCGalaxy / ClassiCube. The plugin ships block *definitions* (the JSONs in
`dat/`) but **no textures**, so redstone torches and levers render as
missing-texture blocks unless your texture pack already provides them.

This overlay provides just the six redstone tiles, on a fully transparent
atlas, so it can be pasted into **any** pack without disturbing the rest.

## What's here

| File | Atlas | Tile size | Covers |
|------|-------|-----------|--------|
| `16x/terrain.png`  | 256×256 | 16 px | Standard / CC-default style packs |
| `32x/terrain.png`  | 512×512 | 32 px | Faithful-style 32x packs (e.g. Royalty³) |

Both files contain **only** these six tiles (everything else is
fully transparent, alpha 0):

| Tile | Block face |
|------|-----------|
| 117  | Lever (switch) — off |
| 118  | Lever (switch) — on |
| 119  | Redstone torch cap — off |
| 120  | Redstone torch cap — on |
| 121  | Redstone torch side — off |
| 122  | Redstone torch side — on |

## How to install (paste into your pack)

1. Pick the file that matches your pack's tile size
   (16 px or 32 px).
2. Open your pack's `terrain.png` (inside `default.zip` /
   `classicube.zip`, or your custom pack folder) in an image editor
   that supports layers / alpha.
3. Open the matching overlay `terrain.png` here as a new layer on top.
4. Because the overlay is transparent everywhere except the six tiles,
   you can simply **merge / flatten** it onto your atlas — only the six
   redstone tiles change, every other tile is untouched.
5. Save your pack's `terrain.png` **as PNG with alpha** (do not save as
   RGB / JPEG, or the transparency is destroyed).
6. Restart MCGalaxy / ClassiCube.

> Tip: in GIMP, *File → Open as Layers* the overlay onto your atlas,
> then *Image → Flatten Image*. In Photoshop, drag the overlay in as a
> layer and merge.

## Where the textures come from

All six tiles are **100% vanilla Minecraft 1.6.4** textures,
pixel-for-pixel (verified by comparison against the original 1.6.4
`terrain.png`):

- Lever off/on  ← `blocks/lever.png`
- Torch off/on  ← `blocks/redstone_torch_off.png` / `redstone_torch_on.png`
- Torch caps    ← `blocks/redstone_torch_on.png` / `redstone_torch_off.png`
  (the 4×4 top-cap detail)

Per the [Minecraft Usage Guidelines](https://www.minecraft.net/en-us/usage-guidelines),
this is a non-commercial fan work.

---

NOT AN OFFICIAL MINECRAFT PRODUCT. NOT APPROVED BY OR ASSOCIATED WITH
MOJANG OR MICROSOFT.

Textures © Mojang / Microsoft (vanilla Minecraft 1.6.4), used as a
non-commercial fan work.
