module Aggregates
open Events
open Domain

type PartiallyServedOrder = {
  PlacedOrder : Order
  ServedItems : OrderItem list
  NonServedItems : OrderItem list
}

type State =
  | ClosedTab
  | OpenedTab
  | PlacedOrder of Order
  | OrderPartiallyServed of PartiallyServedOrder
  | OrderServed of Order

let apply state event  =
  match state, event  with
  | ClosedTab, TabOpened -> OpenedTab
  | OpenedTab _, OrderPlaced placeOrder -> PlacedOrder placeOrder
  | PlacedOrder placeOrder, ItemServed item ->
      let nonServedItems = placeOrder.Items |> List.filter ((<>) item)
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
  | OrderServed _, TabClosed -> ClosedTab
  | _ -> state