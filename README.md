## Development

1. Navigate to the `/src/FivePD.Nui/` project/folder and execute `npm install` if the commands fails, do `npm install --force` instead
1. [Download](https://runtime.fivem.net/artifacts/fivem/build_server_windows/master/) and extract the latest recommended build of FXServer into the `/dev/FXServer/` directory
1. Change the build configuration to `Debug` via Main Menu > Build > Change Solution Configuration
1. Setup a database of your choosing, either by installing MySQL Community Server or MariaDB to your local PC, or by using one of the Docker configurations metioned in the "Databases" section
1. Start the `Run FXServer` run configuration
1. Run the `Start NUI development server` configuration and make sure the value of `ui_page` is `http://localhost:4200`
   in your `fxmanifest.lua`

Building the solution will automatically copy all files recursively from the `FivePD.Statics` folder to the resource
folder and restart the resource.

### Databases

If you choose to run a database locally, you will need to download [MySQL Community Server](https://dev.mysql.com/downloads/mysql/) or [MariaDB](https://mariadb.org/download/?t=mariadb&p=mariadb&r=11.3.2&os=windows&cpu=x86_64&pkg=msi&mirror=heanet-ltd). Be sure to configure your database options correctly in the `general.yaml` file.

Alternatively, you can -- and should -- use one of the included Docker configurations. In order to do so, download [Docker Desktop](https://www.docker.com/products/docker-desktop/) and choose either "Run MySQL" or "Run MariaDB" in the run configuration dropdown.

- Host: localhost
- Username: root
- Password root
- Port: 3306 (MySQL) 3307 (MariaDB)


### Running Migrations

To create a migration, execute the following command from the `/src/` folder:

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