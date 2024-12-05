
local String = {}

local function isNullOrEmpty(value)
    return value == nil or value == ""
end

function String.Concat_6867084(str1, str2)
    if isNullOrEmpty(str1) then
        if isNullOrEmpty(str2) then
            return ""
        end
        return str2
    end

    return str1 .. str2
end

local function toHex(str)
    local output = {}
    for i = 1, #str do
        local char = string.sub(str, i, i)
        table.insert(output, string.byte(char) .. " ")
    end
    return table.concat(output)
end

function String.op_Equality_6863212(str1, str2)
    if str1 == nil then
        return str2 == nil
    end

    return str1 == str2
end

return String
