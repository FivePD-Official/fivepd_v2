export interface IGeneralConfig {
  Unit: {
    Join: string;
    MaxMembers: number;
  };
  Callout: {
    DispatchType: 'auto' | 'manual';
    MinIntervalInMs: number;
    MaxIntervalInMs: number;
    MaxActiveCallouts: number;
    DispatchTimeoutInMs: number;
    DynamicDispatch: {
      Enabled: boolean;
      Radius: number;
    };
  }
  Features: {
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

  dispatchTypeAuto(): boolean {
    return this._config.Callout.DispatchType.toUpperCase() === 'AUTO';
  }

  dispatchTypeManual(): boolean {
    return this._config.Callout.DispatchType.toUpperCase() === 'MANUAL';
  }
}
