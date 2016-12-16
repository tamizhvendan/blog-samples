module Suave.TwoFactorAuth.Web

open Suave
open Suave.Filters
open Suave.Operators
open Suave.DotLiquid

open Combinators
open Login
open GoogleAuthenticator
open Profile
let notFoundPath = "/notfound"

let app =   
  choose [
    loginWebPart authCodePath
    googleAuthenticatorWebPart profilePath notFoundPath
    profileWebPart notFoundPath
    path "/logout" >=> clearSession >=> redirectToLogin None
    path notFoundPath >=> page "not_found.liquid" ""    
  ]



