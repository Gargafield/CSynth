local Console = require("@System/Console")
local loc0, condition, LoopControl, result
loc0 = 1
repeat
    condition = loc0 <= 100
    if not condition then
        LoopControl = 0
    else
        condition = loc0 % 15 ~= 0
        if not condition then
            result = Console.WriteLine("FizzBuzz")
        else
            condition = loc0 % 3 ~= 0
            if not condition then
                result = Console.WriteLine("Fizz")
            else
                condition = loc0 % 5 ~= 0
                if not condition then
                    result = Console.WriteLine("Buzz")
                else
                    result = Console.WriteLine(loc0)
                end
            end
        end
        loc0 = loc0 + 1
        LoopControl = 1
    end
until LoopControl == 0
return result

