local Object = require("@System/Object")
local Console = require("@System/Console")
local Module = {}
local FizzBuzz = {}
function FizzBuzz.Main(args)
    local local_0
    local_0 = 1
    repeat
        if local_0 <= 100 then
            if local_0 % 15 ~= 0 then
                if local_0 % 3 ~= 0 then
                    if local_0 % 5 ~= 0 then
                        Console.WriteLine(local_0)
                    else
                        Console.WriteLine("Buzz")
                    end
                else
                    Console.WriteLine("Fizz")
                end
            else
                Console.WriteLine("FizzBuzz")
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

