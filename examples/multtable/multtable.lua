local rt = require("@lib/Runtime")
local Object = require("@System/Object")
local Console = require("@System/Console")
Module = {}
MultiplicationTable = {}
function MultiplicationTable.PrintTable_1_Int32(num)
    local local_0, local_1, LoopControl
    local_0 = 0x1
    repeat
        if local_0 <= 0x32 then
            local_1 = rt.mul_Int32(num, local_0) -- mul_Int32
            Console.WriteLine_4_String_Object_Object_Object("{0} * {1} = {2}", num, local_0, local_1) -- System.Void System.Console::WriteLine(System.String,System.Object,System.Object,System.Object)
            local_0 = bit32.bor(local_0 + 0x1, 0x0)
            LoopControl = 0x1
        else
            LoopControl = 0x0
        end
    until not (LoopControl ~= 0x0)
    return nil
end
function MultiplicationTable.Main_0_()
    local temp_0, local_0, local_1, local_2, LoopControl
    temp_0 = table.create(0x6)
    temp_0[0x0] = 0x2
    temp_0[0x1] = 0x3
    temp_0[0x2] = 0x5
    temp_0[0x3] = 0x7
    temp_0[0x4] = 0xB
    temp_0[0x5] = 0xD
    local_0 = temp_0
    local_1 = 0x0
    repeat
        if local_1 < #(local_0) + 1 then
            local_2 = local_0[local_1]
            Console.WriteLine_2_String_Object("Multiplication table for {0}:", local_2) -- System.Void System.Console::WriteLine(System.String,System.Object)
            MultiplicationTable.PrintTable_1_Int32(local_2) -- System.Void MultiplicationTable::PrintTable(System.Int32)
            Console.WriteLine_0_() -- System.Void System.Console::WriteLine()
            local_1 = bit32.bor(local_1 + 0x1, 0x0)
            LoopControl = 0x1
        else
            LoopControl = 0x0
        end
    until not (LoopControl ~= 0x0)
    return nil
end
function MultiplicationTable.new_0_(this)
    Object.new_0_(this) -- System.Void System.Object::.ctor()
    return nil
end
MultiplicationTable.Main_0_() -- System.Void MultiplicationTable::Main()

