local Object = require("@System/Object")
local Console = require("@System/Console")
local Module = {}
local FizzBuzz = {}
function FizzBuzz.Main()
    local local_0
    local_0 = 1
    repeat
        if local_0 <= 100 then
            if local_0 % 15 ~= 0 then
                if local_0 % 3 ~= 0 then
                    if local_0 % 5 ~= 0 then
                        Console.WriteLine_Int32(local_0)
                    else
                        Console.WriteLine_String("Buzz")
                    end
                else
                    Console.WriteLine_String("Fizz")
                end
            else
                Console.WriteLine_String("FizzBuzz")
            end
            local_0 = local_0 + 1
            LoopControl = 1
        else
            LoopControl = 0
        end
    until LoopControl == 0
end
function FizzBuzz.new(this)
    Object.new(this)
end
FizzBuzz.Main()

