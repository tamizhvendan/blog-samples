module Handlers
open System
open Domain
open Events
open Commands
open Aggregates
open Chessie.ErrorHandling

let apply state event  =
  match state, event  with
  | ClosedTab, TabOpened openTab  -> OpenedTab openTab
  | OpenedTab _, OrderPlaced placeOrder -> PlacedOrder placeOrder
  | _ -> state

let handleOpenTab tab state =
  match state with
  | ClosedTab ->
     {OpenedAt = DateTime.UtcNow; Tab = tab }
     |> TabOpened |> ok
  | _ -> fail TabAlreadyOpened

let handlePlaceOrder order state =
  match state with
  | OpenedTab openedTab ->
      {Order = order; Tab = openedTab.Tab; PlacedAt = DateTime.UtcNow}
      |> OrderPlaced |> ok
  | PlacedOrder _ -> fail OrderAlreadyPlaced
  | ClosedTab -> fail CanNotOrderWithClosedTab

let execute state command  =
  match command with
  | OpenTab tab -> handleOpenTab tab state
  | PlaceOrder order -> handlePlaceOrder order state

let evolve state command =
  match execute state command  with
  | Ok (event, _) ->
      let newState = apply state event
      ok (newState,event)
  | Bad errors ->
      errors |> List.head |> fail
