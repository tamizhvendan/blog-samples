module Aggregates
open Events

type State =
  | ClosedTab
  | OpenedTab of OpenTab
  | PlacedOrder of PlaceOrder

type Error =
  | TabAlreadyOpened
  | CanNotOrderWithClosedTab
  | OrderAlreadyPlaced
