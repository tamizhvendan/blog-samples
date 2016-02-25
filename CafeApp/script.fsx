#r "packages/Chessie/lib/net40/Chessie.dll"
#load "Domain.fs"
#load "Commands.fs"
#load "Events.fs"
#load "Aggregates.fs"
#load "Errors.fs"
#load "Handlers.fs"

open Domain
open Events
open Aggregates
open Errors
open Handlers
open Commands
open System
open Chessie.ErrorHandling


let coke = Drinks {
    MenuNumber = 10
    Price = 2.5m
    Description = "Coke"
}
let salad = Food {
  MenuNumber = 8
  Price = 5m
  Description = "Salad"
}
let lemonade = Drinks {
  MenuNumber = 11
  Description = "Lemonade"
  Price = 1.5m
}
let order = {
  Items = [coke;salad]
  Id = Guid.NewGuid()
}
let serveCoke = ServeItem coke
let serveSalad = ServeItem salad
let serveLemonade = ServeItem lemonade
let placeOrder = PlaceOrder order
let closeTab = Payment(7.5m) |> CloseTab
let commands = [OpenTab;placeOrder;serveCoke;serveSalad;closeTab]

let lift f m cmd =
  match m with
  | Ok((state,event),_) ->
    f state cmd
  | Bad errors -> errors |> List.head |> fail

let result initalState commands =
  let x, xs =
    match commands with
    | [] -> OpenTab, []
    | x::xs -> x,xs
  let stateM = evolve initalState x
  xs |> List.fold (lift evolve) stateM



result ClosedTab commands