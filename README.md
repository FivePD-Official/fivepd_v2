## Developing
1. Insert the latest recommended build of FXServer into the `FXServer` directory
1. Build and run the `FivePD.Localization` project with the `-s` flag
1. Select the Debug solution configuration
1. Start the **"Run FXServer"** run configuration
1. Run the **"Start NUI development server"** configuration and make sure to change the `ui_page` value to `http://localhost:3000` in your `fxmanifest.lua`

## Running Migrations
Use the command below in the root folder. Migrations require a startup project to run properly.

```
dotnet ef migrations add <MigrationName> --project .\FivePD.Gamemode.Server\FivePD.Gamemode.Server.csproj --startup-project .\FivePD.StartupProject\FivePD.StartupProject.csproj
```

## Localization
Make sure that all keys end with one of the following words, so we can extend them in the future if needed. There are some sentences where this doesn't make sense, for example *UI.LABEL.CITIZEN_DATABASE* or *UI.LABEL.DIRECTION.NEXT*. In these cases we don't want to have more options other than the basic form, so we can omit the endings listed below.
```
SINGULAR, PLURAL, TITLE, DESCRIPTION, HELP_LABEL
```

## Production
1. Select the Release solution configuration
1. Build the solution. Will output to `build-public`
1. Build and run `FivePD.Localization` project with the `-f -o` flags to generate the optimized localization file. Will output to `FivePD.Localization/Localization/Optimized/localization.json`
1. Run `npm run build` in the `FivePD.Nui` project. Will output to `FivePD.Nui/dist`
1. In the `fxmanifest.lua` change the `ui_page` value to `Libraries/FivePD.Nui/index.html`
1. Obfuscate!
1. TBD