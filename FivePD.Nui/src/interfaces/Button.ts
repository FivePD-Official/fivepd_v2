import { ActionResponse } from "@interfaces";

export interface PropButton {
  label: string,
  onClick?: (onResponse: (response: ActionResponse) => void) => void,
  link?: string,
  show?: boolean,
}