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


let commandHandler eventStore (request : HttpRequest) =
  match Encoding.UTF8.GetString request.rawForm with
  | OpenTabRequest tab -> handleOpenTab eventStore tab
  | PlaceOrderRequest placeOrder -> handlePlaceOrder eventStore placeOrder
  | _ -> BAD_REQUEST "Invalid Command Payload"

let api eventStore =
  choose [
    POST >=> path "/command" >=> request (commandHandler eventStore)
    GET >=> path "/tables" >=> getTables
    GET >=> path "/cheftodos" >=> getChefTodos
  ]