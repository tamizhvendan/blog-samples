module Domain
open System
open Chessie.ErrorHandling

type Tab = {
  Id : Guid
  TableNumber : int
  Waiter : string
}

type OpenTab = {
  Tab : Tab
  OpenedAt : DateTime
}

type Command =
  | OpenTab of Tab

type Event =
  | TabOpened of OpenTab

type State =
  | ClosedTab
  | OpenedTab of OpenTab

type InvalidState =
  | TabAlreadyOpened


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
