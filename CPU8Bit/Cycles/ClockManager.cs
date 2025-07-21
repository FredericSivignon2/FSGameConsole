using FSCPU;

namespace FSCPU.Cycles;

/// <summary>
/// Gestionnaire d'horloge pour simuler un vrai timing de processeur
/// Chaque instruction a un nombre de cycles spécifique
/// </summary>
public class ClockManager
{
    // Fréquence de base du processeur (en Hz)
    public const long BASE_FREQUENCY = 1_000_000; // 1 MHz (1 million de cycles par seconde)
    
    // Durée d'un cycle en microsecondes (1/1MHz = 1µs)
    public const double CYCLE_DURATION_MICROSECONDS = 1000000.0 / BASE_FREQUENCY;
    
    // Durée d'un cycle en millisecondes pour les timers
    public const double CYCLE_DURATION_MILLISECONDS = CYCLE_DURATION_MICROSECONDS / 1000.0;
    
    private readonly CPU8Bit _cpu;
    private readonly System.Timers.Timer _clockTimer;
    private int _remainingCycles;
    private DateTime _lastCycleTime;
    
    public ClockManager(CPU8Bit cpu)
    {
        _cpu = cpu ?? throw new ArgumentNullException(nameof(cpu));
        
        // Timer haute précision pour simuler l'horloge
        _clockTimer = new System.Timers.Timer(CYCLE_DURATION_MILLISECONDS);
        _clockTimer.Elapsed += OnClockTick;
        _clockTimer.AutoReset = true;
        
        _remainingCycles = 0;
        _lastCycleTime = DateTime.UtcNow;
    }
    
    /// <summary>
    /// Démarre l'horloge du processeur
    /// </summary>
    public void Start()
    {
        _lastCycleTime = DateTime.UtcNow;
        _clockTimer.Start();
    }
    
    /// <summary>
    /// Arrête l'horloge du processeur
    /// </summary>
    public void Stop()
    {
        _clockTimer.Stop();
    }
    
    /// <summary>
    /// Ajoute des cycles d'attente pour une instruction
    /// </summary>
    /// <param name="cycles">Nombre de cycles que prend l'instruction</param>
    public void AddInstructionCycles(int cycles)
    {
        _remainingCycles += cycles;
    }
    
    /// <summary>
    /// Gestionnaire de tick d'horloge - exécute les cycles en attente
    /// </summary>
    private void OnClockTick(object? sender, System.Timers.ElapsedEventArgs e)
    {
        if (_cpu == null || !_cpu.IsRunning)
            return;
            
        // Si on a des cycles en attente, on décrémente
        if (_remainingCycles > 0)
        {
            _remainingCycles--;
            return;
        }
        
        // Sinon, exécuter la prochaine instruction
        try
        {
            _cpu.ExecuteCycleWithTiming();
        }
        catch (Exception ex)
        {
            // En cas d'erreur, arrêter l'horloge
            Stop();
            _cpu.Stop();
            throw new InvalidOperationException($"Erreur d'exécution CPU: {ex.Message}", ex);
        }
    }
    
    /// <summary>
    /// Obtient le nombre de cycles restants en attente
    /// </summary>
    public int RemainingCycles => _remainingCycles;
    
    /// <summary>
    /// Calcule la fréquence réelle du CPU en Hz
    /// </summary>
    public double GetActualFrequency()
    {
        // Calcul basé sur le temps écoulé
        var elapsed = DateTime.UtcNow - _lastCycleTime;
        if (elapsed.TotalSeconds <= 0) return 0;
        
        return 1.0 / elapsed.TotalSeconds;
    }
    
    /// <summary>
    /// Informations de debug sur l'horloge
    /// </summary>
    public string GetClockInfo()
    {
        return $"Clock Manager:\n" +
               $"  Fréquence de base: {BASE_FREQUENCY:N0} Hz\n" +
               $"  Durée cycle: {CYCLE_DURATION_MICROSECONDS:F1} µs\n" +
               $"  Cycles en attente: {_remainingCycles}\n" +
               $"  Fréquence réelle: {GetActualFrequency():F0} Hz\n" +
               $"  État: {(_clockTimer.Enabled ? "Actif" : "Arrêté")}";
    }
    
    public void Dispose()
    {
        _clockTimer?.Dispose();
    }
}
