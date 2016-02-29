module Api
open Suave
open Suave.Successful
open Suave.RequestErrors
open Suave.Operators
open Suave.Filters
open CommandRequests
open Data
open Domain
open System.Text
open CommandApiHandlers
open ReadModelApi
open Commands

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
  | _ -> BAD_REQUEST "Invalid Command Payload"

let api eventStore =
  choose [
    POST >=> path "/command" >=> request (commandHandler eventStore)
    GET >=> path "/tables" >=> getTables
    GET >=> path "/todo/chef" >=> getChefToDos
    GET >=> path "/todo/waiter" >=> getWaiterToDos
  ]