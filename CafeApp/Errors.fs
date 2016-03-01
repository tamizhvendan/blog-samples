module Errors
open Domain

type Error =
  | TabAlreadyOpened
  | CanNotOrderWithClosedTab
  | OrderAlreadyPlaced
  | ServingNonOrderedDrinks of DrinksItem
  | CanNotServeForNonOrderedDrinksItem of DrinksItem
  | ServingNonOrderedFood of FoodItem
  | CanNotPrepareNotOrderedFoods of FoodItem
  | CanNotServeNonPreparedFood of FoodItem
  | FoodAlreadyPrepared of FoodItem
  | CanNotServeForNonOrderedFoodItem of FoodItem
  | CanNotPrepareForNonPlacedOrder of FoodItem
  | CanNotPrepareWithClosedTab
  | CanNotServeWithClosedTab
  | OrderAlreadyServed
  | InvalidPayment of decimal * decimal
  | CanNotPayForNonServedOrder
  | InvalidStateForSavingEvent
  | ErrorWhileSavingEvent of System.Exception
  | ErrorWhileRetrievingEvents of System.Exception

let toErrorString = function
| TabAlreadyOpened -> "Tab Already Opened"
| CanNotOrderWithClosedTab -> "Cannot Order as Tab is Closed"
| OrderAlreadyPlaced -> "Order already placed"
| ServingNonOrderedDrinks (DrinksItem item)  ->
  sprintf "DrinksItem %s(%d) is not ordered" item.Name item.MenuNumber
| CanNotServeForNonOrderedDrinksItem (DrinksItem item) ->
  sprintf "DrinksItem %s(%d) is not ordered" item.Name item.MenuNumber
| ServingNonOrderedFood (FoodItem item) ->
  sprintf "FoodItem %s(%d) is not ordered" item.Name item.MenuNumber
| CanNotPrepareNotOrderedFoods (FoodItem item) ->
  sprintf "FoodItem %s(%d) is not ordered" item.Name item.MenuNumber
| CanNotServeNonPreparedFood (FoodItem item) ->
  sprintf "FoodItem %s(%d) is not prepared yet" item.Name item.MenuNumber
| FoodAlreadyPrepared (FoodItem item) ->
  sprintf "FoodItem %s(%d) is already prepared" item.Name item.MenuNumber
| CanNotServeForNonOrderedFoodItem (FoodItem item) ->
  sprintf "FoodItem %s(%d) is not ordered" item.Name item.MenuNumber
| CanNotServeWithClosedTab -> "Cannot Serve as Tab is Closed"
| CanNotPrepareWithClosedTab -> "Cannot Prepare as Tab is Closed"
| CanNotPrepareForNonPlacedOrder (FoodItem item) ->
  sprintf "FoodItem %s(%d) is not ordered" item.Name item.MenuNumber
| OrderAlreadyServed -> "Order Already Served"
| InvalidPayment (expected, actual) ->
  sprintf "Invalid Payment. Expected is %f but paid %f" expected actual
| CanNotPayForNonServedOrder -> "Can not pay for non served order"
| InvalidStateForSavingEvent ->
  "Unable save the event as current state is invalid"
| ErrorWhileSavingEvent ex ->
  sprintf "Error while saving event : %s" ex.Message
| ErrorWhileRetrievingEvents ex ->
  sprintf "Error while fetching events : %s" ex.Message