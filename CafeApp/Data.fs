module Data
open System
open System.Collections.Generic
open Domain
open Events
open ReadModel

type TabStatus = Open of Guid | Closed

type Table = {
  Number : int
  Waiter : string
  Status : TabStatus
}

type ChefTodo = {
  Tab : Tab
  FoodItems : FoodItem list
}

let private tables =
  let dict = new Dictionary<int, Table>()
  dict.Add(1, {Number = 1; Waiter = "X"; Status = Closed})
  dict.Add(2, {Number = 2; Waiter = "y"; Status = Closed})
  dict.Add(3, {Number = 3; Waiter = "Z"; Status = Closed})
  dict


let updateTableStatus tableNumber status =
  let table = tables.[tableNumber]
  tables.[tableNumber] <- {table with Status = status}

let getTables () = tables.Values |> Seq.toList;

let projectReadModel e =
  match e with
  | TabOpened tab ->
      updateTableStatus tab.TableNumber (Open tab.Id)
  | OrderPlaced placedOrder ->
      {
        Tab = placedOrder.Tab
        FoodItems = placedOrder.FoodItems
      } |> ignore
  | FoodPrepared pf ->
      pf |> ignore
  | TabClosed payment ->
      updateTableStatus payment.Tab.TableNumber Closed
  | _ -> ()