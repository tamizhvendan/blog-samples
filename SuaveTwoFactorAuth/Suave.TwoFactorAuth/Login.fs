module Suave.TwoFactorAuth.Login


open Suave
open Suave.Redirection
open Suave.Authentication
open Suave.Cookie
open Suave.Operators
open Suave.DotLiquid
open Suave.Filters

open Combinators
open Auth

let loginPath = "/login"
let userSessionKey = "loggedUser"
let authCodeSessionKey = "loginUser"
let redirectToLogin = function
  | Some errMsg -> FOUND (sprintf "%s?err=%s" loginPath errMsg)
  | None -> FOUND loginPath

let loginSucess failureW redirectPath username =
  authenticated Cookie.CookieLife.Session false      
    >=> sessionSet failureW userSessionKey username
    >=> Redirection.FOUND redirectPath

let onLogin authCodePath ctx = async {
  match ctx.request.["Username"], ctx.request.["Password"] with
  | Some username, Some password -> 
    match getUser username with
    | Some user -> 
      match user.Password = password, user.TwoFactorAuthentication with
      | true, Disabled -> return! loginSucess never "/profile" username ctx
      | true, Enabled _ -> 
        return! (sessionSet never authCodeSessionKey username  
                  >=> FOUND authCodePath) ctx
      | _ -> return! redirectToLogin (Some "Password didn't match") ctx
    | _ -> return! redirectToLogin (Some "Invalid username") ctx   
  | _ -> return! redirectToLogin (Some "Invalid request") ctx
}

let renderLoginView (request : HttpRequest) =
  let errMsg =
    match request.["err"] with
    | Some msg -> msg
    | _ -> ""
  page "login.liquid" errMsg

let secured webpart = 
  let onFail = redirectToLogin (Some "sign-in to access")
  sessionGet onFail userSessionKey webpart
  
let loginWebPart authCodePath =
  path loginPath >=> choose [
      GET >=> request renderLoginView
      POST >=> onLogin authCodePath]