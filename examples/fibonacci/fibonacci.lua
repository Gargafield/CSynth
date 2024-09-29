local Object = require("@System/Object")
local Console = require("@System/Console")
local Module = {}
local Fibonacci = {}
function Fibonacci.Main()
    local local_0, local_1, local_2, local_3
    local_0 = 50
    local_1 = table.create(local_0)
    local_1[0] = 0
    local_1[1] = 1
    local_2 = 2
    repeat
        if local_2 < local_0 then
            local_1[local_2] = local_1[local_2 - 1] + local_1[local_2 - 2]
            local_2 = local_2 + 1
            LoopControl = 1
        else
            LoopControl = 0
        end
    until LoopControl == 0
    local_3 = 0
    repeat
        if local_3 < local_0 then
            Console.WriteLine_Int32(local_1[local_3])
            local_3 = local_3 + 1
            LoopControl = 1
        else
            LoopControl = 0
        end
    until LoopControl == 0
end
function Fibonacci.new(this)
    Object.new(this)
end
Fibonacci.Main()

