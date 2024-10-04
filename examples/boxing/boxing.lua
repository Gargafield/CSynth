local Object = require("@System/Object")
local Console = require("@System/Console")
local String = require("@System/String")
local Int32 = require("@System/Int32")
local Module = {}
local Program = {}
function Program.Main()
    local local_0, local_1, local_2
    local_0 = 123
    local_1 = local_0
    local_2 = local_1
    result_24 = Int32.ToString(local_0)
    result_29 = String.Concat_String_String("Value: ", result_24)
    Console.WriteLine_String(result_29)
    overflow_0 = nil
    if local_1 ~= 0 then
        result_51 = Object.ToString(local_1)
        overflow_0 = result_51
    else
        overflow_0 = nil
    end
    result_56 = String.Concat_String_String("Boxed Value: ", overflow_0)
    Console.WriteLine_String(result_56)
    result_73 = Int32.ToString(local_2)
    result_78 = String.Concat_String_String("Unboxed Value: ", result_73)
    Console.WriteLine_String(result_78)
end
function Program.new(this)
    Object.new(this)
end
Program.Main()

