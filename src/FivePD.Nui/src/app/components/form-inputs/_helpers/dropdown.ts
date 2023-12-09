import { DropdownHeightVariants, DropdownVariants } from "@models";

export const dropdownSelectedItemVariants: Record<DropdownVariants, string> = {
  'secondary': `border border-zinc-600 bg-transparent
                before:absolute before:inset-0 before:w-full before:h-full before:rounded-md
                before:bg-transparent`,
  'tertiary': `border border-zinc-800
               before:absolute before:inset-0 before:w-full before:h-full before:rounded-md
               before:bg-zinc-800`,
  'tertiary-bg': `border border-zinc-700
                  before:absolute before:inset-0 before:w-full before:h-full before:rounded-md
                  before:bg-zinc-700`,
};

export const dropdownSelectedItemVariantsHover: Record<DropdownVariants, string> = {
  'secondary': `hover:border-zinc-400`,
  'tertiary': `hover:before:bg-zinc-700`,
  'tertiary-bg': `hover:before:bg-zinc-600`,
};

export const dropdownSelectedItemVariantsHoverReadonly: Record<DropdownVariants, string> = {
  'secondary': `hover:border-zinc-600`,
  'tertiary': `hover:before:border-zinc-800`,
  'tertiary-bg': `hover:before:border-zinc-700`,
};

export const dropdownHeightVariants: Record<DropdownHeightVariants, string> = {
  'base': 'h-[40px]',
  'sm': 'h-[35px]',
};
