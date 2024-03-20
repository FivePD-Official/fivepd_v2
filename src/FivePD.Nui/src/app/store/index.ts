import { UserStateModel } from '@store/user.reducer';
import { AppStateModel } from '@store/app.reducer';
import { SidebarStateModel } from "@store/sidebar.reducer";

export interface GlobalState {
  app: AppStateModel;
  user: UserStateModel;
  sidebar: SidebarStateModel;
}
