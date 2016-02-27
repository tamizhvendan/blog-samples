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


let commandHandler (request : HttpRequest) =
  match Encoding.UTF8.GetString request.rawForm with
  | OpenTabRequest tab -> handleOpenTab tab
  | PlaceOrderRequest placeOrder -> handlePlaceOrder placeOrder
  | _ -> BAD_REQUEST "Invalid Command Payload"

let api =
  choose [
    POST >=> path "/command" >=> request commandHandler
  ]