module Events
open Domain
open System

type PlacedOrder = {
  FoodItems : FoodItem list
  DrinksItems : DrinksItem list
  Tab : Tab
}

type PreparedFood = {
  FoodItem : FoodItem
  Tab : Tab
}

let placedOrderAmount po =
  let foodAmount =
    po.FoodItems |> List.map (Food >> price) |> List.sum
  let drinksAmount =
    po.DrinksItems |> List.map (Drinks >> price) |> List.sum
  foodAmount + drinksAmount

type Event =
  | TabOpened of Tab
  | OrderPlaced of PlacedOrder
  | DrinksServed of DrinksItem
  | FoodPrepared of PreparedFood
  | FoodServed of FoodItem
  | TabClosed of Payment