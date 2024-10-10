
local stdio = require("@lune/stdio")

local Console = {}

function Console.WriteLine_72092()
    print()
end

function Console.WriteLine_72216(value)
    print(value)
end

function Console.WriteLine_72296(value)
    print(value)
end

function Console.WriteLine_72312(value, obj1)
    print((string.gsub(value, "{0}", obj1)))
end

function Console.WriteLine_String_Object_Object(value, obj1, obj2)
    print((string.gsub(string.gsub(value, "{0}", obj1), "{1}", obj2)))
end

function Console.WriteLine_72344(value, obj1, obj2, obj3)
    print((string.gsub(string.gsub(string.gsub(value, "{0}", obj1), "{1}", obj2), "{2}", obj3)))
end

local buf = {}
function Console.ReadLine_72080()
    local contents = stdio.readToEnd()
    if contents ~= nil then
        table.insert(buf, contents)
    end

    local output = {}
    while true do
        if #buf == 0 then
            break
        end
        local v = buf[1]

        local index = string.find(v, "\n")
        if index ~= nil then
            table.insert(output, string.sub(v, 1, index - 1))
            if index + 1 < string.len(v) then
                buf[1] = string.sub(v, index + 1)
            else
                table.remove(buf, 1)
            end
            break;
        else
            table.insert(output, v)
            table.remove(buf, 1)
        end
    end

    -- Remove carriage return
    local v = output[#output]
    if string.sub(v, -1) == "\r" then
        output[#output] = string.sub(v, 1, -2)
    end

    return table.concat(output)
end

return Console