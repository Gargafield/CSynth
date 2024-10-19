local Object = require("@System/Object")
local Int32 = require("@System/Int32")
local String = require("@System/String")
local Console = require("@System/Console")
local List1 = require("@System/Collections/Generic/List1")
Module = {}
Program = {}
function Program.Main_8272()
    local local_0, local_3, local_1, local_4, local_5, local_2
    local_0 = List1.new_8323580() -- System.Void System.Collections.Generic.List`1<System.Int32>::.ctor()
    Console.WriteLine_72296("Enter a number (or 'done' to finish): ") -- System.Void System.Console::WriteLine(System.String)
    repeat
        result_16 = Console.ReadLine_72080() -- System.String System.Console::ReadLine()
        local_3 = result_16
        result_28 = String.op_Equality_6861180(local_3, "done") -- System.Boolean System.String::op_Equality(System.String,System.String)
        if result_28 ~= 0 then
            LoopControl = 0
        else
            result_37 = Int32.Parse_7019272(local_3) -- System.Int32 System.Int32::Parse(System.String)
            List1.Add_8324160(local_0, result_37) -- System.Void System.Collections.Generic.List`1<System.Int32>::Add(!0)
            LoopControl = 1
        end
    until LoopControl == 0
    local_1 = 0
    result_52 = List1.GetEnumerator_8325788(local_0) -- System.Collections.Generic.List`1/Enumerator<!0> System.Collections.Generic.List`1<System.Int32>::GetEnumerator()
    local_4 = result_52
    repeat
        result_77 = List1.Enumerator.MoveNext_8327844(local_4) -- System.Boolean System.Collections.Generic.List`1/Enumerator<System.Int32>::MoveNext()
        if result_77 ~= 0 then
            result_63 = List1.Enumerator.get_Current_8328000(local_4) -- !0 System.Collections.Generic.List`1/Enumerator<System.Int32>::get_Current()
            local_5 = result_63
            local_1 = local_1 + local_5
            LoopControl = 1
        else
            LoopControl = 0
        end
    until LoopControl == 0
    result_103 = List1.get_Count_8323924(local_0) -- System.Int32 System.Collections.Generic.List`1<System.Int32>::get_Count()
    local_2 = local_1 / result_103
    Console.WriteLine_72312("Average: {0}", local_2) -- System.Void System.Console::WriteLine(System.String,System.Object)
    return nil
end
function Program.new_8428(this)
    Object.new_6830880(this) -- System.Void System.Object::.ctor()
    return nil
end
Program.Main_8272() -- System.Void Program::Main()

