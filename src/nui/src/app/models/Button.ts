export interface Button {
  label: string;
  theme?: 'primary' | 'secondary' | 'small' | 'danger';
  type?: 'button' | 'submit';
  show: () => boolean;
  disabled?: boolean;
  link?: string[];
  onClick?: () => any;
}
