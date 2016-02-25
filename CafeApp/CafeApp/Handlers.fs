module Handlers
open System
open Domain
open Events
open Commands
open Chessie.ErrorHandling
open Aggregates

let handleOpenTab tab state =
  match state with
  | ClosedTab ->
     tab |> TabOpened |> ok
  | _ -> fail TabAlreadyOpened

let handlePlaceOrder order state =
  match state with
  | OpenedTab tab ->
      {Order = order; Tab = tab}
      |> OrderPlaced |> ok
  | ClosedTab -> fail CanNotOrderWithClosedTab
  | _ -> fail OrderAlreadyPlaced

let handleServeItems item state =
  match state with
  | PlacedOrder placedOrder ->
      let orderedItems = placedOrder.Order.Items
      match List.contains item orderedItems  with
      | true -> ItemServed item |> ok
      | false -> (item, orderedItems) |> ServingNonOrderedItem |> fail
  | OrderPartiallyServed pso ->
      let nonServedItems = pso.NonServedItems
      match List.contains item nonServedItems with
      | true -> ItemServed item |> ok
      | false -> (item, nonServedItems) |> ServingNonOrderedItem |> fail
  | OrderServed _ -> OrderAlreadyServed |> fail
  | OpenedTab _ -> CanNotServeForNonPlacedOrder |> fail
  | ClosedTab -> CanNotServeWithClosedTab |> fail


let execute state command  =
  match command with
  | OpenTab tab -> handleOpenTab tab state
  | PlaceOrder order -> handlePlaceOrder order state
  | ServeItems item -> handleServeItems item state

let evolve state command =
  match execute state command  with
  | Ok (event, _) ->
      let newState = apply state event
      ok (newState,event)
  | Bad errors ->
      errors |> List.head |> fail