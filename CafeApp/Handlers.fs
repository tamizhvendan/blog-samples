module Handlers
open System
open Domain
open Events
open Commands
open Chessie.ErrorHandling
open Aggregates
open Errors

let handleOpenTab tab = function
  | ClosedTab _ -> TabOpened tab |> ok
  | _ -> fail TabAlreadyOpened

let handlePlaceOrder placedOrder state =
  match state with
  | OpenedTab _ -> placedOrder |> OrderPlaced |> ok
  | ClosedTab _ -> fail CanNotOrderWithClosedTab
  | _ -> fail OrderAlreadyPlaced

let handleServeDrinks item tabId state =
  match state with
  | PlacedOrder order ->
      let orderedDrinks = order.DrinksItems
      match List.contains item orderedDrinks with
      | true -> DrinksServed (item, tabId) |> ok
      | false -> (item, orderedDrinks) |> ServingNonOrderedDrinks |> fail
  | OrderInProgress ipo ->
      match List.contains item ipo.NonServedDrinks with
      | true -> DrinksServed (item, tabId) |> ok
      | false -> (item, ipo.NonServedDrinks) |> ServingNonOrderedDrinks |> fail
  | OrderServed _ -> OrderAlreadyServed |> fail
  | OpenedTab _ -> CanNotServeForNonPlacedOrder |> fail
  | ClosedTab _ -> CanNotServeWithClosedTab |> fail

let handlePrepareFood item tabId state =
  match state with
  | PlacedOrder order ->
      let orderedFoods = order.FoodItems
      match List.contains item orderedFoods with
      | true -> (item, tabId) |> FoodPrepared |> ok
      | false -> (item, orderedFoods) |> CanNotPrepareNotOrderedFoods |> fail
  | OrderInProgress ipo ->
      match List.contains item ipo.PreparedFoods with
      | true -> FoodAlreadyPrepared |> fail
      | false ->
        match List.contains item ipo.NonPreparedFoods with
        | true ->  (item, tabId) |> FoodPrepared |> ok
        | false ->
          (item, ipo.NonPreparedFoods) |> CanNotPrepareNotOrderedFoods |> fail
  | OrderServed _ -> OrderAlreadyServed |> fail
  | OpenedTab _ -> CanNotPrepareForNonPlacedOrder |> fail
  | ClosedTab _ -> CanNotPrepareWithClosedTab |> fail

let handleServeFood item tabId state =
    match state with
    | OrderInProgress ipo ->
        match List.contains item ipo.PreparedFoods with
        | true -> (item, tabId) |> FoodServed  |> ok
        | false ->
          match List.contains item ipo.NonPreparedFoods with
          | true -> CanNotServeNonPreparedFood |> fail
          | false ->
            (item, ipo.NonPreparedFoods) |> ServingNonOrderedFood |> fail
    | PlacedOrder _ -> CanNotServeNonPreparedFood |> fail
    | OrderServed _ -> OrderAlreadyServed |> fail
    | OpenedTab _ -> CanNotServeForNonPlacedOrder |> fail
    | ClosedTab _ -> CanNotServeWithClosedTab |> fail

let handleCloseTab payment state =
  match state with
  | OrderServed order ->
      let totalAmount = orderAmount order
      match totalAmount = payment.Amount with
      | true -> TabClosed payment |> ok
      | false ->
        (payment.Amount, totalAmount) |> InvalidPayment |> fail
  | _ -> CanNotPayForNonServedOrder |> fail

let execute state command  =
  match command with
  | OpenTab tab -> handleOpenTab tab state
  | PlaceOrder order -> handlePlaceOrder order state
  | ServeDrinks (item, tabId) -> handleServeDrinks item tabId state
  | PrepareFood (item, tabId) -> handlePrepareFood item tabId state
  | ServeFood (item, tabId) -> handleServeFood item tabId state
  | CloseTab payment -> handleCloseTab payment state


let evolve state command =
  match execute state command  with
  | Ok (event, _) ->
      let newState = apply state event
      ok (newState,event)
  | Bad errors ->
      errors |> List.head |> fail