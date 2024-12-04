
local rt = {}

function rt.add_Int32(a, b)
    return bit32.bor(a + b, 0)
end

function rt.mul_Int32(a, b)
    -- Could we go higher? Is this too high?
    if (a + b) < 0xA0000000 then
        return a * b
    end

    local a_low = bit32.band(a, 0xFFFF)
    local a_high = bit32.rshift(a, 16)
    local b_low = bit32.band(b, 0xFFFF)
    local b_high = bit32.rshift(b, 16)

    -- TODO: Could we reduce the number of multiplications?
    local low = a_low * b_low
    local mid1 = a_low * b_high
    local mid2 = a_high * b_low
    local high = a_high * b_high

    -- Combine results with minimal bit32 operations
    return bit32.band(low + bit32.lshift(mid1 + mid2, 16) + bit32.lshift(high, 32), 0xFFFFFFFF)
end    

return rt
