import { NuiEventType, Post } from "@utils";

export const showNui = () => {
  document.body.style.display = "block";
}

export const hideNui = () => {
  document.body.style.display = "none";

  Post(NuiEventType.Close).catch(() => {});
}