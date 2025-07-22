using FSCPU;
using System.Diagnostics;
using System.Threading;

namespace FSCPU.Cycles;

/// <summary>
/// Adaptive clock manager that automatically adjusts timing for CPU emulation
/// Supports multiple modes: real-time, fast, stepped, and limited frequency
/// Replaces the old timer-based approach with high-precision thread-based timing
/// </summary>
public class ClockManager : IDisposable
{
    /// <summary>
    /// Clock execution modes
    /// </summary>
    public enum ClockMode
    {
        RealTime,    // Try to match real CPU frequency with high precision
        Fast,        // Run as fast as possible (no timing constraints)
        Stepped,     // Manual step-by-step execution for debugging
        Limited      // Limited frequency for controlled debugging
    }
    
    // Default target frequencies for different modes
    public const long DEFAULT_REALTIME_FREQUENCY = 4_000_000; // 4 MHz
    public const long DEFAULT_LIMITED_FREQUENCY = 100_000;    // 100 KHz for debugging
    
    private readonly CPU8Bit _cpu;
    private ClockMode _mode = ClockMode.Limited;
    private long _targetFrequency = DEFAULT_LIMITED_FREQUENCY;
    
    // Thread management
    private Thread? _clockThread;
    private volatile bool _isRunning;
    private volatile bool _shouldStop;
    
    // Performance tracking
    private long _cyclesExecuted;
    private readonly Stopwatch _totalTimer = new();
    private readonly Stopwatch _frequencyTimer = new();
    private long _lastFrequencyCheck;
    
    // Compatibility properties for existing code
    public int RemainingCycles => 0; // Not applicable in continuous mode
    
    public ClockManager(CPU8Bit cpu)
    {
        _cpu = cpu ?? throw new ArgumentNullException(nameof(cpu));
    }
    
    /// <summary>
    /// Set clock mode and optional target frequency
    /// </summary>
    /// <param name="mode">Execution mode</param>
    /// <param name="targetFrequency">Target frequency (optional, uses defaults if 0)</param>
    public void SetMode(ClockMode mode, long targetFrequency = 0)
    {
        bool wasRunning = _isRunning;
        
        // Stop if running
        if (_isRunning)
        {
            Stop();
        }
        
        _mode = mode;
        
        // Set target frequency based on mode if not specified
        if (targetFrequency > 0)
        {
            _targetFrequency = targetFrequency;
        }
        else
        {
            _targetFrequency = mode switch
            {
                ClockMode.RealTime => DEFAULT_REALTIME_FREQUENCY,
                ClockMode.Limited => DEFAULT_LIMITED_FREQUENCY,
                _ => DEFAULT_LIMITED_FREQUENCY
            };
        }
        
        // Only log mode changes if target frequency is high (to avoid test spam)
        if (_targetFrequency >= 1_000_000)
        {
            Console.WriteLine($"Clock mode set: {mode}, Target: {_targetFrequency:N0} Hz");
        }
        
        // Restart if it was running
        if (wasRunning)
        {
            Start();
        }
    }
    
    /// <summary>
    /// Start the clock manager
    /// </summary>
    public void Start()
    {
        if (_isRunning) return;
        
        _isRunning = true;
        _shouldStop = false;
        _cyclesExecuted = 0;
        _lastFrequencyCheck = 0;
        _totalTimer.Restart();
        _frequencyTimer.Restart();
        
        // For stepped mode, don't start a thread
        if (_mode == ClockMode.Stepped)
        {
            // Silent for stepped mode
            return;
        }
        
        // Start execution thread with appropriate priority
        ThreadPriority priority = _mode == ClockMode.RealTime 
            ? ThreadPriority.AboveNormal 
            : ThreadPriority.Normal;
            
        _clockThread = new Thread(ClockLoop)
        {
            Name = "FS8-Adaptive-Clock",
            IsBackground = true,
            Priority = priority
        };
        
        _clockThread.Start();
        
        // Only log for high-frequency modes
        if (_targetFrequency >= 1_000_000)
        {
            Console.WriteLine($"Clock thread started in {_mode} mode");
        }
    }
    
    /// <summary>
    /// Stop the clock manager
    /// </summary>
    public void Stop()
    {
        _shouldStop = true;
        _isRunning = false;
        _totalTimer.Stop();
        _frequencyTimer.Stop();
        
        // Wait for thread to finish
        if (_clockThread != null)
        {
            _clockThread.Join(1000); // Wait max 1 second
            _clockThread = null;
        }
        
        // Only log for high-frequency modes or debugging
        if (_targetFrequency >= 1_000_000 || _cyclesExecuted > 100_000)
        {
            Console.WriteLine($"Clock stopped. Total cycles executed: {_cyclesExecuted:N0}");
        }
    }
    
    /// <summary>
    /// Execute single step (for stepped mode or manual debugging)
    /// </summary>
    public void Step()
    {
        if (_cpu.IsRunning)
        {
            _cpu.ExecuteCycleWithTiming();
            _cyclesExecuted++;
        }
    }
    
    /// <summary>
    /// Main clock execution loop running in dedicated thread
    /// </summary>
    private void ClockLoop()
    {
        try
        {
            switch (_mode)
            {
                case ClockMode.Fast:
                    FastClockLoop();
                    break;
                    
                case ClockMode.RealTime:
                    PrecisionClockLoop();
                    break;
                    
                case ClockMode.Limited:
                    LimitedClockLoop();
                    break;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Clock thread error: {ex.Message}");
            _cpu.Stop();
        }
        finally
        {
            _isRunning = false;
        }
    }
    
    /// <summary>
    /// Run as fast as possible (no timing constraints)
    /// </summary>
    private void FastClockLoop()
    {
        while (!_shouldStop && _cpu.IsRunning)
        {
            _cpu.ExecuteCycleWithTiming();
            _cyclesExecuted++;
            
            // Yield occasionally to prevent 100% CPU usage
            if (_cyclesExecuted % 10000 == 0)
            {
                Thread.Yield();
            }
        }
    }
    
    /// <summary>
    /// High-precision timing for real-time emulation using Stopwatch
    /// </summary>
    private void PrecisionClockLoop()
    {
        var stopwatch = Stopwatch.StartNew();
        
        // Calculate ticks per cycle for high precision
        long ticksPerCycle = Stopwatch.Frequency / _targetFrequency;
        long nextCycleTime = ticksPerCycle;
        
        // Only log timing details for debugging high-frequency modes
        if (_targetFrequency >= 1_000_000)
        {
            Console.WriteLine($"Precision timing: {Stopwatch.Frequency:N0} ticks/sec, {ticksPerCycle} ticks/cycle");
        }
        
        while (!_shouldStop && _cpu.IsRunning)
        {
            // Execute CPU cycle
            _cpu.ExecuteCycleWithTiming();
            _cyclesExecuted++;
            
            // Wait for precise timing
            WaitUntilTime(stopwatch, nextCycleTime);
            nextCycleTime += ticksPerCycle;
            
            // Reset stopwatch periodically to avoid overflow (every 10 seconds)
            if (nextCycleTime > Stopwatch.Frequency * 10)
            {
                stopwatch.Restart();
                nextCycleTime = ticksPerCycle;
            }
        }
        
        stopwatch.Stop();
    }
    
    /// <summary>
    /// Limited frequency for debugging using Thread.Sleep
    /// </summary>
    private void LimitedClockLoop()
    {
        // Calculate sleep time in milliseconds
        int sleepMs = Math.Max(1, (int)(1000.0 / _targetFrequency));
        
        // Only log for debugging purposes
        if (_targetFrequency >= 1_000_000)
        {
            Console.WriteLine($"Limited timing: {sleepMs}ms between cycles");
        }
        
        while (!_shouldStop && _cpu.IsRunning)
        {
            _cpu.ExecuteCycleWithTiming();
            _cyclesExecuted++;
            
            if (sleepMs > 0)
            {
                Thread.Sleep(sleepMs);
            }
        }
    }
    
    /// <summary>
    /// Wait until specific time with high precision
    /// Uses combination of Thread.Sleep and SpinWait for accuracy
    /// </summary>
    private static void WaitUntilTime(Stopwatch stopwatch, long targetTicks)
    {
        long currentTicks = stopwatch.ElapsedTicks;
        long remainingTicks = targetTicks - currentTicks;
        
        if (remainingTicks <= 0) return; // Already past target time
        
        // For longer waits (>1ms), use Thread.Sleep to avoid burning CPU
        if (remainingTicks > Stopwatch.Frequency / 1000) // > 1ms
        {
            int sleepMs = (int)((remainingTicks - Stopwatch.Frequency / 2000) * 1000 / Stopwatch.Frequency);
            if (sleepMs > 0)
            {
                Thread.Sleep(sleepMs);
            }
        }
        
        // Spin-wait for final precision (last ~0.5ms)
        while (stopwatch.ElapsedTicks < targetTicks)
        {
            Thread.SpinWait(1);
        }
    }
    
    /// <summary>
    /// Get actual CPU frequency based on execution
    /// </summary>
    public double GetActualFrequency()
    {
        // Update frequency calculation every second
        if (_frequencyTimer.ElapsedMilliseconds >= 1000)
        {
            long currentCycles = _cyclesExecuted;
            long cyclesDelta = currentCycles - _lastFrequencyCheck;
            
            if (_frequencyTimer.ElapsedMilliseconds > 0)
            {
                double frequency = cyclesDelta * 1000.0 / _frequencyTimer.ElapsedMilliseconds;
                _lastFrequencyCheck = currentCycles;
                _frequencyTimer.Restart();
                return frequency;
            }
        }
        
        // Fallback calculation
        if (_totalTimer.ElapsedMilliseconds == 0) return 0;
        return _cyclesExecuted * 1000.0 / _totalTimer.ElapsedMilliseconds;
    }
    
    /// <summary>
    /// Add instruction cycles (compatibility method - not used in adaptive mode)
    /// </summary>
    public void AddInstructionCycles(int cycles)
    {
        // In adaptive mode, cycles are handled automatically
        // by ExecuteCycleWithTiming and InstructionCycles class
        // This method is kept for compatibility with existing code
    }
    
    /// <summary>
    /// Get comprehensive clock information for debugging
    /// </summary>
    public string GetClockInfo()
    {
        var actual = GetActualFrequency();
        var accuracy = _targetFrequency > 0 ? (actual / _targetFrequency * 100) : 0;
        
        return $"Adaptive Clock Manager:\n" +
               $"  Mode: {_mode}\n" +
               $"  Target: {_targetFrequency:N0} Hz\n" +
               $"  Actual: {actual:N0} Hz\n" +
               $"  Accuracy: {accuracy:F1}%\n" +
               $"  Cycles executed: {_cyclesExecuted:N0}\n" +
               $"  Runtime: {_totalTimer.ElapsedMilliseconds:N0} ms\n" +
               $"  Thread status: {(_clockThread?.IsAlive == true ? "Running" : "Stopped")}\n" +
               $"  Status: {(_isRunning ? "Running" : "Stopped")}";
    }
    
    /// <summary>
    /// Get current execution mode
    /// </summary>
    public ClockMode Mode => _mode;
    
    /// <summary>
    /// Get target frequency
    /// </summary>
    public long TargetFrequency => _targetFrequency;
    
    /// <summary>
    /// Get total cycles executed
    /// </summary>
    public long TotalCyclesExecuted => _cyclesExecuted;
    
    /// <summary>
    /// Check if clock is running
    /// </summary>
    public bool IsRunning => _isRunning;
    
    public void Dispose()
    {
        Stop();
        _totalTimer?.Stop();
        _frequencyTimer?.Stop();
    }
}
