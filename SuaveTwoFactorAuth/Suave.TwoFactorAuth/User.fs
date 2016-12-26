module Suave.TwoFactorAuth.User

open System.Collections.Generic


type TwoFactorAuthentication =
| Enabled of SecretKey:string
| Disabled

type User = {
  Username : string
  Password : string
  TwoFactorAuthentication : TwoFactorAuthentication
}

let private users = new Dictionary<string, User>()
users.Add("foo", {Username = "foo"; Password = "bar"; TwoFactorAuthentication = Disabled})
let getUser username = 
  match users.TryGetValue username with
  | true, user -> Some user
  | _ -> None
let enableTwoFactorAuth username key =
  match getUser username with
  | Some user ->
    users.[username] <- {user with TwoFactorAuthentication = Enabled key}
  | _ -> ()

let disableTwoFactorAuth username =
  match getUser username with
  | Some user ->
    users.[username] <- {user with TwoFactorAuthentication = Disabled}
  | _ -> ()

