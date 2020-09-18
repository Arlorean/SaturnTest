module Api

open FSharp.Data
open System
type Daily = CsvProvider<"COVID-19/csse_covid_19_data/csse_covid_19_daily_reports/05-01-2020.csv">

let files = 
    System.IO.Directory.GetFiles("COVID-19/csse_covid_19_data/csse_covid_19_daily_reports", "*.csv")
    |> Seq.filter (fun filename -> System.IO.Path.GetFileNameWithoutExtension(filename).StartsWith("05"))
    |> Seq.map System.IO.Path.GetFullPath

let allData = 
    files 
    |> Seq.map Daily.Load
    |> Seq.collect (fun data -> data.Rows)
    |> Seq.distinctBy (fun row -> row.Country_Region, row.Province_State, row.Last_Update.Date)
    |> Seq.sortBy (fun row -> row.Last_Update.Date)
    |> Seq.filter (fun row -> row.Country_Region <> "Others")
    |> Seq.toArray

let cleanseCountry country = 
    match country with
    | "Unite States" -> "US"
    | _ -> country

let confirmedbyCountryDaily =
    [| for country, rows in allData |> Seq.groupBy (fun r -> cleanseCountry r.Country_Region) do
        let countryData = 
            [| for date, rows in rows |> Seq.groupBy (fun r -> r.Last_Update.Date) do
                {| Date = date
                   Confirmed = rows |> Seq.sumBy (fun r-> r.Confirmed) 
                   Deaths = rows |> Seq.sumBy (fun r-> r.Deaths) 
                   Recovered = rows |> Seq.sumBy (fun r-> r.Recovered) |}
            |]
        country, countryData
    |]

let countryLookup = confirmedbyCountryDaily |> Map

let allCountries = confirmedbyCountryDaily |> Array.map fst

let countryStats = [|
    for country, stats in confirmedbyCountryDaily do
        let mostRecent = stats |> Array.tryLast
        match mostRecent with
        | Some mostRecent -> {| mostRecent with Country=country |}
        | None -> ()
|]