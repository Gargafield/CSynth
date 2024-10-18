local Object = require("@System/Object")
local Array = require("@System/Array")
local Console = require("@System/Console")
local RuntimeHelpers = require("@System/Runtime/CompilerServices/RuntimeHelpers")
Module = {}
Program = {}
function Program.Main_8272()
    local local_0, local_1, local_2
    temp_0 = table.create(10)
    RuntimeHelpers.InitializeArray_0(temp_0, PrivateImplementationDetails[10344]) -- System.Void System.Runtime.CompilerServices.RuntimeHelpers::InitializeArray(System.Array,System.RuntimeFieldHandle)
    local_0 = temp_0
    local_1 = local_0
    local_2 = 0
    repeat
        if local_2 < #(local_1) + 1 then
            Console.WriteLine_72216(local_1[local_2]) -- System.Void System.Console::WriteLine(System.Int32)
            local_2 = local_2 + 1
            LoopControl = 1
        else
            LoopControl = 0
        end
    until LoopControl == 0
    Console.WriteLine_72092() -- System.Void System.Console::WriteLine()
    Array.Reverse_6769728(local_0) -- System.Void System.Array::Reverse<System.Int32>(!!0[])
    local_1 = local_0
    local_2 = 0
    repeat
        if local_2 < #(local_1) + 1 then
            Console.WriteLine_72216(local_1[local_2]) -- System.Void System.Console::WriteLine(System.Int32)
            local_2 = local_2 + 1
            LoopControl = 1
        else
            LoopControl = 0
        end
    until LoopControl == 0
end
function Program.new_8363(this)
    Object.new_6830880(this) -- System.Void System.Object::.ctor()
end
PrivateImplementationDetails = {}
PrivateImplementationDetails[10344] = buffer.fromstring("\x01\x00\x00\x00\x02\x00\x00\x00\x03\x00\x00\x00\x04\x00\x00\x00\x05\x00\x00\x00\x06\x00\x00\x00\x07\x00\x00\x00\x08\x00\x00\x00\x09\x00\x00\x00\x0A\x00\x00\x00")
Program.Main_8272() -- System.Void Program::Main()

