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
  let foods =
    order.Items |> List.choose (function | (Food f) -> Some f | _ -> None)
  let drinks =
    order.Items |> List.choose (function | (Drinks d) -> Some d | _ -> None)
  let placedOrder = {
    FoodItems = foods
    DrinksItems = drinks
    Id = order.Id
  }
  match state with
  | OpenedTab _ -> placedOrder |> OrderPlaced |> ok
  | ClosedTab -> fail CanNotOrderWithClosedTab
  | _ -> fail OrderAlreadyPlaced

let handleServeDrinks item state =
  match state with
  | PlacedOrder placedOrder ->
      let orderedDrinks = placedOrder.DrinksItems
      match List.contains item orderedDrinks with
      | true -> DrinksServed item |> ok
      | false -> (item, orderedDrinks) |> ServingNonOrderedDrinks |> fail
  | OrderPartiallyServed pso ->
      match List.contains item pso.NonServedDrinks with
      | true -> DrinksServed item |> ok
      | false -> (item, pso.NonServedDrinks) |> ServingNonOrderedDrinks |> fail
  | OrderServed _ -> OrderAlreadyServed |> fail
  | OpenedTab _ -> CanNotServeForNonPlacedOrder |> fail
  | ClosedTab -> CanNotServeWithClosedTab |> fail

let handleCloseTab (Payment amount) state =
  match state with
  | OrderServed po ->
      let totalAmount = placedOrderAmount po
      match totalAmount = amount with
      | true -> TabClosed |> ok
      | false ->
        (Payment(amount), totalAmount) |> InvalidPayment |> fail
  | _ -> CanNotPayForNonServedOrder |> fail

let execute state command  =
  match command with
  | OpenTab -> handleOpenTab state
  | PlaceOrder order -> handlePlaceOrder order state
  | ServeDrinks item -> handleServeDrinks item state
  | CloseTab payment -> handleCloseTab payment state

let evolve state command =
  match execute state command  with
  | Ok (event, _) ->
      let newState = apply state event
      ok (newState,event)
  | Bad errors ->
      errors |> List.head |> fail