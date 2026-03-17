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
using System.CommandLine.Builder;
using System.CommandLine.Parsing;
using DeadlockEntityHelper.Commands;

var verboseOption = new Option<bool>(
    aliases: ["--verbose", "-v"],
    description: "Enable verbose logging.",
    getDefaultValue: () => false);

var rootCommand = new RootCommand("Deadlock Entity Helper");
rootCommand.AddGlobalOption(verboseOption);

var extractCommand = new ExtractCommand(verboseOption);
rootCommand.AddCommand(extractCommand);

var versionCommand = new VersionCommand(verboseOption);
rootCommand.AddCommand(versionCommand);

var builder = new CommandLineBuilder(rootCommand);
builder.UseDefaults();

var parser = builder.Build();
return await parser.InvokeAsync(args);