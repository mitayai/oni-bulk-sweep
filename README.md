# Bulk Sweep by Type

A mod for [Oxygen Not Included](https://www.klei.com/games/oxygen-not-included).

- Select any sweepable item and use the new **"Sweep All: `<Material>`"** button on its info
  panel to mark every item of that same material anywhere on the map for Sweep, at that
  item's current priority.
- **Bulk Dig** — a new toolbar tool (added alongside the vanilla tools). Pick a priority,
  click one tile of solid terrain, and every tile of that same material on the map gets a
  Dig marker at that priority.

## Building

Requires the [.NET SDK](https://dotnet.microsoft.com/download) and a local install of
Oxygen Not Included. Edit `GameFolder` at the top of `BulkSweepByType.csproj` if your game
isn't installed at the default Steam location, then:

```
dotnet build
```

This produces `BulkSweepByType.dll` (with [PLib](https://www.nuget.org/packages/PLib)
merged in via ILRepack) right next to `mod.yaml`, ready to copy into your
`mods/dev/BulkSweepByType/` folder.

## Dependencies

- [Harmony](https://github.com/pardeike/Harmony) (bundled with the game)
- [PLib](https://github.com/peterhaneve/ONIMods) by Peter Han (NuGet package, merged into
  the output DLL — not a separate runtime dependency)
