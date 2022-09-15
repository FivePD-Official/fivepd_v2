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
  AddonId: string;
}

export enum AttributeDataType {
  String,
  Integer,
  Bool,
  DateTime,
}

export interface SetAttributeOptions {
  Items: Record<number, string>;
}

export interface AddSetAttribute extends Omit<SetAttribute, 'Id' | 'AddonId'> {}
export interface EditSetAttribute extends Omit<SetAttribute, 'AddonId' | 'DataType'> {}
