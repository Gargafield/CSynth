
local rt = {}

function rt.add_Int32(a, b)
    return bit32.bor(a + b, 0)
end

return rt
