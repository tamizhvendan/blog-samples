module CommandValidations
open Domain

let validateOpenTab table tab =
  match table with
  | Some t ->
    match t.Status with
    | Closed -> Choice1Of2(tab)
    | Open tabId ->
      sprintf "Table Number %d is already opened" tab.TableNumber
      |> Choice2Of2
  | None -> "Invalid Table Number" |> Choice2Of2

let validateCloseTab table =
  match table with
  | Some t ->
    match t.Status with
    | Open tabId -> (t.Number, tabId) |> Choice1Of2
    | Closed -> "Invalid Request. Table is closed" |> Choice2Of2
  | None -> "Invalid Tab Id" |> Choice2Of2

let validatePlaceOrder table drinksItems foodItems =
  match table with
  | Some (t) ->
    match foodItems, drinksItems with
    | Choice1Of2 foods, Choice1Of2 drinks ->
      if List.isEmpty foods && List.isEmpty drinks then
        Choice2Of2 "Order Should Contain atleast 1 food or drinks"
      else
        (drinks, foods) |> Choice1Of2
    | Choice2Of2 fkeys, Choice2Of2 dkeys ->
        sprintf "Invalid Food Keys : %A, Invalid Drinks Keys %A" fkeys dkeys
        |> Choice2Of2
    | Choice2Of2 keys, _ ->
      sprintf "Invalid Food Keys : %A" keys |> Choice2Of2
    | _, Choice2Of2 keys ->
        sprintf "Invalid Drinks Keys : %A" keys |> Choice2Of2
  | None -> Choice2Of2 "Invalid Tab Id"

let validateItem table item msg =
  match table with
  | Some (t) ->
    match item with
    | Some i -> i |> Choice1Of2
    | None -> Choice2Of2 msg
  | None -> Choice2Of2 "Invalid Tab Id"