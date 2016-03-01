module Aggregates
open Events
open Domain
open System

type State =
  | ClosedTab of Guid option
  | OpenedTab of Tab
  | PlacedOrder of Order
  | OrderInProgress of InProgressOrder
  | OrderServed of Order

let getState (ipo : InProgressOrder) =
  if ipo.IsOrderServed then
    OrderServed ipo.PlacedOrder
  else
    OrderInProgress ipo

let apply state event  =
  match state, event  with
  | ClosedTab _, TabOpened tab -> OpenedTab tab
  | OpenedTab _, OrderPlaced placeOrder -> PlacedOrder placeOrder
  | PlacedOrder placedOrder, DrinksServed (item,_) ->
      match List.contains item placedOrder.DrinksItems with
      | true ->
          {
            PlacedOrder = placedOrder
            ServedDrinks = [item]
            ServedFoods = []
            PreparedFoods = []
          } |> getState
      | false -> PlacedOrder placedOrder
  | OrderInProgress ipo, DrinksServed (item,_) ->
      match List.contains item ipo.NonServedDrinks with
      | true ->
          {ipo with ServedDrinks = item :: ipo.ServedDrinks}
          |> getState
      | false -> OrderInProgress ipo
  | PlacedOrder placedOrder, FoodPrepared (food, _) ->
      match List.contains food placedOrder.FoodItems with
      | true ->
          {
            PlacedOrder = placedOrder
            ServedDrinks = []
            ServedFoods = []
            PreparedFoods = [food]
          } |> OrderInProgress
      | false -> PlacedOrder placedOrder
  | OrderInProgress ipo, FoodPrepared (food,_) ->
      match List.contains food ipo.NonPreparedFoods with
      | true ->
          {ipo with PreparedFoods = food :: ipo.PreparedFoods}
          |> OrderInProgress
      | false -> OrderInProgress ipo
  | OrderInProgress ipo, FoodServed (food, _) ->
      let isPrepared = List.contains food ipo.PreparedFoods
      let isNonServed = List.contains food ipo.NonServedFoods
      match isPrepared, isNonServed with
      | true,true ->
          {ipo with ServedFoods = food :: ipo.ServedFoods}
          |> getState
      | _ -> OrderInProgress ipo
  | OrderServed order, TabClosed _ -> ClosedTab (Some order.TabId)
  | _ -> state