local Object = require("@System/Object")
local Console = require("@System/Console")
local String = require("@System/String")
local Int32 = require("@System/Int32")
Module = {}
Program = {}
function Program.Main_8272()
    local local_0, local_1, local_2
    local_0 = 123
    local_1 = local_0
    local_2 = local_1
    result_24 = Int32.ToString_7019192(local_0) -- System.String System.Int32::ToString()
    result_29 = String.Concat_6865052("Value: ", result_24) -- System.String System.String::Concat(System.String,System.String)
    Console.WriteLine_72296(result_29) -- System.Void System.Console::WriteLine(System.String)
    overflow_0 = nil
    if local_1 ~= 0 then
        result_51 = Object.ToString_6830888(local_1) -- System.String System.Object::ToString()
        overflow_0 = result_51
        overflow_1 = "Boxed Value: "
    else
        overflow_0 = nil
        overflow_1 = "Boxed Value: "
    end
    result_56 = String.Concat_6865052("Boxed Value: ", overflow_0) -- System.String System.String::Concat(System.String,System.String)
    Console.WriteLine_72296(result_56) -- System.Void System.Console::WriteLine(System.String)
    result_73 = Int32.ToString_7019192(local_2) -- System.String System.Int32::ToString()
    result_78 = String.Concat_6865052("Unboxed Value: ", result_73) -- System.String System.String::Concat(System.String,System.String)
    Console.WriteLine_72296(result_78) -- System.Void System.Console::WriteLine(System.String)
    return nil
end
function Program.new_8373(this)
    Object.new_6830880(this) -- System.Void System.Object::.ctor()
    return nil
end
Program.Main_8272() -- System.Void Program::Main()

