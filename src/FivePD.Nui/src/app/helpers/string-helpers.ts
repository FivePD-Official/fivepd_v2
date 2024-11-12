export function replaceParams(text: string, params: Record<string, string> = {}, boldParams: boolean = true): string {
  params ??= {};

  params['NEW_LINE'] = '<br/>';

  for (const [key, value] of Object.entries(params)) {
    text = text.replace(`{{${key}}}`, boldParams ? `<b>${value}</b>` : value);
  }
  return text;
}
