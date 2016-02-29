module Handlers
open System
open Domain
open Events
open Commands
open Chessie.ErrorHandling
open Aggregates
open Errors

let handleOpenTab tab = function
  | ClosedTab -> TabOpened tab |> ok
  | _ -> fail TabAlreadyOpened

let handlePlaceOrder placedOrder state =
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
  | OrderInProgress ipo ->
      match List.contains item ipo.NonServedDrinks with
      | true -> DrinksServed item |> ok
      | false -> (item, ipo.NonServedDrinks) |> ServingNonOrderedDrinks |> fail
  | OrderServed _ -> OrderAlreadyServed |> fail
  | OpenedTab _ -> CanNotServeForNonPlacedOrder |> fail
  | ClosedTab -> CanNotServeWithClosedTab |> fail

let handlePrepareFood item tabId state =
  match state with
  | PlacedOrder placedOrder ->
      let orderedFoods = placedOrder.FoodItems
      match List.contains item orderedFoods with
      | true -> FoodPrepared {
                  TabId = placedOrder.TabId
                  FoodItem = item
                } |> ok
      | false -> (item, orderedFoods) |> CanNotPrepareNotOrderedFoods |> fail
  | OrderInProgress ipo ->
      match List.contains item ipo.PreparedFoods with
      | true -> FoodAlreadyPrepared |> fail
      | false ->
        match List.contains item ipo.NonServedFoods with
        | true -> FoodPrepared {
                    TabId = tabId
                    FoodItem = item
                  } |> ok
        | false ->
          (item, ipo.NonServedFoods) |> CanNotPrepareNotOrderedFoods |> fail
  | OrderServed _ -> OrderAlreadyServed |> fail
  | OpenedTab _ -> CanNotPrepareForNonPlacedOrder |> fail
  | ClosedTab -> CanNotPrepareWithClosedTab |> fail

let handleServeFood item state =
    match state with
    | OrderInProgress ipo ->
        match List.contains item ipo.PreparedFoods with
        | true -> FoodServed item |> ok
        | false ->
          match List.contains item ipo.NonServedFoods with
          | true -> CanNotServeNonPreparedFood |> fail
          | false ->
            (item, ipo.NonServedFoods) |> ServingNonOrderedFood |> fail
    | PlacedOrder _ -> CanNotServeNonPreparedFood |> fail
    | OrderServed _ -> OrderAlreadyServed |> fail
    | OpenedTab _ -> CanNotServeForNonPlacedOrder |> fail
    | ClosedTab -> CanNotServeWithClosedTab |> fail

let handleCloseTab payment state =
  match state with
  | OrderServed po ->
      let totalAmount = placedOrderAmount po
      match totalAmount = payment.Amount with
      | true -> TabClosed payment |> ok
      | false ->
        (payment.Amount, totalAmount) |> InvalidPayment |> fail
  | _ -> CanNotPayForNonServedOrder |> fail

let execute state command  =
  match command with
  | OpenTab tab -> handleOpenTab tab state
  | PlaceOrder order -> handlePlaceOrder order state
  | ServeDrinks (item, _) -> handleServeDrinks item state
  | PrepareFood (item, tabId) -> handlePrepareFood item tabId state
  | ServeFood (item, _) -> handleServeFood item state
  | CloseTab payment -> handleCloseTab payment state


let evolve state command =
  match execute state command  with
  | Ok (event, _) ->
      let newState = apply state event
      ok (newState,event)
  | Bad errors ->
      errors |> List.head |> fail