module Domain
open System

type Tab = {
  Id : Guid
  TableNumber : int
}

type Item = {
  MenuNumber : int
  Price : decimal
  Name : string
}

type FoodItem = FoodItem of Item
type DrinksItem = DrinksItem of Item

type Payment = {
  Tab : Tab
  Amount : decimal
}

type Order = {
  FoodItems : FoodItem list
  DrinksItems : DrinksItem list
  TabId : Guid
}

let orderAmount order =
  let foodAmount =
    order.FoodItems |> List.map (fun (FoodItem f) -> f.Price) |> List.sum
  let drinksAmount =
    order.DrinksItems |> List.map (fun (DrinksItem d) -> d.Price) |> List.sum
  foodAmount + drinksAmount

type InProgressOrder = {
  PlacedOrder : Order
  ServedDrinks : DrinksItem list
  ServedFoods : FoodItem list
  PreparedFoods : FoodItem list
}
with
    member this.NonServedDrinks =
      List.except this.ServedDrinks this.PlacedOrder.DrinksItems
    member this.NonServedFoods =
      List.except this.ServedFoods this.PlacedOrder.FoodItems
    member this.NonPreparedFoods =
      List.except this.PreparedFoods this.PlacedOrder.FoodItems
    member this.IsOrderServed =
      List.isEmpty this.NonServedFoods && List.isEmpty this.NonServedDrinks

type TabStatus = Open of Guid | Closed

type Table = {
  Number : int
  Waiter : string
  Status : TabStatus
}

type ChefToDo = {
  TabId : Guid
  FoodItems : FoodItem list
}

type WaiterToDo = {
  TabId : Guid
  FoodItems : FoodItem list
  DrinksItem : DrinksItem list
}