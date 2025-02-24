This is the Blazor web portal for E.G.O.N. administration.

# Hosting the service

This web app runs in a docker container and requires an Azure Storage Account for data persistence.

You will need several environment variables configured on the container.

AZURE_PRIVATE_STORAGE_CONNECTION_STRING should contain a connection string to an Azure Storage Account that can run Tables.

AZURE_PUBLIC_STORAGE_CONNECTION_STRING should contain a connection string to an account with a public container in Azure blob storage.

DISCORD_CLIENT_ID should contain an OAuth client ID from the Discord developer portal.

DISCORD_CLIENT_SECRET should contain an OAuth client secret from the Discord developer portal.

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
        "DISCORD_CLIENT_ID": "",
        "DISCORD_CLIENT_SECRET": ""
      }
    }
  }
}
```
DO NOT check this launchSettings.json into source control. It's in the gitignore but I thought it was worth mentioning.

Make sure those environment variables are filled out appropriately or the service cannot run.