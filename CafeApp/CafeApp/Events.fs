module Events
open Domain
open System

type PlaceOrder = {
  Order : Order
  Tab : Tab
}

type Event =
  | TabOpened of Tab
  | OrderPlaced of PlaceOrder
  | ItemServed of OrderItem