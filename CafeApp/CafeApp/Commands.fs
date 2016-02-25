module Commands
open Domain

type Command =
  | OpenTab of Tab
  | PlaceOrder of Order
