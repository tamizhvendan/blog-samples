module Suave.TwoFactorAuth.GoogleAuthenticator

open Suave
open Suave.Filters
open Suave.Operators
open Suave.DotLiquid
open Suave.Redirection

open Login
open User
open Combinators
open OtpSharp
open Base32

let authCodePath = "/authcode"
let enableTwoFactorAuthPath = "/enable_two_factor"

type EnableTwoFactorViewModel = {
  Key : string
  Url : string
  Err : string
}
with static member From username err =
      let secretKey = KeyGeneration.GenerateRandomKey(20)
      let appName = "SuaveRocks"
      let label = sprintf "%s (%s.com/%s)" appName appName username
      let keyUrl = KeyUrl.GetTotpUrl(secretKey, label)
      { Url = sprintf "https://qrcode.kaywa.com/img.php?s=4&d=%s&issuer=%s" keyUrl appName 
        Key = Base32Encoder.Encode(secretKey)
        Err = err}
let renderEnableTwoFactorAuthView notFoundPath username ctx = async {
  match getUser username with
  | Some user -> 
    let err =
      match ctx.request.["err"] with
      | Some err -> err 
      | _ -> ""
    let vm = EnableTwoFactorViewModel.From username err
    return! page "enable_two_factor.liquid" vm ctx
  | _ -> return! redirect notFoundPath ctx
}
let verifyOtp secretKey code =
  let otp = new Totp(Base32Encoder.Decode secretKey)
  otp.VerifyTotp(code, ref 0L, new VerificationWindow(2, 2))

let enableTwoFactorAuth redirectPath notFoundPath username ctx = async {
  match ctx.request.["SecretKey"], ctx.request.["Code"] with
  | Some secretKey, Some code ->     
    match verifyOtp secretKey code with
    | true ->
      enableTwoFactorAuth username secretKey
      return! redirect redirectPath ctx
    | _ -> 
      let redirectTo =
        sprintf "%s?err=code validation failed" enableTwoFactorAuthPath 
      return! redirect redirectTo ctx
  | _ -> return! redirect notFoundPath ctx
}

let disableTwoFactorAuth redirectPath username =
  disableTwoFactorAuth username
  Redirection.redirect redirectPath

let onAuthCodeVerification redirectPath (request : HttpRequest) username = 
  match request.["Code"], getUser username with
  | Some code, Some user ->
    match user.TwoFactorAuthentication with
    | Enabled secretKey -> 
      match verifyOtp secretKey code with
      | true -> loginSucess never redirectPath user.Username
      | _ -> redirectToLogin (Some "invalid otp") 
    | _ -> redirectToLogin (Some "invalid request") 
  | _ -> redirectToLogin (Some "invalid request")

let onVerifyAuthCode redirectPath httpRequest =
  let onFail = redirectToLogin (Some "invalid request")
  let onAuthCodeVerification = 
    onAuthCodeVerification redirectPath httpRequest
  sessionGet onFail authCodeSessionKey onAuthCodeVerification

let googleAuthenticatorWebPart redirectPath notFoundPath = 
  choose [
    path authCodePath >=> page "auth_code.liquid" ""
    path enableTwoFactorAuthPath >=> choose [
      GET >=> secured (renderEnableTwoFactorAuthView notFoundPath)
      POST >=> secured (enableTwoFactorAuth redirectPath notFoundPath)
    ]
    path "/disable_two_factor" >=> secured (disableTwoFactorAuth redirectPath)
    path "/verify_auth_code" >=> request (onVerifyAuthCode redirectPath)]