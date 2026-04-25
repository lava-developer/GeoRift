public static class EnemySpawnerRegistry
{
    public static IEnemySpawner Current { get; private set; }

    public static void Register(IEnemySpawner spawner) => Current = spawner;
    public static void Unregister(IEnemySpawner spawner)
    {
        if (Current == spawner) Current = null;
    }
}