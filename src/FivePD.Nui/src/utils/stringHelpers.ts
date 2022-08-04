export const replaceParams = (text: string, params: Record<string, string>) => {
    for (const [key, value] of Object.entries(params)) {
        text = text.replace(`{{${key}}}`, `<b>${value}</b>`);
    }
    return text;
};

export const toSentence = (...params: string[]): string => {
    return params.join(' ');
};