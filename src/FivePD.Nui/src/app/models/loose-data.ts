export interface Set {
  Id: string;
  Attributes: SetAttribute[];
}

export interface SetAttribute {
  Id: number;
  Name: string;
  DataType: number;
  Required: boolean;
  Label?: string;
  HelpLabel?: string;
  Placeholder?: string;
  Options?: SetAttributeOptions;
}

export interface SetAttributeOptions {
  Items: Record<number, string>;
}
