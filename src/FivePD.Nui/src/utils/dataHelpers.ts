import { post } from '@utils';
import { IUser } from '@interfaces';
import { ClientEvents } from '@enums';
import { store } from '@store';

export function requestAndCommitUserData() {
  post<IUser>(ClientEvents.GetPlayerDetails).subscribe((res) => {
    store.commit('setUserDetails', res);
  });
}