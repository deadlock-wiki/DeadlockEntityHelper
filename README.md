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

### To extract entity locations:

1. `dotnet publish`
`./DeadlockEntityHelper Deadlock/game/citadel/maps/dl_midtown.vpk citadel_breakable_prop_wooden_crate`

use `citadel_breakable_item_container` for golden statues

### To generate a map image

1. Dump the output of `DeadlockEntityHelper` into two json files: `crates.json` and `golden_statues.json`
2. `pip install -r requirements.txt`
3. `python create_graph.py`