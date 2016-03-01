module Events
open Domain
open System

type Event =
  | TabOpened of Tab
  | OrderPlaced of Order
  | DrinksServed of DrinksItem * Guid
  | FoodPrepared of FoodItem * Guid
  | FoodServed of FoodItem * Guid
  | TabClosed of Payment