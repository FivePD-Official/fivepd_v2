export interface Set {
  Id: string;
  Attributes: VariableDataAttribute[];
}

export interface VariableDataAttribute {
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

export interface AddSetAttribute extends Omit<VariableDataAttribute, 'Id' | 'AddonId'> {}
export interface EditSetAttribute extends Omit<VariableDataAttribute, 'AddonId' | 'DataType'> {}
