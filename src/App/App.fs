open Browser
open Sutil
open Sutil.Router
open Sutil.CoreElements

let router = Router.createRouter()

let Link = Router.Link router [Attr.className "your custom attributes to apply to each link"]
let navigate = Router.navigate router

module Pages =
  let indexHandler() =
    fragment [
      Link "/" [text "Home"]; Html.br []
      Link "/about" [text "About"]; Html.br []
      Link "/blog/1" [text "blog 1"]; Html.br []
      Link "/blog/2" [text "blog 2"]; Html.br []
    ]

  let aboutHandler() =
    Html.div [
      Link "/" [text "Home"]; Html.br []
      text "This is a fork of "
      Html.a [Attr.href "https://github.com/sheridanchris/Sutil.Router"; text "Sutil.Router"]
      text " by "
      Html.a [Attr.href "https://github.com/sheridanchris/"; text "this awesome developer"]
    ]

  let blogHandler(blogid: int) =
    sprintf "This is a blog with an id of %i" blogid |> text

  let handle404() =
    text "Welp. What you were looking for isn't here. Unless you were looking for this message"

let getHandlerFromUrl (url: string list) =
  match url with
  | [] -> Pages.indexHandler()
  | [ "blog"; Route.Int blogId ] -> Pages.blogHandler(blogId)
  | [ "about" ] -> Pages.aboutHandler()
  | _ -> Pages.handle404()

let app() =
  Router.renderRouter router getHandlerFromUrl

app() |> Program.mount
