import { AppStateModel } from './app.state';
import { UserStateModel } from './user.state';

export interface GlobalState {
  app: AppStateModel;
  user: UserStateModel;
}
