module Commands
open Domain

type Command =
  | OpenTab
  | PlaceOrder of Order
  | ServeItem of OrderItem
  | CloseTab of Payment