module QueryAST


type Action = Select of Identifier
  and Identifier = Asterisk | Attributes of Attribute list
  and Attribute = Attribute of string

type From = From of string

type Where = Where of Condition
  and Condition = { Attribute : Attribute
                    Operator : Operator
                    Operant : Operant}
  and Operator = Equal | NotEqual
  and Operant = String of string | Int of int

type Query = {
  Action : Action
  From : From
  Where : Where option
}
