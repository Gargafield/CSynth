
local List = {}

function List.new_8323580()
    return {}
end

function List.Add_8324160(this, item)
    table.insert(this, item)
end

function List.GetEnumerator_8325788(this)
    return { list = this, index = 0 }
end

function List.get_Count_8323924(this)
    return #this
end

local Enumerator = {}
List.Enumerator = Enumerator

function Enumerator.MoveNext_8327844(this)
    this.index = this.index + 1
    return this.index <= #this.list and 1 or 0
end

function Enumerator.get_Current_8328000(this)
    return this.list[this.index]
end

return List