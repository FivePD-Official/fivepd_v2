export interface IGeneralConfig {
  Unit: {
    Join: string;
    MaxMembers: number;
  };
  FeatureFlags: {
    DiscordRichPresence: boolean;
    DutyNotification: boolean;
    DisableWantedLevel: boolean;
    DisableWantedLevelAllTime: boolean;
    Backups: boolean;
    ProgressionSystem: boolean;
  };
}

export class GeneralConfig {

  constructor(private readonly _config: IGeneralConfig) {
  }

  get config() {
    return this._config;
  }

  joinFreelyToUnit(): boolean {
    return this._config.Unit.Join.toUpperCase() === 'FREE';
  }

  joinByRequestToUnit(): boolean {
    return this._config.Unit.Join.toUpperCase() === 'REQUEST';
  }

}
