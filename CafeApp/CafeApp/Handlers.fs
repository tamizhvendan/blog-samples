module Handlers
open System
open Domain
open Events
open Commands
open Aggregates
open Chessie.ErrorHandling

let apply event state =
  match state, event  with
  | ClosedTab, TabOpened openTab  -> OpenedTab openTab
  | _ -> state

let execute command state =
  match command with
  | OpenTab tab ->
    match state with
    | ClosedTab ->
       {Tab = tab; OpenedAt = DateTime.Now}
       |> TabOpened |> ok
    | _ -> fail TabAlreadyOpened
