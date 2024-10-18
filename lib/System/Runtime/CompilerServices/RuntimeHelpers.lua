
local RuntimeHelpers = {}

function RuntimeHelpers.InitializeArray_0(array, fieldHandle)
    for i = 0, (buffer.len(fieldHandle) // 4) - 1 do
        array[i] = buffer.readi32(fieldHandle, i * 4)
    end
end

return RuntimeHelpers
