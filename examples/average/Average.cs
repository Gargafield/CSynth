using System;
using System.Collections.Generic;

class Program
{
    static void Main()
    {
        var values = new List<int>();

        Console.WriteLine("Enter a number (or 'done' to finish): ");
        while (true) {
            var input = Console.ReadLine();

            if (input == "done") {
                break;
            }

            values.Add(int.Parse(input));
        }

        var sum = 0;
        foreach (var value in values) {
            sum += value;
        }

        var average = (float)sum / (float)values.Count;
        Console.WriteLine("Average: {0}", average);
    }
}
