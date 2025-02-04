export function replaceParams(
  text: string,
  params: Record<string, string> = {},
  boldParams: boolean = true
): string {
  if (typeof text !== 'string') {
    return '';
  }

  params ??= {};

  // Add default parameters if necessary
  params['NEW_LINE'] = '<br/>';

  // Use regex to match placeholders {{KEY}} and {{KEY(x)}}, case-insensitive
  const regex = /{{([\w]+)(?:\((.*?)\))?}}/gi;

  try {
    text = text.replace(regex, (match, key, variable) => {
      if (params[key] !== undefined) {
        const replacement = params[key];
        if (variable !== undefined && params[`${key}(${variable})`] !== undefined) {
          const variableValue = params[`${key}(${variable})`];
          const fullReplacement = `${replacement} ${variableValue}`;
          return boldParams ? `<b>${fullReplacement}</b>` : fullReplacement;
        }

        return boldParams ? `<b>${replacement}</b>` : replacement;
      }

      return key;
    });
  } catch (_) {
  }

  return text;
}
