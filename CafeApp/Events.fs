module Events
open Domain
open System

type Event =
  | TabOpened
  | OrderPlaced of Order
  | ItemServed of OrderItem
  | PaymentMade of Payment
  | TabClosed