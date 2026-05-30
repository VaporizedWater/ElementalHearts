# notes/

Developer scratch — design docs, raw source/material lists, and reference material. None of
this is compiled or shipped: the folder is excluded from the build by `notes\*` in
[`build.txt`](../build.txt) (the packaged `.tmod`) and `<Compile Remove="notes\**\*.cs" />`
in [`ElementalHearts.csproj`](../ElementalHearts.csproj) (`dotnet build`).

| File | What it is |
|------|------------|
| `antigravity.md`, `Biomes.md`, `Munchies.md`, `MusicDisplay.md` | Design / integration notes referenced from the matching `*.cs` doc-comments. |
| `Consumable_source.txt`, `Munchies_source.txt`, `pngnames.txt`, `pngs.txt` | Raw material/sprite lists used while authoring hearts. |
| `patch.py` | One-off dev script. |
| `MunchiesCenteredUIImage.reference.cs` | A reference copy of Munchies' `UIElements.CenteredUIImage`. It documents the private field/method names that the reflection detour in `Common/CrossMod/Munchies/MunchiesIntegration.cs` targets. **Reference only — not compiled.** |
| `iconOld.png` | A previous mod icon, kept for reference. |
