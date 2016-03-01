module ReadModel
open Chessie.ErrorHandling
open Domain
open Events
open Data



let projectReadModel e =
  match e with
  | TabOpened tab ->
      updateTableStatus tab.TableNumber (Open tab.Id)
  | OrderPlaced order ->
      addChefToDo order.TabId order.FoodItems
      addWaiterToDo order.TabId order.DrinksItems
      addCashierToDo order.TabId (orderAmount order)
  | DrinksServed (item,tabId) ->
      removeDrinksFromWaiterToDo item tabId
  | FoodPrepared (item, tabId) ->
      removeFoodFromChefToDo item tabId
      addFoodToWaiterToDo item tabId
  | FoodServed (item, tabId) ->
      removeFoodFromWaiterToDo item tabId
  | TabClosed payment ->
      updateTableStatus payment.Tab.TableNumber Closed
      removeCashierToDo payment.Tab.Id
      removeChefToDo payment.Tab.Id
      removeWaiterToDo payment.Tab.Id

let dispatchEvent e =
  projectReadModel (snd e)
  e |> ok