import { ActionResponse } from '@interfaces';

interface Button {
  label: string;
  link?: string;
  show: () => boolean;
}

export interface PropButton extends Button {
  onClick: () => void;
}

export interface PropButtonWithResponse extends Button {
  onClick: (onResponse: (response: ActionResponse) => void) => void;
}
