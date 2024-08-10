local Object = require("@System/Object")
local Console = require("@System/Console")
local Module = {}
local FizzBuzz = {}
function FizzBuzz.Main(arg0)
    local loc0, condition, LoopControl
    loc0 = 1
    repeat
        condition = loc0 <= 100
        if not condition then
            LoopControl = 0
        else
            condition = loc0 % 15 ~= 0
            if not condition then
                Console.WriteLine("FizzBuzz")
            else
                condition = loc0 % 3 ~= 0
                if not condition then
                    Console.WriteLine("Fizz")
                else
                    condition = loc0 % 5 ~= 0
                    if not condition then
                        Console.WriteLine("Buzz")
                    else
                        Console.WriteLine(loc0)
                    end
                end
            end
            loc0 = loc0 + 1
            LoopControl = 1
        end
    until LoopControl == 0
    return 
end
function FizzBuzz.new(self)
    Object.new(self)
    return 
end
FizzBuzz.Main()

