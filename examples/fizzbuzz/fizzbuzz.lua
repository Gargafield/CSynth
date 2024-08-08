local Console = require("@System/Console")
local loc0, condition, result, LoopControl

loc0 = 1
repeat
    condition = loc0 <= 100
    if condition then
        condition = loc0 % 15 ~= 0
        if condition then
            condition = loc0 % 3 ~= 0
            if condition then
                condition = loc0 % 5 ~= 0
                if condition then
                    result = Console.WriteLine(loc0)
                else
                    result = Console.WriteLine("Buzz")
                end
            else
                result = Console.WriteLine("Fizz")
            end
        else
            result = Console.WriteLine("FizzBuzz")
        end
        loc0 = loc0 + 1
        LoopControl = true
    else
        LoopControl = false
    end
until not LoopControl
return result

