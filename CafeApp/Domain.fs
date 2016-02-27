module Domain
open System

//type Tab = {
//  Id : Guid
//  TableNumber : int
//  Waiter : string
//}

type Tab = {
  Id : Guid
  TableNumber : int
  Waiter : string
}

type Item = {
  MenuNumber : int
  Price : decimal
  Name : string
}

type FoodItem = FoodItem of Item
type DrinksItem = DrinksItem of Item

type OrderItem =
| Food of FoodItem
| Drinks of DrinksItem

let price = function
| Food (FoodItem f) -> f.Price
| Drinks (DrinksItem d) -> d.Price

type Payment = {
  Tab : Tab
  Amount : decimal
}

type Order = {
  Tab : Tab
  Items : OrderItem list
}