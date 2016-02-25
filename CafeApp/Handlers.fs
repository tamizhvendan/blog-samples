module Handlers
open System
open Domain
open Events
open Commands
open Chessie.ErrorHandling
open Aggregates
open Errors

let handleOpenTab = function
  | ClosedTab -> TabOpened |> ok
  | _ -> fail TabAlreadyOpened

let handlePlaceOrder order state =
  match state with
  | OpenedTab _ -> order |> OrderPlaced |> ok
  | ClosedTab -> fail CanNotOrderWithClosedTab
  | _ -> fail OrderAlreadyPlaced

let handleServeItem item state =
  match state with
  | PlacedOrder placedOrder ->
      let orderedItems = placedOrder.Items
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

let handleCloseTab (Payment amount) state =
  match state with
  | OrderServed order ->
      match order.TotalAmount = amount with
      | true -> TabClosed |> ok
      | false ->
        (Payment(amount), order.TotalAmount) |> InvalidPayment |> fail
  | _ -> CanNotPayForNonServedOrder |> fail

let execute state command  =
  match command with
  | OpenTab -> handleOpenTab state
  | PlaceOrder order -> handlePlaceOrder order state
  | ServeItem item -> handleServeItem item state
  | CloseTab payment -> handleCloseTab payment state

let evolve state command =
  match execute state command  with
  | Ok (event, _) ->
      let newState = apply state event
      ok (newState,event)
  | Bad errors ->
      errors |> List.head |> fail