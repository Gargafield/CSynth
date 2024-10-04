
local String = {}

local function isNullOrEmpty(value)
    return value == nil or value == ""
end

function String.Concat_String_String(str1, str2)
    if isNullOrEmpty(str1) then
        if isNullOrEmpty(str2) then
            return ""
        end
        return str2
    end

    return str1 .. str2
end

return String
