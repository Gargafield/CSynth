
local List = {}

function List.new_0_()
    return {}
end

function List.Add_1_T(this, item)
    table.insert(this, item)
end

function List.GetEnumerator_0_(this)
    return { list = this, index = 0 }
end

function List.get_Count_0_(this)
    return #this
end

local Enumerator = {}
List.Enumerator = Enumerator

function Enumerator.MoveNext_0_(this)
    this.index = this.index + 1
    return this.index <= #this.list
end

function Enumerator.get_Current_0_(this)
    return this.list[this.index]
end

return List