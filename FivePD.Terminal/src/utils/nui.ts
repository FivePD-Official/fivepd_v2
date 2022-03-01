export const showNui = () => {
  document.body.style.display = "block";
}

export const hideNui = () => {
  document.body.style.display = "none";
  try {
    fetch('https://fivepd/fivepd:nui:close',{
      method: 'POST'
    });
  } catch(_) {}
}