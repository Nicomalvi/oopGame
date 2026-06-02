public class MapGrid
// separo todos los pixeles en celdas, asi detectar colisiones no chequea pixel por pixel
// ademas guardo informacion sobre como crear PhysEnt básicas
{
    private List<PhysicalEntity>[][] grid;
    private int cols;
    private int rows;
    private int cellSize;
    private float width;
    private float height;
    private Dictionary<char, Func<float, float, MapGrid, PhysicalEntity>> factories;

    public MapGrid(int width, int height, int cellSize)
    {
        width++;
        height++;
        this.cellSize = cellSize;
        this.width = width;
        this.height = height;
        // redondeo para arriba, tienen que "sobrar" tiles, no restar
        // aunque haya tiles de mas, position sabe el techo verdadero
        cols = (int)Math.Ceiling((float)width / cellSize);
        rows = (int)Math.Ceiling((float)height / cellSize);
        grid = new List<PhysicalEntity>[cols][];
        for (int i = 0; i < cols; i++){
            grid[i] = new List<PhysicalEntity>[rows];
            for (int j = 0; j < rows; j++){
                grid[i][j] = new List<PhysicalEntity>();
            } 
        }  
        factories = new Dictionary<char, Func<float, float, MapGrid, PhysicalEntity>>
        {
            // pared, hitbox tamaño cellSize x cellSize
            ['1'] = (x, y, m) => new PhysicalEntity(x, y, m, m.CellSize, m.CellSize, false),
            // nico, hitbox tamaño cellSize/2 x cellSize/2
            ['@'] = (x, y, m) => new PhysicalEntity(x, y, m, m.CellSize/2, m.CellSize/2, true),
        };
    }
    public List<PhysicalEntity> GetCellByIndex(int x, int y)
    {
        return grid[x][y];
    }
    public List<PhysicalEntity> OccupyingPixel(float x, float y)
    {
        List<PhysicalEntity> cell = GetCellByIndex((int)x/cellSize, (int)y/cellSize);
        List<PhysicalEntity> res = new List<PhysicalEntity>();
        foreach (PhysicalEntity physEnt in cell)
        {
            if(physEnt.HitboxLeft <= x && physEnt.HitboxRight >= x &&
                physEnt.HitboxTop <= y && physEnt.HitboxBottom >= y)
            {
                res.Add(physEnt);
            }
        }
        return res;
    }
    public void AddEntity(PhysicalEntity ent)
    {
        (int x,int endX,int y,int endY) = ent.GetHitboxMapCells();
        for(int i = x; i<=endX; i++)
        {
            for(int j = y; j<=endY; j++)
            {
                grid[i][j].Add(ent);
            }
        }
    }
    public void RemoveEntity(PhysicalEntity ent)
    {
        (int x,int endX,int y,int endY) = ent.GetHitboxMapCells();
        for(int i = x; i<=endX; i++)
        {
            for(int j = y; j<=endY; j++)
            {
                grid[i][j].Remove(ent);
            }
        }
    }
    public void PrintMap(PhysicalEntity player)
    {
        for (int row = 0; row < rows; row++)
        {
            string buffer = "";
            for (int col = 0; col < cols; col++)
                buffer += grid[col][row].Count == 0 ? " - " : " @ ";
            Console.WriteLine(buffer);
        }
        (float x, float y) = player.PosVector;
        Console.WriteLine("(" + x + ", " + y + ")");
    }
    public List<PhysicalEntity> CreateChunks(string[][] chunks, int rowOffset, int colOffset, int chunkRows, int chunkCols)
    {
        int rows = chunks.Count();
        int cols = chunks[0].Count();
        List<PhysicalEntity> created = new List<PhysicalEntity>();
        for(int row = 0; row < rows; row++)
        {
            for(int col = 0; col<cols; col++)
            {
                string currentChunk = chunks[row][col];
                for(int chunkRow = 0; chunkRow<chunkRows; chunkRow++)
                {
                    for(int chunkCol = 0; chunkCol<chunkCols; chunkCol++)
                    {
                        char c = currentChunk[chunkRow * chunkCols + chunkCol];
                        // raro porque uso 1 string en vez de matriz de chars
                        // basicamente dice: para llegar a row actual salteo todos los caracteres
                        // hasta fila actual * cantidad columnas

                        float px = ((colOffset + col) * chunkCols + chunkCol) * cellSize;
                        float py = ((rowOffset + row) * chunkRows + chunkRow) * cellSize;
                        if (factories.ContainsKey(c))
                        {
                            created.Add(factories[c](px, py, this));
                        }
                    }
                }
            }
        }
        return created;
    }
    //public PhysicalEntity?[][] Grid => grid;
    public int Cols => cols;
    public int Rows => rows;
    public int CellSize => cellSize;
    public float Height => height;
    public float Width => width;
}