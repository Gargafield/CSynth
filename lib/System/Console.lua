
local Console = {}

function Console.WriteLine()
    print()
end

function Console.WriteLine_String(value)
    print(value)
end

function Console.WriteLine_String_Object(value, obj1)
    print((string.gsub(value, "{0}", obj1)))
end

function Console.WriteLine_String_Object_Object(value, obj1, obj2)
    print((string.gsub(string.gsub(value, "{0}", obj1), "{1}", obj2)))
end

function Console.WriteLine_String_Object_Object_Object(value, obj1, obj2, obj3)
    print((string.gsub(string.gsub(string.gsub(value, "{0}", obj1), "{1}", obj2), "{2}", obj3)))
end


return Console