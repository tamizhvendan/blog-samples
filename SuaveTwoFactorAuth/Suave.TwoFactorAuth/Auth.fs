module Suave.TwoFactorAuth.Auth

open OtpSharp
open System.Collections.Generic

type TwoFactorAuthentication =
| Enabled of SecretKey:string
| Disabled

type User = {
  Username : string
  Password : string
  TwoFactorAuthentication : TwoFactorAuthentication
}

let users = new Dictionary<string, User>()
users.Add("foo", {Username = "foo"; Password = "bar"; TwoFactorAuthentication = Disabled})
users.Add("bar", {Username = "bar"; Password = "foo"; TwoFactorAuthentication = Disabled})
let getUser username = 
  match users.TryGetValue username with
  | true, user -> Some user
  | _ -> None
let updateUserTwoFactorAuth username key =
  match getUser username with
  | Some user ->
    users.[username] <- {user with TwoFactorAuthentication = Enabled key}
  | _ -> ()

let disableTwoFactorAuth username =
  match getUser username with
  | Some user ->
    users.[username] <- {user with TwoFactorAuthentication = Disabled}
  | _ -> ()