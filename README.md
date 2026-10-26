# FivePD v2.0

> Development has officially ceased.
> This repository is maintained as an archive. The code is provided "as-is" for the community to fork and maintain independently. No further updates, support, or bug fixes will be provided.

## Development

1. Navigate to the `/src/FivePD.Nui/` project/folder and execute `npm install` if the commands fails, do `npm install --force` instead
1. [Download](https://runtime.fivem.net/artifacts/fivem/build_server_windows/master/) and extract the latest recommended build of FXServer into the `/dev/FXServer/` directory
1. Change the build configuration to `Debug` via Main Menu > Build > Change Solution Configuration
1. Setup a database of your choosing, either by installing MySQL Community Server or MariaDB to your local PC, or by using one of the Docker configurations metioned in the "Databases" section
1. Make sure the value of `ui_page` is `http://localhost:4200` in your `fxmanifest.lua`
1. Run the `Start everything` configuration, or alternatively the `Start FXServer`, `Start NUI development server` and `Start static audio server` configurations

Building the solution will automatically copy all files recursively from the `FivePD.Statics` folder to the resource
folder and restart the resource.

### Databases

If you choose to run a database locally, you will need to download [MySQL Community Server](https://dev.mysql.com/downloads/mysql/) or [MariaDB](https://mariadb.org/download/?t=mariadb&p=mariadb&r=11.3.2&os=windows&cpu=x86_64&pkg=msi&mirror=heanet-ltd). Be sure to configure your database options correctly in the `general.yaml` file.

Alternatively, you can -- and should -- use one of the included Docker configurations. In order to do so, download [Docker Desktop](https://www.docker.com/products/docker-desktop/) and choose either "Run MySQL" or "Run MariaDB" in the run configuration dropdown.

- Host: localhost
- Username: root
- Password root
- Port: 3306 (MySQL) 3307 (MariaDB)

### Database Infrastructure

The project uses **Dapper** as the micro-ORM for database interactions. Custom SQL queries are constructed using the internal `SqlBuilder` (see `src/FivePD.Server/Database/Sql/SqlBuilder.cs`).

Database schema migrations are handled via a custom implementation. To manage table structures, we use `TableBuilder`. An example of the current schema definition can be found in `src/FivePD.Server/Database/Migrations/Initial20251228.cs`.

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
