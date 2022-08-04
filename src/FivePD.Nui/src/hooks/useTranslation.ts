import { store } from '@store';

export function useTranslation() {
  return (key: string) => {
    const localization: any = store.state.global.localization;

    let currentItem = localization;
    for (const k of key.split('.')) {
      if (!currentItem.hasOwnProperty(k)) {
        return '';
      }

      currentItem = currentItem[k];
    }
    return currentItem;
  };
}