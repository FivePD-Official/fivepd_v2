# GPMDS' FivePD

*All code and assets contained in this repository are STRICTLY the intellectual property of GTAPoliceMods unless otherwise noted by a license provided. Do not copy or redistribute.*

### Prerequisites

- Before making changes to FivePD.Terminal, run `npm install` in the project's root directory
- Make sure you have created a `.env.production` file in FivePD.Terminal and it contains the following value(s):

  `VITE_OUTDIR`=<your_path_to_the_fivem_server>\\resources\\fivepd\\Libraries\\FivePD.Terminal

### Developing

1. Insert the latest recommended build of FXServer into the `FXServer` directory
2. Select the Debug solution configuration
3. Start the "Run FXServer" run configuration
4. If changes are made to the FivePD.Terminal project, run `npm run build` inside it (will output to `VITE_OUTDIR`). Restart the *fivepd* resource manually

### Building for Production

1. Select the Release solution configuration
2. Build the solution (will output to `build-public`)
3. Run `npm run build` in FivePD.Terminal (will output to `VITE_OUTDIR`).
4. Obfuscate and do your magic, Daniel