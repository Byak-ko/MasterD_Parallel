using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

class ParallelArraySum
{
    private const int ARRAY_SIZE = 1_000_000;
    private static long[] array;

    static void Main()
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;
        Console.WriteLine("=== Паралельне обчислення суми масиву ===\n");

        array = new long[ARRAY_SIZE];
        Random random = new Random(42);
        for (int i = 0; i < ARRAY_SIZE; i++)
        {
            array[i] = random.Next(1, 100);
        }

        Console.WriteLine($"Розмір масиву: {ARRAY_SIZE:N0} елементів\n");

        int[] threadCounts = { 2, 4, 8, 16 };

        foreach (int threadCount in threadCounts)
        {
            Console.WriteLine($"--- Кількість потоків: {threadCount} ---");
            
            long sum1 = SumWithManualThreads(threadCount);
            long sum2 = SumWithTPL(threadCount);
            long sum3 = SumSequential();
            
            Console.WriteLine($"Результати однакові: {sum1 == sum2 && sum2 == sum3}\n");
        }
    }

    static long SumWithManualThreads(int threadCount)
    {
        Stopwatch sw = Stopwatch.StartNew();
        
        Thread[] threads = new Thread[threadCount];
        long[] partialSums = new long[threadCount];
        int chunkSize = ARRAY_SIZE / threadCount;

        for (int i = 0; i < threadCount; i++)
        {
            int threadIndex = i;
            int start = threadIndex * chunkSize;
            int end = (threadIndex == threadCount - 1) ? ARRAY_SIZE : start + chunkSize;

            threads[threadIndex] = new Thread(() =>
            {
                long localSum = 0;
                for (int j = start; j < end; j++)
                {
                    localSum += array[j];
                }
                partialSums[threadIndex] = localSum;
            });
            
            threads[threadIndex].Start();
        }

        foreach (Thread thread in threads)
        {
            thread.Join();
        }

        long totalSum = partialSums.Sum();
        sw.Stop();

        Console.WriteLine($"Метод 1 (Thread): {sw.ElapsedMilliseconds} мс, Сума: {totalSum:N0}");
        return totalSum;
    }

    static long SumWithTPL(int threadCount)
    {
        Stopwatch sw = Stopwatch.StartNew();
        
        int chunkSize = ARRAY_SIZE / threadCount;
        Task<long>[] tasks = new Task<long>[threadCount];

        for (int i = 0; i < threadCount; i++)
        {
            int threadIndex = i;
            int start = threadIndex * chunkSize;
            int end = (threadIndex == threadCount - 1) ? ARRAY_SIZE : start + chunkSize;

            tasks[threadIndex] = Task.Run(() =>
            {
                long localSum = 0;
                for (int j = start; j < end; j++)
                {
                    localSum += array[j];
                }
                return localSum;
            });
        }

        Task.WaitAll(tasks);
        
        long totalSum = tasks.Sum(t => t.Result);
        sw.Stop();

        Console.WriteLine($"Метод 2 (TPL):    {sw.ElapsedMilliseconds} мс, Сума: {totalSum:N0}");
        return totalSum;
    }

    static long SumSequential()
    {
        Stopwatch sw = Stopwatch.StartNew();
        
        long sum = 0;
        for (int i = 0; i < ARRAY_SIZE; i++)
        {
            sum += array[i];
        }
        
        sw.Stop();
        Console.WriteLine($"Послідовно:       {sw.ElapsedMilliseconds} мс, Сума: {sum:N0}");
        return sum;
    }
}
