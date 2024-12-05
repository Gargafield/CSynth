local Object = require("@System/Object")
local Console = require("@System/Console")
Module = {}
Fibonacci = {}
function Fibonacci.Main_8272()
    local local_0, local_1, local_2, LoopControl, local_3
    local_0 = 0x19
    local_1 = table.create(local_0)
    local_1[0x0] = 0x0
    local_1[0x1] = 0x1
    local_2 = 0x2
    repeat
        if local_2 < local_0 then
            local_1[local_2] = bit32.bor(local_1[bit32.bor(local_2 - 0x1, 0x0)] + local_1[bit32.bor(local_2 - 0x2, 0x0)], 0x0)
            local_2 = bit32.bor(local_2 + 0x1, 0x0)
            LoopControl = 0x1
        else
            LoopControl = 0x0
        end
    until not (LoopControl ~= 0x0)
    local_3 = 0x0
    repeat
        if local_3 < local_0 then
            Console.WriteLine_72216(local_1[local_3]) -- System.Void System.Console::WriteLine(System.Int32)
            local_3 = bit32.bor(local_3 + 0x1, 0x0)
            LoopControl = 0x1
        else
            LoopControl = 0x0
        end
    until not (LoopControl ~= 0x0)
    return nil
end
function Fibonacci.new_8349(this)
    Object.new_6832912(this) -- System.Void System.Object::.ctor()
    return nil
end
Fibonacci.Main_8272() -- System.Void Fibonacci::Main()

