module Errors
open Domain

type Error =
  | TabAlreadyOpened
  | CanNotOrderWithClosedTab
  | OrderAlreadyPlaced
  | ServingNonOrderedItem of OrderItem  * OrderItem list
  | ServingNonOrderedDrinks of DrinksItem  * DrinksItem list
  | CanNotServeForNonPlacedOrder
  | CanNotServeWithClosedTab
  | OrderAlreadyServed
  | InvalidPayment of Payment * decimal
  | CanNotPayForNonServedOrder