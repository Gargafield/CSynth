
using System;

class Program
{
    static void Main()
    {
        var arr = new int[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
        foreach (int i in arr) {
            Console.WriteLine(i);
        }

        Console.WriteLine();
        Array.Reverse(arr);

        foreach (int i in arr) {
            Console.WriteLine(i);
        }
    }
}