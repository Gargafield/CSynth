
local Array = {}

function Array.Reverse_1_T(array)
    local length = #array
    for i = 0, length // 2 do
        local temp = array[i]
        array[i] = array[length - i]
        array[length - i] = temp
    end
end

return Array
