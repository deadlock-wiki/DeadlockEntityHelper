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

Not much of a helper right now; currently this is used to extract the locations of crates and golden statues

## Usage

For arguments, run `./DeadlockEntityHelper --help`
For command arguments, run e.g `./DeadlockEntityHelper extract --help`

### Example: To extract wooden crate locations on the Midtown map:

1. `dotnet restore`
2. `dotnet run extract Deadlock/game/citadel/maps/dl_midtown.vpk citadel_breakable_prop_wooden_crate subclass_name string origin vector3`

## Useful entities

|Entity| Description    | Useful properties                    |
|------|----------------|--------------------------------------|
|citadel_breakable_prop_wooden_crate| Wooden crates  | initial_spawn_time_override (double) |
|citadel_breakable_item_container| Golden Statues | initial_spawn_time_override (double) |