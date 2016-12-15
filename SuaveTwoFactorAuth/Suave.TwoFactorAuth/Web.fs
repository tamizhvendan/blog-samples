module Suave.TwoFactorAuth.Web

open Suave.Successful
open Suave.Web
open Suave
open Suave.Http
open Suave.Filters
open Suave.Operators
open Suave.DotLiquid
open Suave.State.CookieStateStore
open Suave.Cookie
open Suave.Authentication
open Suave.TwoFactorAuth.Auth
open OtpSharp
open Base32

let sessionSet failureF key value = 
  statefulForSession
  >=> context (fun ctx ->
                match HttpContext.state ctx with
                | Some state -> state.set key value
                | _ -> printfn "failed to get the state"; failureF
              )

let sessionGet failureF key successF = 
  statefulForSession 
  >=> context (fun ctx ->
                match HttpContext.state ctx with
                | Some store -> 
                  match store.get key with
                  | Some username -> successF username
                  | _ -> failureF
                | _ -> failureF
  )
let (|*>) x f =
  printfn "%A" x
  f x
let redirectToLogin errMsg = Redirection.FOUND (sprintf "/login?err=%s" errMsg)
let redirectToNotFound = Redirection.redirect "/notfound"

let loginSucess username =
  authenticated Cookie.CookieLife.Session false      
    >=> sessionSet never "loggedUser" username
    >=> Redirection.FOUND "/profile" 

let renderAuthCodeVerifcationView =
  page "auth_code.html" ""

let onLogin ctx = async {
  match ctx.request.["Username"], ctx.request.["Password"] with
  | Some username, Some password -> 
    match getUser username with
    | Some user -> 
      match user.Password = password, user.TwoFactorAuthentication with
      | true, Disabled -> return! loginSucess username ctx
      | true, Enabled _ -> 
        return! (sessionSet never "loginUser" username  
                  >=> renderAuthCodeVerifcationView) ctx
      | _ -> return! redirectToLogin "Password didn't match" ctx
    | _ -> return! redirectToLogin "Invalid username" ctx   
  | _ -> return! redirectToLogin "Invalid request" ctx
}
  
  
let onLogout = 
  unsetPair SessionAuthCookie
    >=> unsetPair StateCookie
    >=> redirectToLogin ""

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

let renderProfile username =
  match getUser username with
  | Some user -> 
    user    
    |> ProfileViewModel.FromUser
    |> page "profile.html"
  | _ -> redirectToNotFound

type EnableTwoFactorViewModel = {
  Key : string
  Url : string
  Err : string
}
with static member To username err =
      let secretKey = KeyGeneration.GenerateRandomKey(20)
      let appName = "SuaveRocks"
      let label = sprintf "%s (%s.com/%s)" appName appName username
      let keyUrl = KeyUrl.GetTotpUrl(secretKey, label)
      { Url = sprintf "https://qrcode.kaywa.com/img.php?s=4&d=%s&issuer=%s" keyUrl appName 
        Key = Base32Encoder.Encode(secretKey)
        Err = err}

let renderEnableTwoFactorAuthView username ctx = async {
  match getUser username with
  | Some user -> 
    let err =
      match ctx.request.["err"] with
      | Some err -> err 
      | _ -> ""
    let vm = EnableTwoFactorViewModel.To username err
    return! page "enable_two_factor.html" vm ctx
  | _ -> return! redirectToNotFound ctx
} 

let verifyOtp secretKey code =
  let otp = new Totp(Base32Encoder.Decode secretKey)
  otp.VerifyTotp(code, ref 0L, new VerificationWindow(2, 2))
let enableTwoFactorAuth username ctx = async {
  match ctx.request.["SecretKey"], ctx.request.["Code"] with
  | Some secretKey, Some code ->     
    match verifyOtp secretKey code with
    | true ->
      updateUserTwoFactorAuth username secretKey
      return! Redirection.redirect "/profile" ctx
    | _ -> return! Redirection.redirect "/enable_two_factor?err=code validation failed" ctx
  | _ -> return! redirectToNotFound ctx
}

let onAuthCodeVerification username ctx = async {
  match ctx.request.["Code"], getUser username with
  | Some code, Some user ->
    match user.TwoFactorAuthentication with
    | Enabled secretKey -> 
      match verifyOtp secretKey code with
      | true -> return! loginSucess user.Username ctx
      | _ -> return! redirectToLogin (sprintf "invalid otp") ctx
    | _ -> return! redirectToLogin "invalid request" ctx
  | _ -> return! redirectToLogin "invalid request" ctx
}

let renderLoginView (request : HttpRequest) =
  let errMsg =
    match request.["err"] with
    | Some msg -> msg
    | _ -> ""
  page "login.html" errMsg

let disableTwoFactorAuth username =
  disableTwoFactorAuth username
  Redirection.redirect "/profile"
let app = 
  let secured = sessionGet (redirectToLogin "sign-in to access") "loggedUser"
  choose [
    path "/login" >=> choose [
      GET >=> request renderLoginView
      POST >=> onLogin]
    path "/profile" >=> secured renderProfile 
    path "/logout" >=> onLogout
    path "/notfound" >=> page "not_found.html" ""
    path "/enable_two_factor" >=> choose [
      GET >=> secured renderEnableTwoFactorAuthView
      POST >=> secured enableTwoFactorAuth
    ]
    path "/disable_two_factor" >=> secured disableTwoFactorAuth
    path "/verify_auth_code"  
      >=> sessionGet (redirectToLogin "invalid request") "loginUser" onAuthCodeVerification
  ]


