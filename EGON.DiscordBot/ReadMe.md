﻿E.G.O.N. is a guild bot for the guild Echelon on Icecrown.

# Using the bot
The bot interacts with Discord through Slash Commands.

Slash commands take parameters, then usually respond with a series of dropdowns to gather more information.

Available commands are:

````
/raid NAME DESCRIPTION IMAGE
/mythic NAME DESCRIPTION IMAGE
/event NAME DESCRIPTION IMAGE
````

/event just adds an extra step where it asks if you want a raid, dungeon, or meeting.

# Running the bot
E.G.O.N. runs in a docker container and requires an Azure Storage Account for data persistence.

You will need several environment variables configured on the container.

AZURE_PRIVATE_STORAGE_CONNECTION_STRING should contain a connection string to an Azure Storage Account that can run Tables.

AZURE_PUBLIC_STORAGE_CONNECTION_STRING should contain a connection string to an account with a public container in Azure blob storage.

DISCORD_TOKEN should have your bot token from the discord developer portal.

DISCORD_SERVER_ID should have the ID of the Discord server you want the bot to run on.

You also need a Battle.NET api client id and secret to use that module.

BATTLE_NET_CLIENT_ID

BATTLE_NET_CLIENT_SECRET

If running locally, add them to a launchSettings.json file in the /Properties folder. It should look like this:
```
{
  "profiles": {
    "EGON": {
      "commandName": "Project"
    },
    "Container (Dockerfile)": {
      "commandName": "Docker",
      "environmentVariables": {
        "AZURE_PRIVATE_STORAGE_CONNECTION_STRING": "",
        "AZURE_PUBLIC_STORAGE_CONNECTION_STRING": "",
        "DISCORD_TOKEN": "",
        "DISCORD_SERVER_ID": ""
        "BATTLE_NET_CLIENT_ID": ""
        "BATTLE_NET_CLIENT_SECRET": ""
      }
    }
  }
}
```
DO NOT check this launchSettings.json into source control. It's in the gitignore but I thought it was worth mentioning.

Make sure those environment variables are filled out appropriately or the bot cannot run.

There is a provided PowerShell script for populating the StoredEmojis table appropriately. You will need to run it against your Azure storage account
for signups to work properly.