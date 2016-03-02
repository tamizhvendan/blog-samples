module Api
open Suave
open Suave.Successful
open Suave.RequestErrors
open Suave.ServerErrors
open Suave.Operators
open Suave.Filters
open CommandRequests
open Data
open Domain
open System.Text
open CommandApiHandlers
open ReadModelApi
open Commands
open Chessie.ErrorHandling
open EventsStore
open JsonResponse
open Errors

let commandHandler eventStore (request : HttpRequest) =

  let handleFoodItem (tabId, menuNumber) =
    let msg = sprintf "Invalid Food Menu Number %d" menuNumber
    handleItem eventStore getFoodByMenuNumber msg (tabId, menuNumber)

  let handleDrinksItem (tabId, menuNumber) =
    let msg = sprintf "Invalid Drinks Menu Number %d" menuNumber
    handleItem eventStore getDrinksByMenuNumber msg (tabId, menuNumber)

  match Encoding.UTF8.GetString request.rawForm with
  | OpenTabRequest tab -> handleOpenTab eventStore tab
  | PlaceOrderRequest placeOrder -> handlePlaceOrder eventStore placeOrder
  | ServeDrinksRequest serveDrinks -> handleDrinksItem serveDrinks ServeDrinks
  | PrepareFoodRequest prepareFood -> handleFoodItem prepareFood PrepareFood
  | ServeFoodRequest serveFood -> handleFoodItem serveFood ServeFood
  | CloseTabRequest closeTab -> handleCloseTab eventStore closeTab
  | _ -> toRequestErrorJson "Invalid Command Payload"

let getState eventStore tabId =
  match System.Guid.TryParse(tabId) with
  | true, tabId ->
      match eventStore.GetState tabId with
      | Ok(state,_) -> toStateJson state
      | Bad(err) -> err.Head |> toErrorString |> toInternalErrorJson
  | _ -> toRequestErrorJson "Invalid Tab Id"

let getEvents eventStore tabId =
  match System.Guid.TryParse(tabId) with
  | true, tabId ->
      match eventStore.GetEvents tabId with
      | Ok(events,_) -> toEventsJson events
      | Bad(err) -> err.Head |> toErrorString |> toInternalErrorJson
  | _ -> toRequestErrorJson "Invalid Tab Id"

let api eventStore =
  choose [
    POST >=> path "/command" >=> request (commandHandler eventStore)
    GET >=> path "/tables" >=> getTables
    GET >=> path "/todo/chef" >=> getChefToDos
    GET >=> path "/todo/waiter" >=> getWaiterToDos
    GET >=> path "/todo/cashier" >=> getCashierToDos
    GET >=> pathScan "/state/%s" (getState eventStore)
    GET >=> pathScan "/events/%s" (getEvents eventStore)
  ]