using System.Diagnostics;
using FSCPU;
using FSCPU.Cycles;

// Demonstrate the new Adaptive Clock Manager capabilities

Console.WriteLine("=== FS8 CPU Emulator - Adaptive Clock Manager Demo ===\n");

// Create CPU with 64KB memory
var memory = new Memory(0x10000);
var cpu = new CPU8Bit(memory);

// Load a simple test program
byte[] testProgram = {
    0x10, 50,        // LDA #50
    0x11, 25,        // LDB #25  
    0x20,            // ADD A,B  (A = 75)
    0x50, 0x00, 0x20, // STA $2000
    0x29,            // DEC A    (A = 74)
    0x2B,            // DEC B    (B = 24)  
    0x01             // HALT
};

memory.LoadProgram(testProgram);
cpu.PC = 0x0000; // Start at program beginning

Console.WriteLine("Test Program Loaded:");
Console.WriteLine("  LDA #50      ; Load 50 into A");
Console.WriteLine("  LDB #25      ; Load 25 into B"); 
Console.WriteLine("  ADD A,B      ; Add B to A (A = 75)");
Console.WriteLine("  STA $2000    ; Store A at address 0x2000");
Console.WriteLine("  DEC A        ; Decrement A (A = 74)");
Console.WriteLine("  DEC B        ; Decrement B (B = 24)");
Console.WriteLine("  HALT         ; Stop execution\n");

// Demo 1: Stepped Mode (manual control)
Console.WriteLine("=== Demo 1: Stepped Mode (Manual Step-by-Step) ===");
cpu.Reset();
cpu.PC = 0x0000;
cpu.StartWithMode(ClockManager.ClockMode.Stepped);

Console.WriteLine($"Initial state: A={cpu.A}, B={cpu.B}, PC=0x{cpu.PC:X4}");

for (int step = 1; step <= 7 && cpu.IsRunning; step++)
{
    Console.WriteLine($"Step {step}:");
    cpu.ExecuteStep();
    Console.WriteLine($"  A={cpu.A}, B={cpu.B}, PC=0x{cpu.PC:X4}, Running={cpu.IsRunning}");
}

cpu.Stop();
Console.WriteLine($"Final memory at 0x2000: {memory.ReadByte(0x2000)}");
Console.WriteLine();

// Demo 2: Fast Mode (no timing constraints)
Console.WriteLine("=== Demo 2: Fast Mode (Maximum Speed) ===");
cpu.Reset();
cpu.PC = 0x0000;

var stopwatch = Stopwatch.StartNew();
cpu.StartWithMode(ClockManager.ClockMode.Fast);

// Wait for completion or timeout
while (cpu.IsRunning && stopwatch.ElapsedMilliseconds < 100)
{
    Thread.Sleep(1);
}

stopwatch.Stop();
cpu.Stop();

var (cps, ips) = cpu.GetPerformanceMetrics();
Console.WriteLine($"Execution completed in {stopwatch.ElapsedMilliseconds} ms");
Console.WriteLine($"Final state: A={cpu.A}, B={cpu.B}, PC=0x{cpu.PC:X4}");
Console.WriteLine($"Performance: {ips:N0} IPS, {cps:N0} CPS");
Console.WriteLine($"Total instructions: {cpu.TotalInstructionsExecuted}");
Console.WriteLine($"Total cycles: {cpu.TotalCyclesExecuted}");
Console.WriteLine();

// Demo 3: Limited Mode (controlled frequency)
Console.WriteLine("=== Demo 3: Limited Mode (1000 Hz) ===");
cpu.Reset();
cpu.PC = 0x0000;

stopwatch.Restart();
cpu.StartWithMode(ClockManager.ClockMode.Limited, 1000); // 1kHz

// Wait for completion or timeout
while (cpu.IsRunning && stopwatch.ElapsedMilliseconds < 1000)
{
    Thread.Sleep(10);
}

stopwatch.Stop();
cpu.Stop();

var (cps2, ips2) = cpu.GetPerformanceMetrics();
Console.WriteLine($"Execution completed in {stopwatch.ElapsedMilliseconds} ms");
Console.WriteLine($"Final state: A={cpu.A}, B={cpu.B}, PC=0x{cpu.PC:X4}");
Console.WriteLine($"Target frequency: 1000 Hz");
Console.WriteLine($"Actual performance: {ips2:N0} IPS, {cps2:N0} CPS");
Console.WriteLine($"Frequency accuracy: {(cps2/1000)*100:F1}%");
Console.WriteLine();

// Demo 4: Clock Manager Information
Console.WriteLine("=== Clock Manager Information ===");
Console.WriteLine(cpu.Clock.GetClockInfo());
Console.WriteLine();

// Demo 5: High-precision mode (brief demo)
Console.WriteLine("=== Demo 4: Real-Time Mode (High Precision) ===");
cpu.Reset(); 
cpu.PC = 0x0000;

stopwatch.Restart();
cpu.StartWithMode(ClockManager.ClockMode.RealTime, 100_000); // 100kHz

// Brief execution
while (cpu.IsRunning && stopwatch.ElapsedMilliseconds < 200)
{
    Thread.Sleep(5);
}

stopwatch.Stop();
cpu.Stop();

var (cps3, ips3) = cpu.GetPerformanceMetrics();
Console.WriteLine($"Brief execution in {stopwatch.ElapsedMilliseconds} ms");
Console.WriteLine($"Target: 100,000 Hz, Actual: {cps3:N0} CPS");
Console.WriteLine($"Precision accuracy: {(cps3/100000)*100:F1}%");
Console.WriteLine();

Console.WriteLine("=== Performance Comparison ===");
Console.WriteLine($"Fast Mode:      {cps:N0} CPS (unlimited)");
Console.WriteLine($"Limited Mode:   {cps2:N0} CPS (target: 1,000)"); 
Console.WriteLine($"Real-Time Mode: {cps3:N0} CPS (target: 100,000)");
Console.WriteLine();

// Cleanup
cpu.Dispose();

Console.WriteLine("=== Demo Complete ===");
Console.WriteLine("The Adaptive Clock Manager provides flexible CPU timing:");
Console.WriteLine("- Stepped: Manual step-by-step debugging");
Console.WriteLine("- Fast: Maximum performance for testing");  
Console.WriteLine("- Limited: Controlled frequency for debugging");
Console.WriteLine("- RealTime: High-precision timing for authentic emulation");

Console.WriteLine("\nPress any key to exit...");
Console.ReadKey();