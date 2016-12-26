module Suave.TwoFactorAuth.Login


open Suave
open Suave.Redirection
open Suave.Authentication
open Suave.Cookie
open Suave.Operators
open Suave.DotLiquid
open Suave.Filters

open Combinators
open User

let loginPath = "/login"
let userSessionKey = "loggedUser"
let authCodeSessionKey = "loginUser"
let redirectToLogin = function
  | Some errMsg -> FOUND (sprintf "%s?err=%s" loginPath errMsg)
  | None -> FOUND loginPath

let loginSucess failureW redirectPath username =
  authenticated Cookie.CookieLife.Session false      
    >=> sessionSet failureW userSessionKey username
    >=> FOUND redirectPath

let onLogin redirectPath authCodePath (request : HttpRequest) = 
  match request.["Username"], request.["Password"] with
  | Some username, Some password -> 
    match getUser username with
    | Some user -> 
      match user.Password = password, user.TwoFactorAuthentication with
      | true, Disabled -> loginSucess never redirectPath username
      | true, Enabled _ -> 
          sessionSet never authCodeSessionKey username  
            >=> FOUND authCodePath
      | _ -> redirectToLogin (Some "Password didn't match")
    | _ -> redirectToLogin (Some "Invalid username")   
  | _ -> redirectToLogin (Some "Invalid request")

let renderLoginView (request : HttpRequest) =
  let errMsg =
    match request.["err"] with
    | Some msg -> msg
    | _ -> ""
  page "login.liquid" errMsg

let secured webpart = 
  let onFail = redirectToLogin (Some "sign-in to access")
  sessionGet onFail userSessionKey webpart
  
let loginWebPart redirectPath authCodePath =
  path loginPath >=> choose [
      GET >=> request renderLoginView
      POST >=> request (onLogin redirectPath authCodePath)]