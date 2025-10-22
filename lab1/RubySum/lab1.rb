require 'java'

java_import java.util.concurrent.Executors
java_import java.util.concurrent.CountDownLatch
java_import java.util.concurrent.atomic.AtomicLong

class ParallelArraySum
  ARRAY_SIZE = 10_000_000

  def initialize
    @array = Java::long[ARRAY_SIZE].new
    random = java.util.Random.new(42)
    ARRAY_SIZE.times { |i| @array[i] = random.nextInt(100) + 1 }
    
    puts "=== Паралельне обчислення суми масиву (JRuby) ===\n\n"
    puts "Розмір масиву: #{format_number(ARRAY_SIZE)} елементів\n\n"
  end

  def run
    thread_counts = [2, 4, 8, 16]

    thread_counts.each do |thread_count|
      puts "--- Кількість потоків: #{thread_count} ---"
      
      sum1 = sum_with_executor_optimized(thread_count)
      sum2 = sum_with_atomic(thread_count)
      sum3 = sum_sequential
      
      puts "Результати однакові: #{sum1 == sum2 && sum2 == sum3}\n\n"
    end
  end

  def sum_with_executor_optimized(thread_count)
    start_time = Time.now
    
    executor = Executors.newFixedThreadPool(thread_count)
    latch = CountDownLatch.new(thread_count)
    results = Java::long[thread_count].new
    chunk_size = ARRAY_SIZE / thread_count

    thread_count.times do |i|
      final_i = i
      start_idx = i * chunk_size
      end_idx = (i == thread_count - 1) ? ARRAY_SIZE : start_idx + chunk_size
      
      executor.execute do
        local_sum = 0
        (start_idx...end_idx).each do |j|
          local_sum += @array[j]
        end
        results[final_i] = local_sum
        latch.count_down
      end
    end

    latch.await
    executor.shutdown
    
    total_sum = 0
    thread_count.times { |i| total_sum += results[i] }
    
    elapsed = ((Time.now - start_time) * 1000).round(2)
    puts "Метод 1 (Executor): #{elapsed} мс, Сума: #{format_number(total_sum)}"
    
    total_sum
  end

  def sum_with_atomic(thread_count)
    start_time = Time.now
    
    atomic_sum = AtomicLong.new(0)
    threads = []
    chunk_size = ARRAY_SIZE / thread_count

    thread_count.times do |i|
      start_idx = i * chunk_size
      end_idx = (i == thread_count - 1) ? ARRAY_SIZE : start_idx + chunk_size

      threads << Thread.new do
        local_sum = 0
        (start_idx...end_idx).each do |j|
          local_sum += @array[j]
        end
        atomic_sum.add_and_get(local_sum)
      end
    end

    threads.each(&:join)
    
    total_sum = atomic_sum.get
    elapsed = ((Time.now - start_time) * 1000).round(2)
    
    puts "Метод 2 (Atomic):   #{elapsed} мс, Сума: #{format_number(total_sum)}"
    total_sum
  end

  def sum_sequential
    start_time = Time.now
    
    sum = 0
    ARRAY_SIZE.times { |i| sum += @array[i] }
    
    elapsed = ((Time.now - start_time) * 1000).round(2)
    
    puts "Послідовно:         #{elapsed} мс, Сума: #{format_number(sum)}"
    sum
  end

  private

  def format_number(num)
    num.to_s.reverse.gsub(/(\d{3})(?=\d)/, '\\1,').reverse
  end
end

calculator = ParallelArraySum.new
calculator.run
