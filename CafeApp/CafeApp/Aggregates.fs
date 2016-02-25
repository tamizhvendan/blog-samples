module Aggregates
open Events

type State =
  | ClosedTab
  | OpenedTab of OpenTab

type InvalidState =
  | TabAlreadyOpened
