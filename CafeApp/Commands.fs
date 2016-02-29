module Commands
open Domain
open System

type Command =
  | OpenTab of Tab
  | PlaceOrder of Order
  | ServeDrinks of DrinksItem * Guid
  | PrepareFood of FoodItem * Guid
  | ServeFood of FoodItem * Guid
  | CloseTab of Payment