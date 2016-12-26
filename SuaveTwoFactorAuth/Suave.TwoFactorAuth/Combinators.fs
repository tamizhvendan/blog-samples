module Suave.TwoFactorAuth.Combinators

open Suave.State.CookieStateStore
open Suave.Cookie
open Suave
open Suave.Operators
open Suave.Authentication

let sessionSet failureF key value = 
  statefulForSession
  >=> context (fun ctx ->
                match HttpContext.state ctx with
                | Some state -> state.set key value
                | _ -> failureF
              )

let sessionGet failureF key successF = 
  statefulForSession 
  >=> context (fun ctx ->
                match HttpContext.state ctx with
                | Some store -> 
                  match store.get key with
                  | Some value -> successF value
                  | _ -> failureF
                | _ -> failureF
  )

let clearSession = 
  unsetPair SessionAuthCookie
    >=> unsetPair StateCookie

  