module Commands
open Domain

type Command =
  | OpenTab of Tab
  | PlaceOrder of PlacedOrder
  | ServeDrinks of DrinksItem
  | PrepareFood of FoodItem
  | ServeFood of FoodItem
  | CloseTab of Payment