module JsonResponse
open FSharp.Data
open Suave
open Suave.Successful
open Suave.Operators
open Suave.RequestErrors
open Suave.ServerErrors
open Aggregates
open Newtonsoft.Json.Linq
open Domain
open Events

let (.=) key (value : obj) = new JProperty(key, value)

let jobj jProperties =
  let jObject = new JObject()
  jProperties |> List.iter jObject.Add
  jObject

let jArray jObjects =
  let jArray = new JArray()
  jObjects |> List.iter jArray.Add
  jArray

let tabJObj tab =
  jobj [
    "id" .= tab.Id
    "tableNumber" .= tab.TableNumber
  ]
let itemJObj item =
  jobj [
    "menuNumber" .= item.MenuNumber
    "name" .= item.Name
  ]
let foodItemJObj (FoodItem item) = itemJObj item
let drinksItemJObj (DrinksItem item) = itemJObj item
let foodItemsJArray foodItems =
  foodItems |> List.map foodItemJObj |> jArray
let drinksItemsJArray drinksItems =
  drinksItems |> List.map drinksItemJObj |> jArray

let orderJObj order =
  jobj [
    "tabId" .= order.TabId
    "foodItems" .= foodItemsJArray order.FoodItems
    "drinksItems" .= drinksItemsJArray order.DrinksItems
  ]

let orderInProgressJObj ipo =
  jobj [
    "tabId" .=  ipo.PlacedOrder.TabId.ToString()
    "placedOrder" .= orderJObj ipo.PlacedOrder
    "servedFoods" .= foodItemsJArray ipo.ServedFoods
    "servedDrinks" .= drinksItemsJArray ipo.ServedDrinks
  ]

let stateJObj = function
| ClosedTab tabId ->
  let state = "state" .= "ClosedTab"
  match tabId with
  | Some id ->
    jobj [ state; "tabId" .= id.ToString() ]
  | None -> jobj [state]
| OpenedTab tab ->
  jobj [
    "state" .= "OpenedTab"
    "data" .= tabJObj tab
  ]
| PlacedOrder order ->
  jobj [
    "state" .= "PlacedOrder"
    "data" .= orderJObj order
  ]
| OrderInProgress ipo ->
  jobj [
    "state" .= "OrderInProgress"
    "data" .= orderInProgressJObj ipo
  ]
| OrderServed order ->
  jobj [
    "state" .= "OrderServed"
    "data" .= orderJObj order
  ]

let eventJObj = function
| TabOpened tab ->
  jobj [
    "event" .= "TabOpened"
    "data" .= tabJObj tab
  ]
| OrderPlaced order ->
  jobj [
    "event" .= "OrderPlaced"
    "data" .= orderJObj order
  ]
| DrinksServed (item,_) ->
  jobj [
    "event" .= "DrinksServed"
    "data" .= drinksItemJObj item
  ]
| FoodPrepared (item,_) ->
  jobj [
    "event" .= "FoodPrepared"
    "data" .= foodItemJObj item
  ]
| FoodServed (item,_) ->
  jobj [
    "event" .= "FoodServed"
    "data" .= foodItemJObj item
  ]
| TabClosed payment ->
  jobj [
    "event" .= "TabClosed"
    "paymentAmount" .= payment.Amount
  ]

let statusJObj = function
| Open tabId ->
  "status" .= jobj [
                "open" .= tabId.ToString()
              ]
| _ -> "status" .= "closed"

let tableJObj table =
  jobj [
    "number" .= table.Number
    "waiter" .= table.Waiter
    statusJObj table.Status
  ]

let chefToDoJObj (todo : ChefToDo) =
  jobj [
    "tabId" .= todo.Tab.Id.ToString()
    "tableNumber" .= todo.Tab.TableNumber
    "foodItems" .= foodItemsJArray todo.FoodItems
  ]

let waiterToDoJObj todo =
  jobj [
    "tabId" .= todo.Tab.Id.ToString()
    "tableNumber" .= todo.Tab.TableNumber
    "foodItems" .= foodItemsJArray todo.FoodItems
    "drinksItems" .= drinksItemsJArray todo.DrinksItems
  ]

let cashierToDoJObj (payment : Payment) =
  jobj [
    "tabId" .= payment.Tab.Id.ToString()
    "tableNumber" .= payment.Tab.TableNumber
    "paymentAmount" .= payment.Amount
  ]

let private jsonHeaderWebPart =
  Writers.setMimeType "application/json; charset=utf-8"

let JSON jsonString = OK jsonString >=> jsonHeaderWebPart

let private toErrorJson (msg : string) =
  jobj ["error" .= msg] |> string

let toRequestErrorJson msg =
  msg |> toErrorJson |> BAD_REQUEST >=> jsonHeaderWebPart

let toInternalErrorJson msg =
  msg |> toErrorJson |> INTERNAL_ERROR >=> jsonHeaderWebPart

let toStateJson state =
  state |> stateJObj |> string |> JSON

let toReadModelsJson map key models =
  models
  |> List.map map |> jArray
  |> (.=) key |> List.singleton |> jobj
  |> string |> JSON

let toEventsJson = toReadModelsJson eventJObj "events"
let toTablesJson = toReadModelsJson tableJObj "tables"
let toChefToDosJson = toReadModelsJson chefToDoJObj "todo"
let toWaiterToDosJson = toReadModelsJson waiterToDoJObj "todo"
let toCashierToDosJson = toReadModelsJson cashierToDoJObj "todo"