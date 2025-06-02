// module UISTstats
open Utils
open FSharp.Stats
open System.IO

let scores (submission: Submission) =
    [| submission.CommitteeScore
       submission.ReviewerScore
       submission.OverallScore
       submission.Pscore
       submission.S1score
       submission.S2score
       submission.S3score
       submission.E1score
       submission.E2score
       submission.E3score |]
    |> Array.choose id
    |> Array.map (function
        | Score s -> s)

let averageSd (sub: Submission) = //
    let lscore = sub |> scores
    (Seq.average lscore, Seq.stDev lscore)


let median (sub: Submission) = //
    sub |> scores |> Seq.median



// helpers
let isSplitA (sub: Submission) = sub.Subcommittee = SplitA
let isSplitB (sub: Submission) = sub.Subcommittee = SplitB

let isConflict (sub: Submission) =
    sub.Pname.IsNone && sub.S1name.IsNone && sub.ReviewsTotal > 0

let metaMissing (sub: Submission) = sub.Pscore.IsNone

let committeeMissing (sub: Submission) =
    let reviews = sub |> committeeScores |> Array.choose id |> Array.length
    if sub.Pscore.IsSome then reviews - 1 = 0 else reviews = 0


let externalMissing (missing: int) (sub: Submission) =
    let scores = sub |> externalsScores
    let submitted = scores |> Array.choose id
    scores.Length - submitted.Length = missing

let tap message x =
    printf message
    printfn "%A" x
    x


let getLink (x: Submission) =
    let (ID id) = x.ID
    $"https://new.precisionconference.com/uist25a/chair/subs/{id}"



// Main


// let quantile75 = Quantile.mode 0.75 sample
// printfn "75th percentile: %f" quantile75


let printStats (header: string, submissions: seq<Submission>) =
    printfn "%s" header
    submissions |> Seq.length |> printfn "Total submissions: %d"

    let links = submissions |> Seq.filter metaMissing |> Seq.map getLink

    printfn
        "Meta: %d (%.1f%%)"
        (Seq.length links)
        (100.0 - float (Seq.length links) / float (Seq.length submissions) * 100.0)

    // links |> Seq.iter (printfn "\t%s")
    printfn ""

    let links = submissions |> Seq.filter committeeMissing |> Seq.map getLink
    printfn "Missing committee scores: %d" (Seq.length links)
    links |> Seq.iter (printfn "\t%s")
    printfn ""

    let links = submissions |> Seq.filter (externalMissing 1) |> Seq.map getLink
    printfn "Missing 1 externals scores: %d" (Seq.length links)
    links |> Seq.iter (printfn "\t%s")
    printfn ""

    let links = submissions |> Seq.filter (externalMissing 2) |> Seq.map getLink
    printfn "Missing 1 externals scores: %d" (Seq.length links)
    links |> Seq.iter (printfn "\t%s")
    printfn ""


[<EntryPoint>]
let main argv =
    printfn "UIST 2025 Statistics"
    printfn "====================="

    // Load the data
    let data = getData (Directory.GetCurrentDirectory() + "/data/Submissions.csv")

    // Split A and B
    let all = data |> Seq.filter (isConflict >> not)
    let a = data |> Seq.filter isSplitA |> Seq.filter (isConflict >> not)
    let b = data |> Seq.filter isSplitB |> Seq.filter (isConflict >> not)

    printStats ("All submissions", all)
    printfn "====================="
    printStats ("Split A submissions", a)
    printfn "====================="
    printStats ("Split B submissions", b)
    0
