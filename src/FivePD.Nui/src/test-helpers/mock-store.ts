import { User } from '@models';
import FunctionPropertyNames = jest.FunctionPropertyNames;
import { UserStateModel } from '@store/user.reducer';
import { AppStateModel } from "@store/app.reducer";
import { of } from "rxjs";

type DeepPartial<T> = {
  [P in keyof T]?: T[P] extends object ? DeepPartial<T[P]> : T[P];
};

interface GlobalStateModel {
  [key: string]: any;

  app: AppStateModel,
  user: UserStateModel,
}

type PartialState = DeepPartial<GlobalStateModel>;

const defaultState: GlobalStateModel = {
  app: {
    previousMdtPath: ['mdt', 'dashboard'],
    dispatchMode: false,
    dispatchModeMapZoom: 0,
    recentlyViewedPeds: [],
    windowMaximized: false,
    devTools: {
      coordinates: {
        CurrentPage: 0,
        ItemCount: 0,
        PageCount: 0,
        PageSize: 0,
        Items: [],
      },
    },
  },
  user: {
    details: {} as any,
  }
};

export class MockStore {

  private subscriptions: any = [];
  private _state: GlobalStateModel = {...defaultState};

  get state(): GlobalStateModel {
    return this._state;
  }

  subscribe(callback: (state: GlobalStateModel) => void): void {
    this.subscriptions.push(callback);
  }

  reset(state?: GlobalStateModel): void {
    this._state = state ?? {...defaultState};
    this.invokeSubscriptions();
  }

  dispatch(partialState: PartialState): void {
    this.updateState('app', partialState);
    this.updateState('user', partialState);
    this.invokeSubscriptions();
  }

  select(...params: any[]): any {
    return of();
  }

  pipe(...params: any[]): any {
    return of();
  }

  private updateState(key: string, partialState: PartialState): void {
    if (!partialState) {
      return;
    }

    let updatedState = {...this._state[key]};
    if (partialState[key]) {
      updatedState = {...this._state[key], ...partialState[key]};
    }
    this._state[key] = updatedState;
  }

  private invokeSubscriptions(): void {
    for (const item of this.subscriptions) {
      item(this._state);
    }
  }

}

export function mockStoreDispatchDecorators<T extends Object>(instance: T, methods: { [K in FunctionPropertyNames<T>]?: (...args: any) => any }): void {
  for (const [name, method] of Object.entries(methods)) {
    jest.spyOn(instance, name as any).mockImplementation(method as any);
  }
}
