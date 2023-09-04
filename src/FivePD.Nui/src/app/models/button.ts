import { Tooltip } from './tooltip';

export type ButtonVariants = 'primary' | 'secondary' | 'tertiary' | 'tertiary-bg' | 'danger';
export type ButtonSizes = 'base' | 'sm';

export interface Button {
  label: string;
  variant?: ButtonVariants;
  size?: ButtonSizes;
  type?: 'button' | 'submit';
  show: () => boolean;
  disabled: () => boolean;
  tooltip?: Tooltip;
  link?: string[];
  onClick?: () => any;
}
