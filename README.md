# GPMDS' FivePD

*All code and assets contained in this repository are STRICTLY the intellectual property of GTAPoliceMods unless otherwise noted by a license provided. Do not copy or redistribute.*

### Prerequisites

- Run `npm install` in the `FivePD.Nui` directory
- Make sure you have created a `.env.production` file in FivePD.Nui and it contains the following value(s):

  `VITE_OUTDIR`=<your_path_to_the_fivem_server>\\resources\\fivepd\\Libraries\\FivePD.Nui

### Running Migrations

- Use the syntax `dotnet ef migrations add <MigrationName> --project .\FivePD.Gamemode.Server\FivePD.Gamemode.Server.csproj --startup-project .\FivePD.StartupProject\FivePD.StartupProject.csproj` from the root folder. Migrations require a startup project to run properly.
### Developing

1. Insert the latest recommended build of FXServer into the `FXServer` directory
1. Select the Debug solution configuration
1. Start the "Run FXServer" run configuration
1. If you want to make changes to FivePD.Nui, change the `ui_page` value to `http://localhost:3000` in your `fxmanifest.lua`. You might need to restart the resource sometimes, because the old UI and events could be cached.
1. To generate the localization and config files run the `Generate JSON files` configuration. The files will be written to the `FivePD.Gamemode.Statics` directory.

### Building for Production

1. Select the Release solution configuration
1. Build the solution (will output to `build-public`)
1. Run `FivePD.JsonGenerator.exe` to generate the required files (will output to `FivePD.Gamemode.Statics/*`)
1. Run `npm run build` in FivePD.Nui (will output to `VITE_OUTDIR`).
1. In the `fxmanifest.lua` change the `ui_page` value to `Libraries/FivePD.Nui/index.html`
1. Obfuscate and do your magic, Daniel