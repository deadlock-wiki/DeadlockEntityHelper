/*
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
*/
using System.CommandLine;
using System.Diagnostics;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using SteamDatabase.ValvePak;
using ValveResourceFormat;
using ValveResourceFormat.ResourceTypes;

var rootCommand = new RootCommand("Deadlock Entity Helper");

// Arguments
var vpkFileArgument = new Argument<FileInfo>(
    name: "vpk-file",
    description: "The path to the midtown VPK file")
    .ExistingOnly(); // Ensures the file exists
rootCommand.AddArgument(vpkFileArgument);

var entitySubClassArgument = new Argument<string>(
    name: "subclass",
    description: "The entity subclass to filter for");
rootCommand.AddArgument(entitySubClassArgument);

// Options
var verboseOption = new Option<bool>(
    "--verbose",
    "Enable verbose logging.");
verboseOption.AddAlias("-v");
rootCommand.AddOption(verboseOption);

rootCommand.SetHandler(async (vpkFile, entitySubClass, verbose) =>
{
    await RunHelperAsync(vpkFile, entitySubClass, verbose);
}, vpkFileArgument, entitySubClassArgument, verboseOption);

return await rootCommand.InvokeAsync(args);

static async Task RunHelperAsync(FileInfo vpkFile, string entitySubClass, bool verbose)
{
    using var loggerFactory = LoggerFactory.Create(builder =>
    {
        builder.AddConsole();
        builder.SetMinimumLevel(verbose ? LogLevel.Debug : LogLevel.Information);
    });
    var logger = loggerFactory.CreateLogger("DeadlockEntityHelper");

    logger.LogDebug("Loading VPK: {VpkFile}", vpkFile.FullName);
    using var package = new Package();
    package.Read(vpkFile.FullName);
    logger.LogDebug("Successfully loaded VPK");

    string ventsPath = "maps/dl_midtown/entities/default_ents.vents_c";
    PackageEntry? ventsEntry = package.FindEntry(ventsPath);
    if (ventsEntry == null)
    {
        logger.LogError("Failed to find {VentsPath} in the VPK", ventsPath);
        return;
    }
    
    package.ReadEntry(ventsEntry, out var rawFile);
    using var ms = new MemoryStream(rawFile);
    using var ventsResource = new Resource();
    ventsResource.FileName = ventsEntry.GetFullPath();
    ventsResource.Read(ms);
    
    Debug.Assert(ventsResource.ResourceType == ResourceType.EntityLump);

    if (ventsResource.DataBlock == null)
    {
        logger.LogError("Unable to read entity resource data block");
        return;
    }
    var ventsLump = (EntityLump)ventsResource.DataBlock;

    var entities = ventsLump.GetEntities();
    var entCount = entities.Count;
    logger.LogDebug("Parsing {EntCount} entities...", entCount);

    var filteredEntities = entities
        .Where(entity => entity.GetProperty<string>("subclass_name") == entitySubClass)
        .Select(entity =>
        {
            var subclassName = entity.GetProperty<string>("subclass_name");
            var spawnTimeOverride = entity.GetProperty<double>("initial_spawn_time_override");
            var scales = entity.GetVector3Property("scales");
            var origin = entity.GetVector3Property("origin");
            return new
            {
                subclass_name = subclassName,
                initial_spawn_time_override = spawnTimeOverride,
                scales = new[] { scales.X, scales.Y, scales.Z },
                origin = new[] { origin.X, origin.Y, origin.Z }
            };
        })
        .ToList();

    var jsonOptions = new JsonSerializerOptions { WriteIndented = true };
    var jsonString = JsonSerializer.Serialize(filteredEntities, jsonOptions);
    Console.WriteLine(jsonString);

    await Task.CompletedTask;
}