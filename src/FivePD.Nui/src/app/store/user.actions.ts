import { createActionGroup, props } from '@ngrx/store';
import { InformationUpdateDTO, PlayerDetailsDTO } from '@models';

export const UserActions = createActionGroup({
  source: 'user',
  events: {
    'Set user': props<{ user: PlayerDetailsDTO }>(),
    'Set has shown welcome modal': props<{ hasShownWelcomeModal: boolean }>(),
    'Set information': props<{ information: InformationUpdateDTO }>(),
  },
});
