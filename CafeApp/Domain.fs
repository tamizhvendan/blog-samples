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