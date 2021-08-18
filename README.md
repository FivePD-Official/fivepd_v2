# GPMDS' FivePD

*All code and assets contained in this repository are STRICTLY the intellectual property of GTAPoliceMods unless otherwise noted by a license provided. Do not copy or redistribute.*

### Developing

1. Insert the latest recommended build of FXServer into the `FXServer` directory
2. Select the Debug solution configuration
3. Start the "Run FXServer" run configuration
4. Start the "Publish FivePD.Terminal to dev FXServer" run configuration
5. After changes are made, build the solution WHILE the run configuration is still running, this will hot reload all changes
6. If changes are made to the FivePD.Terminal project, re-run the "Publish FivePD.Terminal to dev FXServer" run configuration manually

### Building for Production

1. Select the Release solution configuration
2. Build the solution (will output to `build-public`)
3. Start the "Publish FivePD.Terminal to build-public" run configuration
4. Obfuscate and do your magic, Daniel