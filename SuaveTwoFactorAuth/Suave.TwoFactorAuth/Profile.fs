module Suave.TwoFactorAuth.Profile

open Suave.DotLiquid
open Suave.Redirection
open Suave.Filters
open Suave.Operators

open Auth
open Login
let profilePath = "/profile"

type ProfileViewModel = {
  IsTwoFactorAuthEnabled : bool
  Username: string
  SecretKey : string
}
with static member FromUser user =
        let isTwoFactorAuthEnabled, secretKey = 
          match user.TwoFactorAuthentication with
          | Enabled secretKey -> true, secretKey
          | _ -> false, ""
          
        {
          IsTwoFactorAuthEnabled = isTwoFactorAuthEnabled
          SecretKey = secretKey
          Username = user.Username
        }

let renderProfile notFoundPath username =
  match getUser username with
  | Some user -> 
    user    
    |> ProfileViewModel.FromUser
    |> page "profile.liquid"
  | _ -> redirect notFoundPath

let profileWebPart notFoundPath = path profilePath >=> secured (renderProfile notFoundPath)
