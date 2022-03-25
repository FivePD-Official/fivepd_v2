export const Post = (path: string, body?: object | number) => {
  return new Promise (async (resolve, reject) => {

    let newBody: string | number = '';
    if (typeof body === 'object') {
      newBody = JSON.stringify(body);
    } else {
      newBody = body as number;
    }

    const res = await fetch(`https://fivepd/${path}`, {
      method: 'POST',
      body: newBody as BodyInit,
      headers: {
        'Content-Type': typeof body === 'object' ? 'application/json' : 'text/plain'
      }
    });

    if (!res) return reject();
    const json = await res.json();
    if (!json) return reject();
    return resolve(json);
  });
}