module CommandValidations
open Data
open Domain

let validateOpenTab tab =
  let table = getTableByNumber tab.TableNumber
  match table with
  | Some t ->
    match t.Status with
    | Closed -> Choice1Of2(tab)
    | Open tabId ->
      sprintf "Table Number %d is opened by tabID %A" tab.TableNumber tabId
      |> Choice2Of2
  | None -> "Invalid Table Number" |> Choice2Of2

let validatePlaceOrder tabId drinksItems foodItems =
  let table = getTableByTabId tabId
  match table with
  | Some (t) ->
    match foodItems, drinksItems with
    | Choice1Of2 foods, Choice1Of2 drinks ->
      if List.isEmpty foods && List.isEmpty drinks then
        Choice2Of2 "Order Should Contain atleast 1 food or drinks"
      else
        {
          TabId = tabId
          DrinksItems = drinks
          FoodItems = foods
        } |> Choice1Of2
    | Choice2Of2 fkeys, Choice2Of2 dkeys ->
        sprintf "Invalid Food Keys : %A,Invalid Drinks Keys %A" fkeys dkeys
        |> Choice2Of2
    | Choice2Of2 keys, _ ->
      sprintf "Invalid Food Keys : %A" keys |> Choice2Of2
    | _, Choice2Of2 keys ->
        sprintf "Invalid Drinks Keys : %A" keys |> Choice2Of2
  | None -> Choice2Of2 "Invalid Tab Id"