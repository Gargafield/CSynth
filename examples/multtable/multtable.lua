local Object = require("@System/Object")
local Console = require("@System/Console")
Module = {}
MultiplicationTable = {}
function MultiplicationTable.PrintTable_8272(num)
    local local_0, local_1
    local_0 = 1
    repeat
        if local_0 <= 50 then
            local_1 = num * local_0
            Console.WriteLine_72344("{0} * {1} = {2}", num, local_0, local_1) -- System.Void System.Console::WriteLine(System.String,System.Object,System.Object,System.Object)
            local_0 = local_0 + 1
            LoopControl = 1
        else
            LoopControl = 0
        end
    until LoopControl == 0
    return nil
end
function MultiplicationTable.Main_8332()
    local local_0, local_1, local_2
    temp_0 = table.create(6)
    temp_0[0] = 2
    temp_0[1] = 3
    temp_0[2] = 5
    temp_0[3] = 7
    temp_0[4] = 11
    temp_0[5] = 13
    local_0 = temp_0
    local_1 = 0
    repeat
        if local_1 < #(local_0) + 1 then
            local_2 = local_0[local_1]
            Console.WriteLine_72312("Multiplication table for {0}:", local_2) -- System.Void System.Console::WriteLine(System.String,System.Object)
            MultiplicationTable.PrintTable_8272(local_2) -- System.Void MultiplicationTable::PrintTable(System.Int32)
            Console.WriteLine_72092() -- System.Void System.Console::WriteLine()
            local_1 = local_1 + 1
            LoopControl = 1
        else
            LoopControl = 0
        end
    until LoopControl == 0
    return nil
end
function MultiplicationTable.new_8423(this)
    Object.new_6830880(this) -- System.Void System.Object::.ctor()
    return nil
end
MultiplicationTable.Main_8332() -- System.Void MultiplicationTable::Main()

