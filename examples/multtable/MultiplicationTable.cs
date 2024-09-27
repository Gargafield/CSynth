using System;

class MultiplicationTable
{
    static void PrintTable(int num) {
        for (int i = 1; i <= 50; i++)
        {
            int result = num * i;
            Console.WriteLine("{0} * {1} = {2}", num, i, result);
        }
    }

    static void Main()
    {
        var factors = new int[6];
        factors[0] = 2;
        factors[1] = 3;
        factors[2] = 5;
        factors[3] = 7;
        factors[4] = 11;
        factors[5] = 13;

        foreach (int factor in factors)
        {
            Console.WriteLine("Multiplication table for {0}:", factor);
            PrintTable(factor);
            Console.WriteLine();
        }
    }
}