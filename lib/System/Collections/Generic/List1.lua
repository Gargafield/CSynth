
local List = {}

function List.new_8326360()
    return {}
end

function List.Add_8326940(this, item)
    table.insert(this, item)
end

function List.GetEnumerator_8328568(this)
    return { list = this, index = 0 }
end

function List.get_Count_8326704(this)
    return #this
end

local Enumerator = {}
List.Enumerator = Enumerator

function Enumerator.MoveNext_8330624(this)
    this.index = this.index + 1
    return this.index <= #this.list
end

function Enumerator.get_Current_8330780(this)
    return this.list[this.index]
end

return List