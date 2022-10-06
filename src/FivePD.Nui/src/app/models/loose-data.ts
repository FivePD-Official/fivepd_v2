export interface Set {
  Id: string;
  Attributes: SetAttribute[];
}

export interface SetAttribute {
  Id: number;
  Name: string;
  Order?: number;
  DataType: number;
  Required: boolean;
  Readonly?: boolean;
  ShowInGrid?: boolean;
  Label?: string;
  HelpLabel?: string;
  Placeholder?: string;
  Options?: SetAttributeOptions;
  AddonId: string;
}

export enum AttributeDataType {
  ShortString,
  LongString,
  Integer,
  Bool,
  Date,
}

export interface SetAttributeOptions {
  Items: Record<number, string>;
}

export interface AddSetAttribute extends Omit<SetAttribute, 'Id' | 'AddonId'> {}
export interface EditSetAttribute extends Omit<SetAttribute, 'AddonId' | 'DataType'> {}
