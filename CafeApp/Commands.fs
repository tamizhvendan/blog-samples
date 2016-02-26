module Commands
open Domain

type Command =
  | OpenTab
  | PlaceOrder of Order
  | ServeDrinks of DrinksItem
  | CloseTab of Payment