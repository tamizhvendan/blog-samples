module Aggregates
open Events
open Domain

type PartiallyServedOrder = {
  PlacedOrder : PlaceOrder
  ServedItems : OrderItem list
  NonServedItems : OrderItem list
}

type State =
  | ClosedTab
  | OpenedTab of Tab
  | PlacedOrder of PlaceOrder
  | OrderPartiallyServed of PartiallyServedOrder
  | OrderServed of PlaceOrder

type Error =
  | TabAlreadyOpened
  | CanNotOrderWithClosedTab
  | OrderAlreadyPlaced
  | ServingNonOrderedItem of OrderItem * OrderItem list
  | CanNotServeForNonPlacedOrder
  | CanNotServeWithClosedTab
  | OrderAlreadyServed

let apply state event  =
  match state, event  with
  | ClosedTab, TabOpened openTab  -> OpenedTab openTab
  | OpenedTab _, OrderPlaced placeOrder -> PlacedOrder placeOrder
  | PlacedOrder placeOrder, ItemServed item ->
      let nonServedItems = placeOrder.Order.Items |> List.filter ((<>) item)
      match nonServedItems.Length = 0 with
      | true -> OrderServed placeOrder
      | false -> OrderPartiallyServed {
        PlacedOrder = placeOrder
        ServedItems = [item]
        NonServedItems = nonServedItems
      }
  | OrderPartiallyServed pso, ItemServed item ->
      let nonServedItems = pso.NonServedItems |> List.filter ((<>) item)
      match nonServedItems.Length = 0 with
      | true -> OrderServed pso.PlacedOrder
      | false -> OrderPartiallyServed {
                    pso with
                      ServedItems =  item :: pso.ServedItems
                      NonServedItems = nonServedItems }
    | _ -> state