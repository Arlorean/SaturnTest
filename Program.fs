open Saturn
open Giraffe

module ApiRoutes =
    let findByCountry country =
        match Api.countryLookup.TryFind country with
        | Some stats -> json stats
        | None -> RequestErrors.NOT_FOUND (sprintf "Unknown country %s" country)

    let apiRouter = router {
        get "/api/countries" (Api.allCountries |> json)
        getf "/api/countries/%s" findByCountry 
    }

module UiRoutes =
    let uiRouter = text "Hello World"

let appRouter = router {
    forward "/api" ApiRoutes.apiRouter
    forward "" UiRoutes.uiRouter
}

let myApplication = application {
    use_json_serializer (Thoth.Json.Giraffe.ThothSerializer())
    use_router appRouter
}

run myApplication

(*
    http://localhost:5000/
    http://localhost:5000/api/countries
    http://localhost:5000/api/countries/China
    http://localhost:5000/api/countries/UnknownCountry
*)