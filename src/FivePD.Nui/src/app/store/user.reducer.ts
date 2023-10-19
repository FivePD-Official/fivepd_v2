import { createReducer, on } from '@ngrx/store';
import { InformationUpdateDTO, PlayerDetailsDTO } from '@models';
import { setHasShownWelcomeModal, setInformation, setUser } from '@store/user.actions';
import { FDutyStatus, FUnitAvailability } from "@enums";

export interface UserStateModel {
  details: PlayerDetailsDTO;
  hasShownWelcomeModal: boolean;
  information: InformationUpdateDTO;
}

const initialState: UserStateModel = {
  details: {} as PlayerDetailsDTO,
  hasShownWelcomeModal: false,
  information: {
    Event: '',
    Status: '-',
    DutyStatus: FDutyStatus.OffDuty,
    Availability: FUnitAvailability.NotReceiving,
  },
};

export const userReducer = createReducer(
  initialState,
  on(setUser, (state, action) => {
    return {...state, details: action.payload};
  }),
  on(setHasShownWelcomeModal, (state, action) => {
    return {...state, hasShownWelcomeModal: action.payload};
  }),
  on(setInformation, (state, action) => {
    return {...state, information: action.payload};
  }),
);
