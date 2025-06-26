local Object = require("@System/Object")
local Array = require("@System/Array")
local Console = require("@System/Console")
local RuntimeHelpers = require("@System/Runtime/CompilerServices/RuntimeHelpers")
Module = {}
Program = {}
function Program.Main_0_()
    local temp_0, local_0, local_1, local_2, LoopControl
    temp_0 = table.create(0xA)
    RuntimeHelpers.InitializeArray_2_Array_RuntimeFieldHandle(temp_0, PrivateImplementationDetails[0x2858]) -- System.Void System.Runtime.CompilerServices.RuntimeHelpers::InitializeArray(System.Array,System.RuntimeFieldHandle)
    local_0 = temp_0
    local_1 = local_0
    local_2 = 0x0
    repeat
        if local_2 < #(local_1) + 1 then
            Console.WriteLine_1_Int32(local_1[local_2]) -- System.Void System.Console::WriteLine(System.Int32)
            local_2 = bit32.bor(local_2 + 0x1, 0x0)
            LoopControl = 0x1
        else
            LoopControl = 0x0
        end
    until not (LoopControl ~= 0x0)
    Console.WriteLine_0_() -- System.Void System.Console::WriteLine()
    Array.Reverse_1_T(local_0) -- System.Void System.Array::Reverse<System.Int32>(!!0[])
    local_1 = local_0
    local_2 = 0x0
    repeat
        if local_2 < #(local_1) + 1 then
            Console.WriteLine_1_Int32(local_1[local_2]) -- System.Void System.Console::WriteLine(System.Int32)
            local_2 = bit32.bor(local_2 + 0x1, 0x0)
            LoopControl = 0x1
        else
            LoopControl = 0x0
        end
    until not (LoopControl ~= 0x0)
    return nil
end
function Program.new_0_(this)
    Object.new_0_(this) -- System.Void System.Object::.ctor()
    return nil
end
PrivateImplementationDetails = {}
PrivateImplementationDetails[0x2858] = buffer.fromstring("\x01\x00\x00\x00\x02\x00\x00\x00\x03\x00\x00\x00\x04\x00\x00\x00\x05\x00\x00\x00\x06\x00\x00\x00\x07\x00\x00\x00\x08\x00\x00\x00\x09\x00\x00\x00\x0A\x00\x00\x00")
Program.Main_0_() -- System.Void Program::Main()

