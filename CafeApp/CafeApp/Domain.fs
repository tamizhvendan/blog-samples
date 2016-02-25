module Domain
open System

type Tab = {
  Id : Guid
  TableNumber : int
  Waiter : string
}

type Item = {
  MenuNumber : int
  Price : decimal
  Description : string
}

type OrderItem =
| Food of Item
| Drinks of Item

type Order = {
  Id : Guid
  OrderItems : OrderItem list
}
