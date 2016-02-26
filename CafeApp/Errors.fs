module Errors
open Domain

type Error =
  | TabAlreadyOpened
  | CanNotOrderWithClosedTab
  | OrderAlreadyPlaced
  | ServingNonOrderedDrinks of DrinksItem  * DrinksItem list
  | ServingNonOrderedFood of FoodItem  * FoodItem list
  | CanNotPrepareNotOrderedFoods of FoodItem  * FoodItem list
  | CanNotServeNonPreparedFood
  | FoodAlreadyPrepared
  | CanNotServeForNonPlacedOrder
  | CanNotServeWithClosedTabCanNotPrepareWithClosedTab
  | CanNotPrepareForNonPlacedOrder
  | CanNotPrepareWithClosedTab
  | CanNotServeWithClosedTab
  | OrderAlreadyServed
  | InvalidPayment of Payment * decimal
  | CanNotPayForNonServedOrder