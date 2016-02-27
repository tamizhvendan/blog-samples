module CommandRequests
open System
open Domain
open Commands
open FSharp.Data

[<Literal>]
let OpenTabJson = """{
  "openTab" : {
    "tableNumber" : 1
  }
}"""
type OpenTabReq = JsonProvider<OpenTabJson>

let (|OpenTabRequest|_|) payload =
  try
    let req = OpenTabReq.Parse(payload).OpenTab
    { Id = Guid.NewGuid(); TableNumber = req.TableNumber}
    |> Some
  with
  | ex -> None

[<Literal>]
let PlaceOrderJson = """{
  "placeOrder" : {
    "tabId" : "2a964d85-f503-40a1-8014-2c8ee5ac4a49",
    "foods" : [8,9],
    "drinks" : [10,11]
  }
}"""
type PlaceOrderReq = JsonProvider<PlaceOrderJson>

let (|PlaceOrderRequest|_|) payload =
  try
    let req = PlaceOrderReq.Parse(payload).PlaceOrder
    req |> Some
  with
  | ex -> None