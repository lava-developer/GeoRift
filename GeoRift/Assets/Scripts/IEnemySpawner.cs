public interface IEnemySpawner
{
    int CurrentWave { get; }
    void EnemyDeath();
    void Disable();
}