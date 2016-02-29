module CommandApiHandlers

open Domain
open Data
open Suave.RequestErrors
open Suave.ServerErrors
open CommandRequests
open Suave.Successful
open Aggregates
open ReadModel
open Handlers
open EventsStore
open Commands
open Chessie.ErrorHandling

let eventStore = getState InMemoryEventStore.Instance
let saveEvent = saveEvent InMemoryEventStore.Instance

let handleCommand state command =
  match evolve state command with
  | Ok(result,_) ->
    match saveEvent result with
    | Ok(result,_) ->
        match dispatchEvent result with
        | Ok(result,_) ->
          OK <| sprintf "%A" result
        | Bad err -> BAD_REQUEST <| sprintf "%A" err
    | Bad err -> BAD_REQUEST <| sprintf "%A" err
  | Bad err -> BAD_REQUEST <| sprintf "%A" err


let handleOpenTab tab  =
  let table = getTableByNumber tab.TableNumber
  match table with
  | Some t ->
    match t.Status with
    | Closed ->
      match eventStore tab.Id with
      | Ok (state,_) -> handleCommand state (OpenTab tab)
      | Bad _ -> INTERNAL_ERROR "Unable to retrieve events from event store"
    | Open tabId ->
      BAD_REQUEST
      <| sprintf "Table Number %d is opened by tabID %A" tab.TableNumber tabId
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