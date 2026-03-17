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
using System.Text.Json;
using Microsoft.Extensions.Logging;
using SteamKit2;

namespace DeadlockEntityHelper.Commands;

public class VersionCommand : Command
{
    private const uint DeadlockAppId = 1422450;
    public VersionCommand(Option<bool> verboseOption) : base("version", "Get the latest public build ID and timestamps for Deadlock")
    {
        this.SetHandler(async (isVerbose) =>
        {
            using var loggerFactory = LoggerFactory.Create(builder =>
            {
                builder.AddConsole();
                builder.SetMinimumLevel(isVerbose ? LogLevel.Debug : LogLevel.Information);
            });
            var logger = loggerFactory.CreateLogger<VersionCommand>();
            await RunHelperAsync(logger);
        }, verboseOption);
    }

    private async Task RunHelperAsync(ILogger<VersionCommand> logger)
    {
        var configuration = SteamConfiguration.Create(b => b.WithProtocolTypes(ProtocolTypes.Tcp));
        var steamClient = new SteamClient(configuration);
        var steamApps = steamClient.GetHandler<SteamApps>();
        if (steamApps == null)
        {
            logger.LogError("Failed to get SteamApps handler");
            return;
        }
        
        var steamUser = steamClient.GetHandler<SteamUser>();
        if (steamUser == null)
        {
            logger.LogError("Failed to get SteamUser handler");
            return;
        }
        
        var manager = new CallbackManager(steamClient);
        
        manager.Subscribe<SteamClient.ConnectedCallback>((_) => OnConnected(logger, steamUser));
        manager.Subscribe<SteamClient.DisconnectedCallback>((_) => OnDisconnected(logger));
        manager.Subscribe<SteamUser.LoggedOnCallback>((_) => onLoggedOn(logger, steamApps));
        manager.Subscribe<SteamApps.PICSTokensCallback>((cb) => OnPICSTokens(cb, logger, steamApps));
        manager.Subscribe<SteamApps.PICSProductInfoCallback>((cb) => OnPICSProductInfo(cb, logger));
        
        IsRunning = true;
        
        steamClient.Connect();

        while (IsRunning)
        {
            // in order for the callbacks to get routed, they need to be handled by the manager
            manager.RunWaitCallbacks(TimeSpan.FromSeconds(1));
        }

        steamClient.Disconnect();
        
        await Task.CompletedTask;
    }
    
    private void OnConnected(ILogger<VersionCommand> logger, SteamUser steamUser)
    {
        logger.LogDebug("Connected to Steam");
        // steamUser.LogOnAnonymous();
        steamUser.LogOn( new SteamUser.LogOnDetails
        {
            Username = Environment.GetEnvironmentVariable("STEAM_BOT_USERNAME"),
            Password = Environment.GetEnvironmentVariable("STEAM_BOT_PASSWORD"),
        } );
    }

    private void OnDisconnected(ILogger<VersionCommand> logger)
    {
        logger.LogDebug("Disconnected from Steam");
    }

    private void onLoggedOn(ILogger<VersionCommand> logger, SteamApps steamApps)
    {
        logger.LogDebug("Logged in to Steam");
        steamApps.PICSGetAccessTokens([DeadlockAppId], []);
    }

    private void OnPICSTokens(SteamApps.PICSTokensCallback callback, ILogger<VersionCommand> logger,
        SteamApps steamApps)
    {
        logger.LogDebug("Received PICS token");
        var request = new SteamApps.PICSRequest(DeadlockAppId)
        {
            AccessToken = callback.AppTokens[DeadlockAppId]
        };
        steamApps.PICSGetProductInfo(request, null);
    }

    private void OnPICSProductInfo(SteamApps.PICSProductInfoCallback callback, ILogger<VersionCommand> logger)
    {
        logger.LogDebug("Received product info");
        IsRunning = false;

        ParseAppInfo(callback.Apps[DeadlockAppId].KeyValues);
    }
    
    private void ParseAppInfo(KeyValue appInfo)
    {
        var latestInfo = appInfo["depots"]["branches"]["public"];
        
        var jsonOptions = new JsonSerializerOptions { WriteIndented = true };
        var jsonString = JsonSerializer.Serialize(
        new {
            buildid = latestInfo["buildid"].Value,
            timeupdated = latestInfo["timeupdated"].Value,
            timebuildupdated = latestInfo["timebuildupdated"].Value,
        }, jsonOptions);
        Console.WriteLine(jsonString);
    }

    private static bool IsRunning { get; set; }
}