module CommandApiHandlers

open Domain
open Data
open Suave.RequestErrors
open Suave.ServerErrors
open CommandRequests
open Suave.Successful
open Aggregates
open ReadModel
open Handlers
open EventsStore
open Commands
open Chessie.ErrorHandling
open CommandValidations

let getTabIdFromCommand = function
| OpenTab tab -> tab.Id
| PlaceOrder order -> order.TabId
| ServeDrinks (item, tabId) -> tabId
| PrepareFood (item, tabId) -> tabId
| ServeFood (item, tabId) -> tabId
| CloseTab payment -> payment.Tab.Id

let handleCommand eventStore command =
  match eventStore.GetState (getTabIdFromCommand command) with
  | Ok(state, _) ->
    let result =
      evolve state command
      >>= eventStore.SaveEvent
      >>= dispatchEvent
    match result with
    | Ok((state, event),_) ->
      OK <| sprintf "State : %A, Event : %A" state event
    | Bad err -> BAD_REQUEST <| sprintf "%A" err
  | Bad _ -> INTERNAL_ERROR "Unable to retrieve events from event store"


let handleOpenTab eventStore tab  =
  match validateOpenTab tab with
  | Choice1Of2(_) -> handleCommand eventStore (OpenTab tab)
  | Choice2Of2 err -> BAD_REQUEST err

let handlePlaceOrder eventStore (placeOrder : PlaceOrderReq.PlaceOrder) =
  let foodItems = getFoodItems placeOrder.Foods
  let drinksItems = getDrinksItems placeOrder.Drinks
  match validatePlaceOrder placeOrder.TabId drinksItems foodItems with
  | Choice1Of2 order -> handleCommand eventStore (PlaceOrder order)
  | Choice2Of2 err -> BAD_REQUEST err