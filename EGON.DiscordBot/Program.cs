﻿using Azure.Data.Tables;
using Azure.Storage.Blobs;
using Discord.Interactions;
using Discord.WebSocket;
using EGON.DiscordBot.Services;
using EGON.DiscordBot.Services.WoW;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureAppConfiguration(config =>
    {
        // Always load environment variables (for Azure)
        config.AddEnvironmentVariables();
    })
    .ConfigureServices((context, services) =>
    {
        // The order here matters, so don't go mucking with it.

        services.AddSingleton<DiscordSocketClient>();

        services.AddSingleton(provider =>
        {
            var discord = provider.GetRequiredService<DiscordSocketClient>();
            return new InteractionService(discord);
        });

        services.AddHostedService<InteractionHandlingService>();

        services.AddHostedService<DiscordStartupService>();

        services.AddSingleton(provider =>
        {
            return new TableServiceClient(Environment.GetEnvironmentVariable("AZURE_PRIVATE_STORAGE_CONNECTION_STRING"));
        });

        services.AddSingleton<StorageService>();

        services.AddSingleton(provider =>
        {
            return new BlobServiceClient(Environment.GetEnvironmentVariable("AZURE_PUBLIC_STORAGE_CONNECTION_STRING"));
        });

        services.AddSingleton<BlobUploadService>();

        services.AddSingleton<EmoteFinder>();

        services.AddSingleton<EmbedFactory>();

        services.AddSingleton<BattleNetAuthService>();

        services.AddSingleton<WoWApiService>();

        services.AddHttpClient();

        services.AddHostedService<ScheduledMessageService>();

        services.AddHostedService<ScheduledPostService>();
    })
    .Build();

await host.RunAsync();