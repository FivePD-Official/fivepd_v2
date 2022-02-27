export const showNui = () => {
  document.body.style.display = "block";
}

export const hideNui = () => {
  document.body.style.display = "none";
  fetch(`https://fivepd/closeMdt`,{
    method: 'POST'
  });
}