import { createActionGroup, props } from '@ngrx/store';
import { PlayerDetailsDTO } from '@models';

export const UserActions = createActionGroup({
  source: 'user',
  events: {
    'Set user': props<{ user: PlayerDetailsDTO }>(),
    'Set has shown welcome modal': props<{ hasShownWelcomeModal: boolean }>(),
  },
});
