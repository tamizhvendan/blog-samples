module QueryParser

open QueryAST
open FParsec

let pstringCIWS str = pstringCI str .>> spaces
let pstringWS str = pstring str .>> spaces
let stringReturnWS str r = stringReturn str r .>> spaces
let pName label = many1SatisfyL isLetter label .>> spaces

let pasterik = stringReturnWS "*" Asterisk
let pattributes =
  sepBy1 (pName "attribute name")  (pstringWS ",")
  |>> (fun names -> names |> List.map Attribute |> Attributes)
let pIdentifier =  pasterik <|> pattributes
let pselect =
  pipe2 (pstringCIWS "select") pIdentifier (fun _ attr -> Select(attr))
let paction = pselect

let pFrom = pstringCI "from"  .>> spaces >>. pName "table name" |>> From

let pOperator = stringReturnWS "=" Equal <|> stringReturnWS "<>" NotEqual
let pStringOperant =
  pchar '\'' >>. pName "operant string value" .>> pchar '\'' |>> String
let pIntOperant = pint32 |>> Int
let pOperant = pStringOperant <|> pIntOperant
let pCondtion =
  (pName "attribute name") .>>. pOperator .>>. pOperant
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
