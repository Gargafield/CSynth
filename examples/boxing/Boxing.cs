using System;

class Program
{
    static void Main()
    {
        int value = 123;
        object boxedValue = value;

        int unboxedValue = (int)boxedValue;

        Console.WriteLine("Value: " + value);
        Console.WriteLine("Boxed Value: " + boxedValue);
        Console.WriteLine("Unboxed Value: " + unboxedValue);
    }
}
