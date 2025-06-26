local Object = require("@System/Object")
local Console = require("@System/Console")
local String = require("@System/String")
local Int32 = require("@System/Int32")
Module = {}
Program = {}
function Program.Main_0_()
    local local_0, local_1, local_2, result_24, result_29, result_51, overflow_0, result_56, result_73, result_78
    local_0 = 0x7B
    local_1 = local_0
    local_2 = local_1
    result_24 = Int32.ToString_0_(local_0) -- System.String System.Int32::ToString()
    result_29 = String.Concat_2_String_String("Value: ", result_24) -- System.String System.String::Concat(System.String,System.String)
    Console.WriteLine_1_String(result_29) -- System.Void System.Console::WriteLine(System.String)
    overflow_0 = nil
    if local_1 ~= nil then
        result_51 = Object.ToString_0_(local_1) -- System.String System.Object::ToString()
        overflow_0 = result_51
    else
        overflow_0 = nil
    end
    result_56 = String.Concat_2_String_String("Boxed Value: ", overflow_0) -- System.String System.String::Concat(System.String,System.String)
    Console.WriteLine_1_String(result_56) -- System.Void System.Console::WriteLine(System.String)
    result_73 = Int32.ToString_0_(local_2) -- System.String System.Int32::ToString()
    result_78 = String.Concat_2_String_String("Unboxed Value: ", result_73) -- System.String System.String::Concat(System.String,System.String)
    Console.WriteLine_1_String(result_78) -- System.Void System.Console::WriteLine(System.String)
    return nil
end
function Program.new_0_(this)
    Object.new_0_(this) -- System.Void System.Object::.ctor()
    return nil
end
Program.Main_0_() -- System.Void Program::Main()

