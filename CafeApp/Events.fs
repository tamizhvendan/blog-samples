module Events
open Domain
open System

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
  | FoodPrepared of FoodItem * Guid
  | FoodServed of FoodItem * Guid
  | TabClosed of Payment