module Domain

type Food = {
  MenuNumber : int
  Name : string
}

type Order = {
  Foods : Food list
}

type InProgressOrder = {
  PlacedOrder : Order
  PreparedFoods : Food list
  ServedFoods : Food list
}

let (|NonOrderedFood|_|) order food =
  match List.contains food order.Foods with
  | false -> Some food
  | true -> None

let (|UnPreparedFood|_|) ipo food =
  match List.contains food ipo.PreparedFoods with
  | false -> Some food
  | true -> None

let (|PreparedFood|_|) ipo food =
  match List.contains food ipo.PreparedFoods with
  | true -> Some food
  | false -> None

let (|AlreadyServedFood|_|) ipo food =
  match List.contains food ipo.ServedFoods with
  | true -> Some food
  | false -> None

let (|AlreadyPreparedFood|_|) ipo food =
  match List.contains food ipo.PreparedFoods with
  | true -> Some food
  | false -> None

let (|CompletesOrder|_|) ipo food =
  let nonServedFoods =
    List.except ipo.ServedFoods ipo.PlacedOrder.Foods
  match nonServedFoods = [food] with
  | true -> Some food
  | false -> None

let serveFood food ipo =
  match food with
  | NonOrderedFood ipo.PlacedOrder _ ->
    printfn "Can not serve non ordered food"
  | UnPreparedFood ipo _ ->
    printfn "Can not serve unprepared food"
  | AlreadyServedFood ipo _ ->
    printfn "Can not serve already served food"
  | CompletesOrder ipo _ ->
    printfn "Order Served"
  | _ -> printfn "Still some food(s) to serve"

let prepareFood food ipo =
  match food with
  | NonOrderedFood ipo.PlacedOrder _ ->
    printfn "Can not prepare non ordered food"
  | PreparedFood ipo _ ->
    printfn "Can not prepare already prepared food"
  | AlreadyServedFood ipo _ ->
    printfn "Can not prepare already served food"
  | _ -> printfn "Prepare Food"