export const Post = (path: string, body?: object) => {
  return new Promise (async (resolve, reject) => {
    const res = await fetch(`https://fivepd/${path}`, {
      method: 'POST',
      body: JSON.stringify(body)
    });
    if (!res) reject();
    const json = await res.json();
    if (!json) reject();
    resolve(json);
  });
}