local Object = require("@System/Object")
local Console = require("@System/Console")
Module = {}
FizzBuzz = {}
function FizzBuzz.Main_8272()
    local local_0, LoopControl
    local_0 = 0x1
    repeat
        if local_0 <= 0x64 then
            if local_0 % 0xF ~= 0x0 then
                if local_0 % 0x3 ~= 0x0 then
                    if local_0 % 0x5 ~= 0x0 then
                        Console.WriteLine_72216(local_0) -- System.Void System.Console::WriteLine(System.Int32)
                    else
                        Console.WriteLine_72296("Buzz") -- System.Void System.Console::WriteLine(System.String)
                    end
                else
                    Console.WriteLine_72296("Fizz") -- System.Void System.Console::WriteLine(System.String)
                end
            else
                Console.WriteLine_72296("FizzBuzz") -- System.Void System.Console::WriteLine(System.String)
            end
            local_0 = bit32.bor(local_0 + 0x1, 0x0)
            LoopControl = 0x1
        else
            LoopControl = 0x0
        end
    until not (LoopControl ~= 0x0)
    return nil
end
function FizzBuzz.new_8356(this)
    Object.new_6832912(this) -- System.Void System.Object::.ctor()
    return nil
end
FizzBuzz.Main_8272() -- System.Void FizzBuzz::Main()

