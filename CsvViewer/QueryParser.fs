module QueryParser

open QueryAST
open FParsec

let pstringCIWS str = pstringCI str .>> spaces
let pstringWS str = pstring str .>> spaces
let stringReturnWS str r = stringReturn str r .>> spaces
let isWhiteSpace c = c = ' '
let isUnderScore c = c = '_'
let pIdentifierName label =
  let isIdentifierChar c = isLetter c || isDigit c || isUnderScore c
  many1SatisfyL isIdentifierChar label .>> spaces

let pStringValue =
  let isString c =
    isLetter c || isWhiteSpace c || isDigit c || isUnderScore c
  many1SatisfyL isString "string value"

let pasterik = stringReturnWS "*" Asterisk
let pattributes =
  sepBy1 (pIdentifierName "attribute name")  (pstringWS ",")
  |>> (fun names -> names |> List.map Attribute |> Attributes)
let pIdentifier =  pasterik <|> pattributes
let pselect =
  pipe2 (pstringCIWS "select") pIdentifier (fun _ attr -> Select(attr))
let paction = pselect

let pFrom = pstringCI "from"  .>> spaces >>. pIdentifierName "table name" |>> From

let pOperator = stringReturnWS "=" Equal <|> stringReturnWS "<>" NotEqual
let pStringOperant =
  pchar '\'' >>. pStringValue .>> pchar '\'' |>> String
let pIntOperant = pint32 |>> Int
let pOperant = pStringOperant <|> pIntOperant
let pCondtion =
  (pIdentifierName "attribute name") .>>. pOperator .>>. pOperant
  |>> fun ((attrName, operator), operant) ->
              {
                Attribute = Attribute(attrName)
                Operator = operator
                Operant = operant
              }
let pWhere =
  opt (pstringCIWS "where" >>. pCondtion)
  |>> Option.bind (Where >> Some)



let toAST action from where =
  {Action = action; From = from; Where = where}

let parser = pipe3 paction pFrom pWhere toAST


let parse queryDSL =
  match run parser queryDSL with
  | Success(result, _,_) ->
    Choice1Of2 result
  | Failure(msg,_,_) ->
    Choice2Of2 msg
