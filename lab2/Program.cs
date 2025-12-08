using System;
using System.Linq;
using ParallelSum;

class Program
{
    static void Main(string[] args)
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;
        
        while (true)
        {
            Console.Clear();
            int arraySize;
            while (true)
            {
                Console.Write("Введіть довжину масиву: ");

                if (Console.KeyAvailable && Console.ReadKey(true).Key == ConsoleKey.Escape)
                {
                    Console.WriteLine("\n\nПрограма завершена.");
                    return;
                }
                
                string input = Console.ReadLine();

                if (string.IsNullOrEmpty(input))
                {
                    continue;
                }

                if (int.TryParse(input, out arraySize) && arraySize > 0)
                {
                    break;
                }

                Console.WriteLine("Помилка! Введіть додатне ціле число.\n");
            }
            
            int[] testArray = GenerateRandomArray(arraySize);
            Console.WriteLine($"\nМасив згенеровано (розмір: {arraySize})");
            
            long expectedSum = testArray.Sum(x => (long)x);
            
            Console.WriteLine("\n" + new string('-', 70) + "\n");
            

            var calculator = new ParallelArraySumCalculator(testArray);
            int result = calculator.CalculateSum();
            
            Console.WriteLine("\n" + new string('-', 70));
            Console.WriteLine($"Перевірка:");
            Console.WriteLine($"  Отриманий результат: {result}");
            Console.WriteLine($"  Очікувана сума: {expectedSum}");
            Console.WriteLine($"  Результат правильний: {result == expectedSum}");
            
            Console.WriteLine("\n" + new string('=', 70));
            Console.WriteLine("Натисніть ENTER для нового обчислення або ESC для виходу...");
            
            ConsoleKeyInfo keyInfo = Console.ReadKey(true);
            
            if (keyInfo.Key == ConsoleKey.Escape)
            {
                Console.WriteLine("\nПрограма завершена.");
                break;
            }
        }
    }

    static int[] GenerateRandomArray(int size)
    {
        var random = new Random();
        int[] arr = new int[size];
        
        for (int i = 0; i < size; i++)
        {
            arr[i] = random.Next(1, 100);
        }
        
        return arr;
    }
}