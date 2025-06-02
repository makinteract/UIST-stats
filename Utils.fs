module Utils

// #r "nuget: FSharp.Data"

open System
open System.IO
open FSharp.Data


type ID = ID of int32
type Title = Title of string

type Subcommittee =
    | SplitA
    | SplitB

type Decision =
    | QR
    | InReview

type Score = Score of float


type Submission =
    { ID: ID
      Title: Title
      Subcommittee: Subcommittee
      Decision: Decision
      ReviewsTotal: int
      ReviewsDone: int
      ReviewsLeft: int
      ReviewsTentative: int
      CommitteeScore: Score option
      ReviewerScore: Score option
      OverallScore: Score option
      Pname: string option
      Pscore: Score option
      S1name: string option
      S1score: Score option
      S2name: string option
      S2score: Score option
      S3name: string option
      S3score: Score option
      E1name: string option
      E1score: Score option
      E2name: string option
      E2score: Score option
      E3name: string option
      E3score: Score option }


let private transformDecision (decision: string) =
    match decision with
    | "R-Quick" -> QR
    | _ -> InReview

let private tranformSubcommittee (subcommittee: string) =
    match subcommittee with
    | "Split A" -> SplitA
    | "Split B" -> SplitB
    | _ -> failwithf "Unknown subcommittee: %s" subcommittee


let private getScore (score: string) =
    match Double.TryParse score with
    | true, v -> Some(Score v)
    | false, _ -> None

let private getName (name: string) =
    match name with
    | "" -> None
    | _ -> Some name


let private transformRow (row: CsvRow) : Submission =
    { ID = ID(int32 row.[1])
      Title = Title row.[2]
      Subcommittee = tranformSubcommittee row.[7]
      Decision = transformDecision row.[8]
      ReviewsTotal = int32 row.[12]
      ReviewsDone = int32 row.[13]
      ReviewsLeft = int32 row.[14]
      ReviewsTentative = int32 row.[15]
      CommitteeScore = getScore row.[17]
      ReviewerScore = getScore row.[18]
      OverallScore = getScore row.[19]
      Pname = getName row.[27]
      Pscore = getScore row.[29]
      S1name = getName row.[37]
      S1score = getScore row.[38]
      S2name = getName row.[44]
      S2score = getScore row.[45]
      S3name = getName row.[51]
      S3score = getScore row.[52]
      E1name = getName row.[65]
      E1score = getScore row.[66]
      E2name = getName row.[70]
      E2score = getScore row.[71]
      E3name = getName row.[75]
      E3score = getScore row.[76] }


let private isNotValid (sub: Submission) = sub.Decision.Equals(QR)
let private isValid (sub: Submission) = not (isNotValid sub)


let getData (filePath: string) =
    if not (File.Exists filePath) then
        failwithf "File not found: %s" filePath

    let csv = CsvFile.Load(filePath)

    csv.Rows //
    |> Seq.distinct
    |> Seq.map (transformRow)
    |> Seq.filter isValid


let externalsScores (sub: Submission) =
    [| if sub.E1name.IsSome then
           sub.E1score
       if sub.E2name.IsSome then
           sub.E2score
       if sub.E3name.IsSome then
           sub.E3score |]

let committeeScores (sub: Submission) =
    [| if sub.Pname.IsSome then
           sub.Pscore
       if sub.S1name.IsSome then
           sub.S1score
       if sub.S2name.IsSome then
           sub.S2score
       if sub.S3name.IsSome then
           sub.S3score |]


let printFormatted (submission: Submission) =
    let { ID = (ID id)
          Title = (Title t)
          Subcommittee = s
          ReviewsTotal = rtot
          ReviewsDone = rdone
          ReviewsLeft = rleft
          ReviewsTentative = rentent } =
        submission

    printfn $"{id},{t[0..5]}...,{s},{rtot},{rdone},{rleft},{rentent}"
