using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Linq;
using System.Threading;

namespace ParallelSum
{
    public class ParallelArraySumCalculator
    {
        private int[] array;
        private int activeLength;
        private int workerThreadCount;
        private BlockingCollection<int> taskQueue;
        private CountdownEvent waveComplete;
        private Thread[] workerThreads;
        private volatile bool shouldStop = false;

        public ParallelArraySumCalculator(int[] initialArray, int? threadCount = null)
        {
            array = (int[])initialArray.Clone();
            activeLength = array.Length;

            workerThreadCount = threadCount ?? Math.Min(
                Environment.ProcessorCount,
                activeLength / 2
            );

            if (workerThreadCount < 1) workerThreadCount = 1;

            taskQueue = new BlockingCollection<int>(new ConcurrentQueue<int>());
            workerThreads = new Thread[workerThreadCount];

            Console.WriteLine($"Ініціалізація з {workerThreadCount} робочими потоками");
            Console.WriteLine($"Доступно ядер процесора: {Environment.ProcessorCount}");
        }

        public int CalculateSum()
        {
            var stopwatch = Stopwatch.StartNew();

            Console.WriteLine($"Довжина масиву: {array.Length}");
            
            StartWorkerThreads();

            int waveNumber = 1;

            while (activeLength > 1)
            {
                int pairsCount = activeLength / 2;
                bool hasMiddleElement = activeLength % 2 == 1;

                Console.WriteLine($"Хвиля {waveNumber}: обробка {pairsCount} пар...");

                // лічильник на кількість пар
                waveComplete = new CountdownEvent(pairsCount);

                for (int i = 0; i < pairsCount; i++)
                {
                    taskQueue.Add(i);
                }

                // очікуємо завершення обробки всіх пар у цій хвилі
                waveComplete.Wait();

                activeLength = pairsCount + (hasMiddleElement ? 1 : 0);
                waveNumber++;
            }

            StopWorkerThreads();

            stopwatch.Stop();

            Console.WriteLine($"\n=== Результат ===");
            Console.WriteLine($"Сума елементів масиву: {array[0]}");
            Console.WriteLine($"Час виконання: {stopwatch.ElapsedMilliseconds} мс");
            Console.WriteLine($"Кількість хвиль: {waveNumber - 1}");

            return array[0];
        }

        private void StartWorkerThreads()
        {
            for (int i = 0; i < workerThreadCount; i++)
            {
                int threadId = i;
                workerThreads[i] = new Thread(() => WorkerThreadProc(threadId))
                {
                    Name = $"Worker-{threadId}",
                    IsBackground = false
                };
                workerThreads[i].Start();
            }
        }

        private void WorkerThreadProc(int threadId)
        {
            while (!shouldStop)
            {
                try
                {
                    if (taskQueue.TryTake(out int leftIndex, 100))
                    {
                        ProcessPair(leftIndex);
                    }
                }
                catch (InvalidOperationException)
                {
                    break;
                }
            }
        }

        private void ProcessPair(int leftIndex)
        {
            int rightIndex = activeLength - 1 - leftIndex;

            array[leftIndex] = array[leftIndex] + array[rightIndex];

            waveComplete.Signal();
        }

        private void StopWorkerThreads()
        {
            shouldStop = true;
            taskQueue.CompleteAdding(); // Забороняємо додавання нових завдань

            // Чекаємо коректного завершення всіх потоків
            foreach (var thread in workerThreads)
            {
                thread.Join();
            }

            taskQueue.Dispose();
        }
    }
}
