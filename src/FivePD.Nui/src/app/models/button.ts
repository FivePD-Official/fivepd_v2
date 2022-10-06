import { Tooltip } from './tooltip';

export interface Button {
  label: string;
  theme?: 'primary' | 'secondary' | 'small' | 'danger';
  type?: 'button' | 'submit';
  show: () => boolean;
  disabled: () => boolean;
  tooltip?: Tooltip;
  link?: string[];
  onClick?: () => any;
}
