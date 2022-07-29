import { PluginOption } from "vite";
import { readFile, watch, writeFile } from "fs";
import * as path from "path";

const localizationPath = path.join(__dirname, '../../FivePD.Gamemode.Statics/Localization/en.json');
const localizationInterfacePath = path.join(__dirname, '../src/interfaces/ILocalization.ts');
let previousData = '';

export default function localizationToInterfaceTransformer(): PluginOption {
    return {
        name: 'localization-to-interface',
        enforce: 'post',
        configureServer() {
            watch(localizationPath, async () => {
                try {
                    const data = await read();
                    if (previousData === data) {
                        return;
                    }

                    previousData = data;
                    const formattedInterface = data.replace(/(:.*".*)/gm, ": string;").replace(/"/gm, '');
                    const formattedObject = formattedInterface.replace(/string;/gm, "'',");
                    write(`// The content of this file is auto-generated, do not edit it manually.\n\nexport interface ILocalization {${formattedInterface.substring(1)}\n\nexport const defaultLocalization: ILocalization = ${formattedObject}`)
                } catch (err) {
                    console.log(err)
                }
            });
        }
    }
}

function read(): Promise<string> {
    return new Promise((resolve, reject) => {
        readFile(localizationPath, { encoding: 'utf-8' }, (err, data) => {
          if (err) {
              reject(err);
              return;
          }

          resolve(data);
        })
    })
}

function write(data: string) {
    writeFile(localizationInterfacePath, data, (err) => {
        if (err) {
            console.log(err)
        }
    });
}