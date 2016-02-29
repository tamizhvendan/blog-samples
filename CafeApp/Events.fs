module Events
open Domain
open System

type PreparedFood = {
  FoodItem : FoodItem
  TabId : Guid
}

let placedOrderAmount (po : Order) =
  let foodAmount =
    po.FoodItems |> List.map (fun (FoodItem f) -> f.Price) |> List.sum
  let drinksAmount =
    po.DrinksItems |> List.map (fun (DrinksItem d) -> d.Price) |> List.sum
  foodAmount + drinksAmount

type Event =
  | TabOpened of Tab
  | OrderPlaced of Order
  | DrinksServed of DrinksItem * Guid
  | FoodPrepared of PreparedFood
  | FoodServed of FoodItem
  | TabClosed of Payment