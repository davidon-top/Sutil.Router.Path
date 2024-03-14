namespace Sutil.Router

open System
open Sutil
open Browser
open Browser.Types

module internal Common =
  let split (splitBy: string) (value: string) =
    value.Split([| splitBy |], StringSplitOptions.RemoveEmptyEntries)
    |> Array.toList

  let splitRoute = split "/"
  let splitQueryParam = split "&"
  let splitKeyValuePair = split "="

  let getKeyValuePair (parts: 'a list) =
    if List.length parts = 2 then
      Some(parts[0], parts[1])
    else
      None

[<RequireQualifiedAccess>]
module Router =
  open Common

  type RouterLocation =
    struct
      val pathname: string
      val query: string
      new(location: Location) = { pathname = location.pathname; query = location.search }
    end

  let getCurrentUrl (router: RouterLocation) =
    let route =
      router.pathname.TrimStart('/').TrimEnd('/')

    if not (router.query = "") then
      splitRoute route @ [ router.query.TrimStart('?') ]
    else
      splitRoute route

  let navigate (pathStore: Store<RouterLocation>) (url: string)  =
    window.history.pushState((), "", url)
    pathStore |> Store.modify (fun _ -> new RouterLocation(window.location))

  let Link (pathStore: Store<RouterLocation>) (defaultApply: seq<Core.SutilElement>) (href: string) (apply: seq<Core.SutilElement>) =
    Html.a (defaultApply |> Seq.append apply |> Seq.append [EngineHelpers.Attr.href href; CoreElements.onClick (fun e ->
      e.preventDefault()
      navigate pathStore href
    ) []])

  let createRouter() =
    Store.make(new RouterLocation(window.location))

  let renderRouter (router: Store<RouterLocation>) (conf: string list -> Core.SutilElement) =
    Bind.el(router, (fun location ->
      getCurrentUrl location |> conf
    ))

// Credits to Feliz.Router for these amazing active pattern definitions.
// https://github.com/Zaid-Ajaj/Feliz.Router/blob/master/src/Router.fs#L1430
module Route =
  open Common

  let (|Int|_|) (value: string) =
    match Int32.TryParse value with
    | true, value -> Some value
    | _ -> None

  let (|Int64|_|) (input: string) =
    match Int64.TryParse input with
    | true, value -> Some value
    | _ -> None

  let (|Guid|_|) (input: string) =
    match Guid.TryParse input with
    | true, value -> Some value
    | _ -> None

  let (|Number|_|) (input: string) =
    match Double.TryParse input with
    | true, value -> Some value
    | _ -> None

  let (|Decimal|_|) (input: string) =
    match Decimal.TryParse input with
    | true, value -> Some value
    | _ -> None

  let (|Bool|_|) (input: string) =
    match input.ToLower() with
    | ("1" | "true") -> Some true
    | ("0" | "false") -> Some false
    | "" -> Some true
    | _ -> None

  let (|Query|_|) (input: string) =
    match splitQueryParam input with
    | [] -> None
    | queryParams -> queryParams |> List.choose (splitKeyValuePair >> getKeyValuePair) |> Some
