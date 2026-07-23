public class LevelLoader
{
    private int cellSize;
    private PhysicsSystem physicsSystem; // se guarda una vez, no se repite en cada llamada
    private Dictionary<char, Func<float, float, PhysicalEntity>> factories;

    public LevelLoader(int cellSize, PhysicsSystem physicsSystem)
    {
        this.cellSize = cellSize;
        this.physicsSystem = physicsSystem;
        factories = new Dictionary<char, Func<float, float, PhysicalEntity>>
        {
            ['1'] = (x, y) => new PhysicalEntity(x, y, cellSize, cellSize, false),
            ['@'] = (x, y) => new PhysicalEntity(x, y, cellSize/2, cellSize/2, true),
        };
    }

    public List<PhysicalEntity> CreateChunk(string chunk, int cols, float originX, float originY)
    {
        List<PhysicalEntity> created = new List<PhysicalEntity>();
        int rows = chunk.Length / cols;

        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < cols; col++)
            {
                char c = chunk[row * cols + col];
                if (!factories.ContainsKey(c)) continue;

                float px = originX + col * cellSize;
                float py = originY + row * cellSize;

                var ent = factories[c](px, py);
                physicsSystem.AddEntity(ent);
                created.Add(ent);
            }
        }
        return created;
    }
}