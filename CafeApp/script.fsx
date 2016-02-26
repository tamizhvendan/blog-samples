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


let coke = DrinksItem {
    MenuNumber = 10
    Price = 2.5m
    Description = "Coke"
}
let salad = FoodItem {
  MenuNumber = 8
  Price = 5m
  Description = "Salad"
}
let lemonade = DrinksItem {
  MenuNumber = 11
  Description = "Lemonade"
  Price = 1.5m
}
let order = {
  Items = [Drinks(coke)]
  Id = Guid.NewGuid()
}
let serveCoke = ServeDrinks coke
//let serveSalad = ServeItem salad
let serveLemonade = ServeDrinks lemonade
let placeOrder = PlaceOrder order
let closeTab = Payment(2.5m) |> CloseTab
let commands = [OpenTab;placeOrder;serveCoke;closeTab]

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