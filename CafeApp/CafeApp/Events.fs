module Events
open Domain
open System

type OpenTab = {
  Tab : Tab
  OpenedAt : DateTime
}

type PlaceOrder = {
  Order : Order
  PlacedAt : DateTime
  Tab : Tab
}

type Event =
  | TabOpened of OpenTab
  | OrderPlaced of PlaceOrder
