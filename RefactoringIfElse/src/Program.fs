open Domain
[<EntryPoint>]
let main argv =
    let pizza = {
      MenuNumber = 1
      Name = "Pizza"
    }
    let salad = {
      MenuNumber = 2
      Name = "Salad"
    }
    let order = {Foods = [pizza]}
    let ipo = {
      PlacedOrder = order
      ServedFoods = [pizza]
      PreparedFoods = []
    }

    serveFood salad ipo
    0 // return an integer exit code