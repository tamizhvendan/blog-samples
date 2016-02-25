module Domain
open System

//type Tab = {
//  Id : Guid
//  TableNumber : int
//  Waiter : string
//}

type Item = {
  MenuNumber : int
  Price : decimal
  Description : string
}

type OrderItem =
| Food of Item
| Drinks of Item

type Payment = Payment of decimal

type Order = {
  Id : Guid
  Items : OrderItem list
} with member this.TotalAmount =
        this.Items
        |> List.map (function | Food i -> i | Drinks i -> i)
        |> List.sumBy (fun item -> item.Price)