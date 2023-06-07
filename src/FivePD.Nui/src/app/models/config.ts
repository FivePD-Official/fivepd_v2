export interface IGeneralSettings {
  Unit: {
    Join: string;
  };
}

export class GeneralSettings {

  constructor(private readonly _settings: IGeneralSettings) {
  }

  joinFreelyToUnit(): boolean {
    return this._settings.Unit.Join.toUpperCase() === 'FREE';
  }

  joinByRequestToUnit(): boolean {
    return this._settings.Unit.Join.toUpperCase() === 'REQUEST';
  }

}
