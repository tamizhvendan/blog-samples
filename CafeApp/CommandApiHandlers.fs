module CommandApiHandlers

open Domain
open Data
open Suave.RequestErrors
open CommandRequests
open Suave.Successful

let handleOpenTab tab  =
  let table = getTableByNumber tab.TableNumber
  match table with
  | Some t ->
    match t.Status with
    | Closed -> OK <| sprintf "%A" tab
    | Open tabId ->
      BAD_REQUEST
      <| sprintf "Table Number %d is opened by tabID %A" tab.TableNumber tab.Id
  | None ->
    BAD_REQUEST "Invalid Table Number"

let handlePlaceOrder (placeOrder : PlaceOrderReq.PlaceOrder) =
  let table = getTableByTabId placeOrder.TabId
  let isEmptyOrder =
    Array.isEmpty placeOrder.Drinks && Array.isEmpty placeOrder.Foods
  match table, isEmptyOrder with
  | Some (t), false ->
    let foodItems = getFoodItems placeOrder.Foods
    let drinksItems = getDrinksItems placeOrder.Drinks
    match foodItems, drinksItems with
    | Choice1Of2 foods, Choice1Of2 drinks ->
      {
        TabId = placeOrder.TabId
        DrinksItems = drinks
        FoodItems = foods
      } |> sprintf "%A" |> OK
    | Choice2Of2 fkeys, Choice2Of2 dkeys ->
        let msg =
          sprintf "Invalid Food Keys : %A,Invalid Drinks Keys %A" fkeys dkeys
        BAD_REQUEST msg
    | Choice2Of2 keys, _ ->
        BAD_REQUEST <| sprintf "Invalid Food Keys : %A" keys
    | _, Choice2Of2 keys ->
        BAD_REQUEST <| sprintf "Invalid Drinks Keys : %A" keys
  | _,true -> BAD_REQUEST "Order Should Contain atleast 1 food or drinks"
  | None,_ -> BAD_REQUEST "Invalid Tab Id"