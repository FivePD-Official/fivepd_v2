export type TimeStyle = 'full' | 'short' | 'medium' | 'long';

export function formatDate(date: Date, timeStyle: TimeStyle = 'short'): string {
  return new Intl.DateTimeFormat('en-US', {dateStyle: 'medium', timeStyle}).format(date);
}
