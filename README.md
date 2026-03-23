>DeadlockEntityHelper
>Copyright (C) 2026 Michael Manis
>
>This program is free software: you can redistribute it and/or modify
>it under the terms of the GNU Affero General Public License as published by
>the Free Software Foundation, either version 3 of the License, or
>(at your option) any later version.
>
>This program is distributed in the hope that it will be useful,
>but WITHOUT ANY WARRANTY; without even the implied warranty of
>MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
>GNU Affero General Public License for more details.
>
>You should have received a copy of the GNU Affero General Public License
>along with this program.  If not, see <https://www.gnu.org/licenses/>.


# Deadlock Entity Helper

Extract entity properties from the Deadlock map (currently just "Midtown").

## Usage

For arguments, run `./DeadlockEntityHelper --help`
For command arguments, run e.g `./DeadlockEntityHelper extract --help`

### Example: To extract the positions of all wooden crates on the map:

`./DeadlockEntityHelper extract Deadlock/game/citadel/maps/dl_midtown.vpk citadel_breakable_prop_wooden_crate subclass_name string origin vector3`

## Building

### For release

`dotnet publish`

The resulting executable will be in `DeadlockEntityHelper/bin/Release/net10.0/<your platform>/publish/`

## Useful entities

|Entity| Description    | Useful properties                    |
|------|----------------|--------------------------------------|
|citadel_breakable_prop_wooden_crate| Wooden crates  | initial_spawn_time_override (double) |
|citadel_breakable_item_container| Golden Statues | initial_spawn_time_override (double) |

## Development

### Versioning
1. Push the change
2. Increment new versions in `DeadlockEntityHelper/DeadlockEntityHelper.csproj`
3. Tag new releases by running:
   1. `NEW_VER=vX.Y.Z`
   2. `git tag -a $NEW_VER -m "$NEW_VER release"`
   2. Push the tag by running `git push origin $NEW_VER`