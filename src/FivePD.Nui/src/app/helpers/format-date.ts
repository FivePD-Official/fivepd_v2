export type TimeStyle = 'full' | 'short' | 'medium' | 'long';

export function formatDate(date: Date, timeStyle: TimeStyle | 'none' = 'short'): string {
  return new Intl.DateTimeFormat('en-US', {
    dateStyle: 'medium',
    timeStyle: timeStyle === 'none' ? undefined : timeStyle
  }).format(date);
}
