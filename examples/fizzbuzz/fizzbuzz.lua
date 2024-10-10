local Object = require("@System/Object")
local Console = require("@System/Console")
local Module = {}
local FizzBuzz = {}
function FizzBuzz.Main_8272()
    local local_0
    local_0 = 1
    repeat
        if local_0 <= 100 then
            if local_0 % 15 ~= 0 then
                if local_0 % 3 ~= 0 then
                    if local_0 % 5 ~= 0 then
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
            local_0 = local_0 + 1
            LoopControl = 1
        else
            LoopControl = 0
        end
    until LoopControl == 0
end
function FizzBuzz.new_8356(this)
    Object.new_6830880(this) -- System.Void System.Object::.ctor()
end
FizzBuzz.Main_8272() -- System.Void FizzBuzz::Main()

