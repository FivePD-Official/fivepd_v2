export interface IMenuItem {
  Title: string,
  Hashcode: string
}

export interface IMenu {
  Title: string,
  Hashcode: string,
  Items: IMenuItem[]
}