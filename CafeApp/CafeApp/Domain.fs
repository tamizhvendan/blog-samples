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

type AggregateRoot =
  | ClosedTab
  | OpenedTab of OpenTab

type InvalidState =
  | TabAlreadyOpened


let apply event aggregate =
  match aggregate, event  with
  | ClosedTab, TabOpened openTab  -> OpenedTab openTab
  | _ -> aggregate 

let execute command aggregate =
  match command with
  | OpenTab tab ->
    match aggregate with
    | ClosedTab ->
      ok {Tab = tab; OpenedAt = DateTime.Now}
    | _ -> fail TabAlreadyOpened
