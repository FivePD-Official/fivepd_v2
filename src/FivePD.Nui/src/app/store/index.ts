import { UserStateModel } from '@store/user.reducer';
import { AppStateModel } from '@store/app.reducer';

export interface GlobalState {
  app: AppStateModel;
  user: UserStateModel;
}
