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
using Microsoft.Extensions.Logging.Console;
using SteamDatabase.ValvePak;
using ValveResourceFormat;
using ValveResourceFormat.ResourceTypes;

namespace DeadlockEntityHelper.Commands;

public class ExtractCommand : Command
{
    private readonly Argument<FileInfo> _vpkFileArgument = new Argument<FileInfo>(
        name: "vpk-file",
        description: "The path to the midtown VPK file")
        .ExistingOnly();

    private readonly Argument<string> _entityKeyArgument = new Argument<string>(
        name: "entity-key",
        description: "The entity key to filter for");
    
    private readonly Argument<string> _entityValueArgument = new Argument<string>(
        name: "entity-value",
        description: "The value of the given entity key to filter for");

    private readonly Argument<string[]> _propertiesArgument = new Argument<string[]>(
        name: "properties",
        description: "List of property names and types (string, double, or vector3) to extract, e.g. \"my_prop string\"");


    public ExtractCommand(Option<bool> verboseOption) : base("extract", "Extract and filter entities from the midtown VPK file")
    {
        AddArgument(_vpkFileArgument);
        AddArgument(_entityKeyArgument);
        AddArgument(_entityValueArgument);
        AddArgument(_propertiesArgument);

        this.SetHandler(async (vpkFile, entityKey, entityValue, properties, isVerbose) =>
        {
            using var loggerFactory = LoggerFactory.Create(builder =>
            {
                builder.AddConsole(options => options.LogToStandardErrorThreshold = LogLevel.Trace);
                builder.SetMinimumLevel(isVerbose ? LogLevel.Debug : LogLevel.Information);
            });
            var logger = loggerFactory.CreateLogger<ExtractCommand>();
            await RunHelperAsync(vpkFile, entityKey, entityValue, properties, logger);
        }, _vpkFileArgument, _entityKeyArgument, _entityValueArgument, _propertiesArgument, verboseOption);
    }

    private static async Task RunHelperAsync(FileInfo vpkFile, string entityKey, string entityValue, string[] properties, ILogger<ExtractCommand> logger)
    {
        logger.LogDebug("Loading VPK: {VpkFile}", vpkFile.FullName);
        using var package = new Package();
        package.Read(vpkFile.FullName);
        logger.LogDebug("Successfully loaded VPK");

        const string ventsPath = "maps/dl_midtown/entities/default_ents.vents_c";
        var ventsEntry = package.FindEntry(ventsPath);
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

        object? ReadPropertyValue(EntityLump.Entity entity, string propertyName, string propertyType)
        {
            switch (propertyType)
            {
                case "string":
                    return entity.GetProperty<string>(propertyName);
                case "double":
                    return entity.GetProperty<double>(propertyName);
                case "vector3":
                    var vec3Prop = entity.GetVector3Property(propertyName);
                    return new[] { vec3Prop.X, vec3Prop.Y, vec3Prop.Z };
                default:
                    throw new Exception($"Invalid property type: {propertyType}");
            }
        }
        
        var filteredEntities = entities
            .Where(entity => entity.GetProperty<string>(entityKey) == entityValue)
            .Select(entity =>
            {
                var extractedProperties = new Dictionary<string, object?>();

                for (var index = 0; index < properties.Length; index += 2)
                {
                    var propertyName = properties[index];
                    var propertyType = properties[index + 1];

                    extractedProperties[propertyName] = ReadPropertyValue(entity, propertyName, propertyType);
                }

                return extractedProperties;
            })
            .ToList();

        var jsonOptions = new JsonSerializerOptions { WriteIndented = true };
        var jsonString = JsonSerializer.Serialize(filteredEntities, jsonOptions);
        Console.WriteLine(jsonString);

        await Task.CompletedTask;
    }
}
