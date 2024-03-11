import { createReducer, on } from '@ngrx/store';
import { InformationUpdateDTO, PlayerDetailsDTO } from '@models';
import { UserActions } from '@store/user.actions';
import { FDutyStatus, FUnitAvailability } from "@enums";

export interface UserStateModel {
  details: PlayerDetailsDTO;
  information: InformationUpdateDTO;
}

const initialState: UserStateModel = {
  details: {} as PlayerDetailsDTO,
  information: {
    PanicMode: false,
    Event: '',
    Status: '-',
    DutyStatus: FDutyStatus.OffDuty,
    Availability: FUnitAvailability.NotReceiving,
  },
};

export const userReducer = createReducer(
  initialState,
  on(UserActions.setUser, (state, action) => ({...state, details: action.user})),
  on(UserActions.setInformation, (state, {information}) => ({...state, information})),
);
