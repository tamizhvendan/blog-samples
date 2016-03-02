module CommandApiHandlers

open Domain
open Data
open Suave.RequestErrors
open Suave.ServerErrors
open Suave.Successful
open Aggregates
open ReadModel
open Handlers
open EventsStore
open Commands
open Chessie.ErrorHandling
open CommandValidations
open JsonResponse

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
    | Ok((state, event),_) -> state |> toStateJson
    | Bad err -> err.Head |> Errors.toErrorString |> toRequestErrorJson
  | Bad _ ->
    "Unable to retrieve events from event store" |> toInternalErrorJson

let handleOpenTab eventStore tab  =
  let table = getTableByNumber tab.TableNumber
  match validateOpenTab table tab with
  | Choice1Of2(_) -> handleCommand eventStore (OpenTab tab)
  | Choice2Of2 err -> toRequestErrorJson err

let handlePlaceOrder
    eventStore (tabId, drinksMenuNumbers, foodMenuNumbers) =

  let foodItems = getFoodItems foodMenuNumbers
  let drinksItems = getDrinksItems drinksMenuNumbers
  let table = getTableByTabId tabId
  match validatePlaceOrder table drinksItems foodItems with
  | Choice1Of2 (drinks,foods) ->
    {
      TabId = tabId
      FoodItems = foods
      DrinksItems = drinks
    }
    |> PlaceOrder
    |> handleCommand eventStore
  | Choice2Of2 err -> toRequestErrorJson err

let handleItem eventStore getItemByMenuNumber msg (tabId, menuNumber) cmd =
    let table = getTableByTabId tabId
    let item = getItemByMenuNumber menuNumber
    match validateItem table item msg with
    | Choice1Of2 food ->
      (food,tabId)
      |> cmd
      |> handleCommand eventStore
    | Choice2Of2 err -> toRequestErrorJson err

let handleCloseTab eventStore (tabId, amount) =
  let table = getTableByTabId tabId
  match validateCloseTab table with
  | Choice1Of2 (tableNumber,tabId) ->
    {Amount = amount; Tab = {Id = tabId; TableNumber = tableNumber}}
    |> CloseTab
    |> handleCommand eventStore
  | Choice2Of2 err -> toRequestErrorJson err