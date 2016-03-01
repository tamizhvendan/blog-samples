module Data
open System
open System.Collections.Generic
open Domain
open Events

let private tables =
  let dict = new Dictionary<int, Table>()
  dict.Add(1, {Number = 1; Waiter = "X"; Status = Closed})
  dict.Add(2, {Number = 2; Waiter = "y"; Status = Closed})
  dict.Add(3, {Number = 3; Waiter = "Z"; Status = Closed})
  dict

let updateTableStatus tableNumber status =
  let table = tables.[tableNumber]
  tables.[tableNumber] <- {table with Status = status}

let getTableByTabId tabId =
  tables.Values
  |> Seq.tryFind(fun t ->
                  match t.Status with
                  | (Open id) -> id = tabId
                  | _ -> false)
let getItem<'a> (dict : Dictionary<int,'a>) key =
  if dict.ContainsKey key then
    dict.[key] |> Some
  else
    None

let getTableByNumber = getItem tables

let getTables () = tables.Values |> Seq.toList

let private chefToDos = new Dictionary<Guid, ChefToDo>()

let addChefToDo tabId foodItems =
  match getTableByTabId tabId with
  | Some table ->
    let todo : ChefToDo  =
      {Tab = {Id = tabId; TableNumber = table.Number}; FoodItems = foodItems}
    chefToDos.Add(tabId, todo)
  | None -> ()

let removeChefToDo tabId = chefToDos.Remove(tabId) |> ignore
let getChefToDos () = chefToDos.Values |> Seq.toList
let removeFoodFromChefToDo item tabId =
  let todo = chefToDos.[tabId]
  let chefToDo =
    { todo with FoodItems = List.filter (fun d -> d <> item) todo.FoodItems}
  chefToDos.[tabId] <- chefToDo

let private waiterToDos = new Dictionary<Guid, WaiterToDo>()

let addWaiterToDo tabId drinksItem =
  match getTableByTabId tabId with
  | Some table ->
    let todo : WaiterToDo  =
      { Tab = {Id = tabId; TableNumber = table.Number}
        FoodItems = []
        DrinksItems = drinksItem}
    waiterToDos.Add(tabId, todo)
  | None -> ()

let addFoodToWaiterToDo item tabId =
  let todo = waiterToDos.[tabId]
  let waiterToDo =
    {todo with FoodItems = item :: todo.FoodItems}
  waiterToDos.[tabId] <- waiterToDo
let removeWaiterToDo tabId = waiterToDos.Remove(tabId) |> ignore
let getWaiterToDos () = waiterToDos.Values |> Seq.toList

let removeDrinksFromWaiterToDo item tabId =
  let todo = waiterToDos.[tabId]
  let waiterToDo =
    { todo with
        DrinksItems = List.filter (fun d -> d <> item) todo.DrinksItems }
  waiterToDos.[tabId] <- waiterToDo

let removeFoodFromWaiterToDo item tabId =
  let todo = waiterToDos.[tabId]
  let waiterToDo =
    { todo with
        FoodItems = List.filter (fun d -> d <> item) todo.FoodItems }
  waiterToDos.[tabId] <- waiterToDo

let private cashierToDos = new Dictionary<Guid, Payment>()

let addCashierToDo tabId amount =
  match getTableByTabId tabId with
  | Some table ->
    let payment  =
      {Tab = {Id = tabId; TableNumber = table.Number}; Amount = amount}
    cashierToDos.Add(tabId, payment)
  | None -> ()

let removeCashierToDo tabId = cashierToDos.Remove(tabId) |> ignore
let getCashierToDos () = cashierToDos.Values |> Seq.toList

let private foodItems =
  let dict = new Dictionary<int, FoodItem>()
  dict.Add(8, FoodItem {
    MenuNumber = 8
    Price = 5m
    Name = "Salad"
  })
  dict.Add(9, FoodItem {
    MenuNumber = 9
    Price = 10m
    Name = "Pizza"
  })
  dict

let private getItems<'a> (dict : Dictionary<int,'a>) keys =
  let invalidKeys = keys |> Array.except dict.Keys
  if Array.isEmpty invalidKeys then
    keys
    |> Array.map (fun n -> dict.[n])
    |> Array.toList
    |> Choice1Of2
  else
    invalidKeys |> Choice2Of2



let getFoodItems = getItems foodItems
let getFoodByMenuNumber = getItem foodItems
let private drinksItems =
  let dict = new Dictionary<int, DrinksItem>()
  dict.Add(10, DrinksItem {
      MenuNumber = 10
      Price = 2.5m
      Name = "Coke"
  })
  dict.Add(11, DrinksItem {
    MenuNumber = 11
    Name = "Lemonade"
    Price = 1.5m
  })
  dict

let getDrinksItems = getItems drinksItems
let getDrinksByMenuNumber = getItem drinksItems