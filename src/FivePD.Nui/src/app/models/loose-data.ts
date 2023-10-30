export interface Set {
  Id: string;
  Attributes: LooseDataAttribute[];
}

export interface LooseDataAttribute {
  Id: number;
  Name: string;
  Order?: number;
  DataType: AttributeDataType;
  Required: boolean;
  Hidden?: boolean;
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
  IntegerList,
}

export interface SetAttributeOptions {
  Items: Record<number, string>;
}

export interface AddSetAttribute extends Omit<LooseDataAttribute, 'Id' | 'AddonId'> {}
export interface EditSetAttribute extends Omit<LooseDataAttribute, 'AddonId' | 'DataType'> {}
