module Events
open Domain
open System

type PlacedOrder = {
  FoodItems : FoodItem list
  DrinksItems : DrinksItem list
  Id : Guid
}

let placedOrderAmount po =
  let foodAmount =
    po.FoodItems |> List.map (Food >> price) |> List.sum
  let drinksAmount =
    po.DrinksItems |> List.map (Drinks >> price) |> List.sum
  foodAmount + drinksAmount

type Event =
  | TabOpened
  | OrderPlaced of PlacedOrder
  | DrinksServed of DrinksItem
  | FoodPrepared of FoodItem
  | FoodServed of FoodItem
  | PaymentMade of Payment
  | TabClosed