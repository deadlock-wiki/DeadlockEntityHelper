"""
    DeadlockEntityHelper
    Copyright (C) 2026 Michael Manis

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU Affero General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU Affero General Public License for more details.

    You should have received a copy of the GNU Affero General Public License
    along with this program.  If not, see <https://www.gnu.org/licenses/>.
"""

import json
import matplotlib.pyplot as plt
from matplotlib.lines import Line2D

def crates():
    with open("crates.json") as f:
        data = json.load(f)

    print("There are", len(data), "crates on the map")

    x_coords = []
    y_coords = []
    colors = []
    for entry in data:
        x_coords.append(entry["origin"][0])
        y_coords.append(entry["origin"][1])
        if entry["scales"][0] <= 0.5:
            colors.append("blue")
        elif entry["initial_spawn_time_override"] > 0:
            colors.append("green")
        else:
            colors.append("#cc5500")  # burnt orange

    legend_elements = [
        Line2D([0], [0], marker="o", color="w", label="Vent access required",
               markerfacecolor="blue", markersize=10),
        Line2D([0], [0], marker="o", color="w", label="Mid-Boss Crate (spawns after 10 minutes)",
               markerfacecolor="green", markersize=10),
        Line2D([0], [0], marker="o", color="w", label="Normal Crate",
               markerfacecolor="#cc5500", markersize=10),
    ]

    create_graph(x_coords, y_coords, colors, legend_elements, "crates.png")


def golden_statues():
    with open("golden_statues.json") as f:
        data = json.load(f)

    print("There are", len(data), "golden statues on the map")

    glitched_statues = [[-704, -2320.0002, 704], [704, 2320.0002, 704]]

    x_coords = []
    y_coords = []
    colors = []
    for entry in data:
        x_coords.append(entry["origin"][0])
        y_coords.append(entry["origin"][1])
        if entry["scales"][0] < 0.85:
            colors.append("blue")
        elif entry["initial_spawn_time_override"] > 0:
            colors.append("green")
        elif entry["origin"] in glitched_statues:
            colors.append("red")
        else:
            colors.append("orange")

    from matplotlib.lines import Line2D
    legend_elements = [
        Line2D([0], [0], marker="o", color="w", label="Vent access required",
               markerfacecolor="blue", markersize=10),
        Line2D([0], [0], marker="o", color="w", label="T2 Statue (10 minute spawn)",
               markerfacecolor="green", markersize=10),
        Line2D([0], [0], marker="o", color="w", label="Glitched Statue",
               markerfacecolor="red", markersize=10),
        Line2D([0], [0], marker="o", color="w", label="Normal Statue",
               markerfacecolor="orange", markersize=10),
    ]

    create_graph(x_coords, y_coords, colors, legend_elements, "golden_statues.png")


def create_graph(x_coords, y_coords, colors, legend_elements, filename):
    plt.figure(figsize=(20, 20))

    img = plt.imread("minimap_midtown_mid_opaque.png")

    scale = 10750.0
    plt.imshow(img, extent=(-scale, scale, -scale, scale))

    plt.scatter(x_coords, y_coords, s=20, c=colors, alpha=1)

    plt.legend(handles=legend_elements, loc="upper left", fontsize="xx-large", framealpha=1)

    plt.axis("off")

    plt.xlim(-10000, 10000)
    plt.ylim(-10000, 10000)

    plt.savefig(filename, bbox_inches="tight", pad_inches=0)
    print(f"Graph saved as {filename}")


if __name__ == "__main__":
    golden_statues()
    crates()