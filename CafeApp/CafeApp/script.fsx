#r "packages/Chessie/lib/net40/Chessie.dll"
#load "Domain.fs"
#load "Commands.fs"
#load "Events.fs"
#load "Aggregates.fs"
#load "Handlers.fs"


open Domain
open Events
open Aggregates
open Handlers
open Commands
open System
open Chessie.ErrorHandling

let openTab = OpenTab {
    Id = Guid.NewGuid()
    TableNumber = 1
    Waiter = "John"
}
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
let order = {
  Items = [coke;salad]
  Id = Guid.NewGuid()
}
let placeOrder = PlaceOrder order

let x = evolve ClosedTab openTab
let x',_ =  returnOrFail x
let y = evolve x' placeOrder
let z',_ = returnOrFail y
