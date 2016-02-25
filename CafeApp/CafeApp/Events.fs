module Events
open Domain
open System

type OpenTab = {
  Tab : Tab
  OpenedAt : DateTime
}

type Event =
  | TabOpened of OpenTab
