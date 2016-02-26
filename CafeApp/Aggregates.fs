module Aggregates
open Events
open Domain
open System

type PartiallyServedOrder = {
  PlacedOrder : PlacedOrder
  ServedDrinks : DrinksItem list
  ServedFoods : FoodItem list
}
with
    member this.NonServedDrinks =
      List.except this.ServedDrinks this.PlacedOrder.DrinksItems
    member this.NonServedFoods =
      List.except this.ServedFoods this.PlacedOrder.FoodItems
    member this.IsOrderServed =
      List.isEmpty this.NonServedFoods && List.isEmpty this.NonServedDrinks

type State =
  | ClosedTab
  | OpenedTab
  | PlacedOrder of PlacedOrder
  | OrderPartiallyServed of PartiallyServedOrder
  | OrderServed of PlacedOrder

let getState (pso : PartiallyServedOrder) =
  if pso.IsOrderServed then
    OrderServed pso.PlacedOrder
  else
    OrderPartiallyServed pso

let apply state event  =
  match state, event  with
  | ClosedTab, TabOpened -> OpenedTab
  | OpenedTab _, OrderPlaced placeOrder -> PlacedOrder placeOrder
  | PlacedOrder placedOrder, DrinksServed item ->
      match List.contains item placedOrder.DrinksItems with
      | true ->
          {
            PlacedOrder = placedOrder
            ServedDrinks = [item]
            ServedFoods = []
          } |> getState
      | false -> PlacedOrder placedOrder
  | OrderPartiallyServed pso, DrinksServed item ->
      match List.contains item pso.NonServedDrinks with
      | true ->
          {pso with ServedDrinks = item :: pso.ServedDrinks}
          |> getState
      | false -> PlacedOrder pso.PlacedOrder
  | OrderServed _, TabClosed -> ClosedTab
  | _ -> state