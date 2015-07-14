module IdentityStore    

open System.Security.Claims       


let getClaims userName =
    seq {
        yield (ClaimTypes.Name, userName)
        if (userName = "Admin") then
            yield (ClaimTypes.Role, "Admin")
        if (userName = "Foo") then
            yield (ClaimTypes.Role, "SuperUser")            
    } |> Seq.map (fun x -> new Claim(fst x, snd x)) |> async.Return

let isValidCredentials username password =
    username = password |> async.Return

    