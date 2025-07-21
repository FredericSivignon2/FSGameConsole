using FSCPU;

namespace FSCPU.Cycles;

/// <summary>
/// Gestionnaire d'horloge pour simuler un vrai timing de processeur
/// Chaque instruction a un nombre de cycles sp�cifique
/// </summary>
public class ClockManager
{
    // Fr�quence de base du processeur (en Hz)
    public const long BASE_FREQUENCY = 1_000_000; // 1 MHz (1 million de cycles par seconde)
    
    // Dur�e d'un cycle en microsecondes (1/1MHz = 1�s)
    public const double CYCLE_DURATION_MICROSECONDS = 1000000.0 / BASE_FREQUENCY;
    
    // Dur�e d'un cycle en millisecondes pour les timers
    public const double CYCLE_DURATION_MILLISECONDS = CYCLE_DURATION_MICROSECONDS / 1000.0;
    
    private readonly CPU8Bit _cpu;
    private readonly System.Timers.Timer _clockTimer;
    private int _remainingCycles;
    private DateTime _lastCycleTime;
    
    public ClockManager(CPU8Bit cpu)
    {
        _cpu = cpu ?? throw new ArgumentNullException(nameof(cpu));
        
        // Timer haute pr�cision pour simuler l'horloge
        _clockTimer = new System.Timers.Timer(CYCLE_DURATION_MILLISECONDS);
        _clockTimer.Elapsed += OnClockTick;
        _clockTimer.AutoReset = true;
        
        _remainingCycles = 0;
        _lastCycleTime = DateTime.UtcNow;
    }
    
    /// <summary>
    /// D�marre l'horloge du processeur
    /// </summary>
    public void Start()
    {
        _lastCycleTime = DateTime.UtcNow;
        _clockTimer.Start();
    }
    
    /// <summary>
    /// Arr�te l'horloge du processeur
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
    /// Gestionnaire de tick d'horloge - ex�cute les cycles en attente
    /// </summary>
    private void OnClockTick(object? sender, System.Timers.ElapsedEventArgs e)
    {
        if (_cpu == null || !_cpu.IsRunning)
            return;
            
        // Si on a des cycles en attente, on d�cr�mente
        if (_remainingCycles > 0)
        {
            _remainingCycles--;
            return;
        }
        
        // Sinon, ex�cuter la prochaine instruction
        try
        {
            _cpu.ExecuteCycleWithTiming();
        }
        catch (Exception ex)
        {
            // En cas d'erreur, arr�ter l'horloge
            Stop();
            _cpu.Stop();
            throw new InvalidOperationException($"Erreur d'ex�cution CPU: {ex.Message}", ex);
        }
    }
    
    /// <summary>
    /// Obtient le nombre de cycles restants en attente
    /// </summary>
    public int RemainingCycles => _remainingCycles;
    
    /// <summary>
    /// Calcule la fr�quence r�elle du CPU en Hz
    /// </summary>
    public double GetActualFrequency()
    {
        // Calcul bas� sur le temps �coul�
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
               $"  Fr�quence de base: {BASE_FREQUENCY:N0} Hz\n" +
               $"  Dur�e cycle: {CYCLE_DURATION_MICROSECONDS:F1} �s\n" +
               $"  Cycles en attente: {_remainingCycles}\n" +
               $"  Fr�quence r�elle: {GetActualFrequency():F0} Hz\n" +
               $"  �tat: {(_clockTimer.Enabled ? "Actif" : "Arr�t�")}";
    }
    
    public void Dispose()
    {
        _clockTimer?.Dispose();
    }
}
