## Development

1. Install the Nuget packages and install the Node packages in the `FivePD.Nui` project
1. Insert the latest recommended build of FXServer into the `FXServer` directory
1. Select the Debug solution configuration
1. Start the `Run FXServer` run configuration
1. Run the `Start NUI development server` configuration and make sure the value of `ui_page` is `http://localhost:4200`
   in your `fxmanifest.lua`

Building the solution will automatically copy all files recursively from the `FivePD.Statics` folder to the resource
folder and restart the resource.

### Databases

You can start a MySQL or a MariaDB server with the corresponding docker configurations.
Make sure to change the connections settings.

### Running Migrations

Use the command below in the root folder. Migrations require a startup project to run properly.

```
dotnet ef migrations add <MigrationName> --project .\FivePD.Server\FivePD.Server.csproj --startup-project .\FivePD.StartupProject\FivePD.StartupProject.csproj
```

## Localization

Make sure that all keys end with one of the following words, so we can extend them in the future if needed. There are
some sentences where this doesn't make sense, for example *UI.LABEL.CITIZEN_DATABASE* or *UI.LABEL.DIRECTION.NEXT*. In
these cases we don't want to have more options other than the basic form, so we can omit the endings listed below.

```
SINGULAR, PLURAL, TITLE, DESCRIPTION, HELP_LABEL
```

### Updating

1. Edit the `FivePD.Localization/Localization/en-US.json` file
1. Run the `Build localization` configuration

It'll output the files to `FivePD.Statics/Localizations`.  
In case you need the new localization file to be loaded on the server just build the solution again.

## Production

1. Select the Release solution configuration
1. Build the solution. Will output to `build-public`
1. Run the `Build localization` configuration to generate the optimized localization file. Will output
   to `FivePD.Statics/Localizations`.
1. Run `npm run build` in the `FivePD.Nui` project. Will output to `FivePD.Nui/dist`
1. In the `fxmanifest.lua` change the `ui_page` value to `Libraries/FivePD.Nui/index.html`
1. Obfuscate!