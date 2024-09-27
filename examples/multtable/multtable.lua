local Object = require("@System/Object")
local Console = require("@System/Console")
local Module = {}
local MultiplicationTable = {}
function MultiplicationTable.PrintTable_Int32(num)
    local local_0, local_1
    local_0 = 1
    repeat
        if local_0 <= 50 then
            local_1 = num * local_0
            Console.WriteLine_String_Object_Object_Object("{0} * {1} = {2}", num, local_0, local_1)
            local_0 = local_0 + 1
            LoopControl = 1
        else
            LoopControl = 0
        end
    until LoopControl == 0
end
function MultiplicationTable.Main()
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
            Console.WriteLine_String_Object("Multiplication table for {0}:", local_2)
            MultiplicationTable.PrintTable_Int32(local_2)
            Console.WriteLine()
            local_1 = local_1 + 1
            LoopControl = 1
        else
            LoopControl = 0
        end
    until LoopControl == 0
end
function MultiplicationTable.new(this)
    Object.new(this)
end
MultiplicationTable.Main()

