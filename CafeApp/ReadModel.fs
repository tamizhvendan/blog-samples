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
      {
        ChefToDo.TabId = order.TabId
        FoodItems = order.FoodItems
      } |> addChefToDo
      {
        TabId = order.TabId
        DrinksItem = order.DrinksItems
        FoodItems = []
      } |> addWaiterToDo
  | DrinksServed (item,tabId) ->
      removeDrinksFromWaiterToDo item tabId
  | FoodPrepared (item, tabId) ->
      removeFoodFromChefToDo item tabId
      addFoodToWaiterToDo item tabId
  | TabClosed payment ->
      updateTableStatus payment.Tab.TableNumber Closed
  | _ -> ()



let dispatchEvent e =
  projectReadModel (snd e)
  e |> ok