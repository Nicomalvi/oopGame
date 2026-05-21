public class MapGrid
{
    private List<PhysicalEntity?>[][] grid;
    private int cols;
    private int rows;
    private int cellSize;
    private float width;
    private float height;

    public MapGrid(int width, int height, int cellSize)
    {
        this.cellSize = cellSize;
        this.width = width;
        this.height = height;
        cols = width / cellSize;
        rows = height / cellSize;
        grid = new List<PhysicalEntity?>[cols][];
        for (int i = 0; i < cols; i++){
            grid[i] = new List<PhysicalEntity?>[rows];
            for (int j = 0; j < rows; j++){
                grid[i][j] = new List<PhysicalEntity?>();
            } 
        }  
    }
    public List<PhysicalEntity?> GetCellEntities(Position pos)
    {
        (float x, float y) = pos.Vector;
        int col = (int)x/cellSize;
        int row = (int)y/cellSize; // chequeo Out of bounds
        return grid[col][row]; // es una referencia, si modifico se modifica el objeto
    }
    public List<PhysicalEntity?> GetCellByIndex(int x, int y)
    {
        return grid[x][y];
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
    //public PhysicalEntity?[][] Grid => grid;
    public int Cols => cols;
    public int Rows => rows;
    public int CellSize => cellSize;
    public float Height => height;
    public float Width => width;
}
public class Position
{
    private float x;
    private float y;
    private float maxX;
    private float maxY;
    // solo puedo tocar una posicion mediante sumar velocidades, sino solo lectura
    public Position(float x, float y, float maxX, float maxY)
    {
        this.maxX = maxX;
        this.maxY = maxY;
        this.x = Math.Clamp(x, 0, maxX);
        this.y = Math.Clamp(y, 0, maxY);
    }
    public void MovePos(Movement move)
    {
        (float dx, float dy) = move.Vector;
        x = Math.Clamp(x + dx, 0, maxX);
        y = Math.Clamp(y + dy, 0, maxY);
    }
    public void Set(float dx, float dy)
    {
        x = Math.Clamp(dx, 0, maxX);
        y = Math.Clamp(dy, 0, maxY);
    }
    public (float X, float Y) Vector => (x, y);
    public (float x, float y) Bounds => (maxX, maxY);
}
public class Movement
{
    private float vx;
    private float vy;
    public Movement(float vx = 0, float vy = 0)
    {
        this.vx = vx;
        this.vy = vy;
    }
    public void Add(Movement movement)
    {
        (float dx, float dy) = movement.Vector;
        vx += dx;
        vy += dy;
    }
    public (float VX, float VY) Vector => (vx, vy);
    public void Reset() => (vx, vy) = (0, 0);
}
public class Hitbox
{
    // Pienso....
    // mapa de lugares que ocupan hitvoxes?
    // problema ejemplo:
    // A esta en 50,50
    // B en 0,0 pero ocupa toda la pantalla
    // cuando A se mueve a 51,50 tal vez no detecta colision pero en realidad si la hay
    private float xOffset;
    private float yOffset;
    private float width;
    private float height;
    public Hitbox(float width, float height, float xOffset = 0, float yOffset = 0)
    {
        this.width = width;
        this.height = height;
        this.xOffset = xOffset;
        this.yOffset = yOffset;
    }
    public (float x,float y) Offsets => (xOffset, yOffset);
    public (float x, float y) Dimensions => (width, height);
}
public class PhysicalEntity
{
    private Position position;
    private Movement movement;
    private Hitbox hitbox;
    private MapGrid map;
    public PhysicalEntity(float x, float y, MapGrid map, float width, float height)
    {
        this.map = map;
        movement = new Movement();
        position = new Position(x, y, map.Width - 1, map.Height - 1);
        hitbox = new Hitbox(width, height);
        this.map.AddEntity(this);
    }
    public void AddMovement(Movement newMove)
    {
        movement.Add(newMove);
    }
    public (int,int,int,int) GetHitboxMapCells()
    {
        // puede devolver out of bounds !!! 
        (float x, float y) = position.Vector;
        (float xOffset, float yOffset) = hitbox.Offsets;
        (float width, float height) = hitbox.Dimensions;
        return((int)(x + xOffset)/map.CellSize,(int)(x + xOffset + width)/map.CellSize,
               (int)(y + yOffset)/map.CellSize,(int)(y + yOffset + height)/map.CellSize);
    }
    public bool NewPositionOverlapsEntity(Position pos)
    {
        (float nx, float ny) = pos.Vector;
        (float xOffset, float yOffset) = hitbox.Offsets;
        (float width, float height) = hitbox.Dimensions;
        int startCol = (int)(nx + xOffset) / map.CellSize;
        int endCol   = (int)(nx + xOffset + width) / map.CellSize;
        int startRow = (int)(ny + yOffset) / map.CellSize;
        int endRow   = (int)(ny + yOffset + height) / map.CellSize;
        Position tempPosition = new Position(0,0,map.Width-1,map.Height-1);
        for (int col = startCol; col <= endCol; col++)
        {
            for (int row = startRow; row <= endRow; row++)
            {   
                tempPosition.Set(col,row);
                if (map.GetCellByIndex(col,row).Count != 0){return true;}
            }
        }
        return false;
    }
    public bool NewPositionInBounds(Position newPos)
    {
        (float x, float y) = newPos.Vector;
        (float maxX, float maxY) = newPos.Bounds;
        (float xOffset, float yOffset) = hitbox.Offsets;
        (float width, float height) = hitbox.Dimensions;
        return x + xOffset + width <= maxX && x + xOffset + width >= 0 &&
               y + yOffset + height <= maxY && y + yOffset + height >= 0;
    }
    public void Move()
    {
        (float vx, float vy) =  MoveVector;
        if(MoveVector == (0,0)){return;}
        map.RemoveEntity(this);
        (float x, float y) =    PosVector;
        float maxSteps = Math.Max(Math.Abs(vx), Math.Abs(vy));
        (float stepX, float stepY) = (vx / maxSteps, vy / maxSteps);
        Position tempPos = new Position(x,y,map.Width-1,map.Height-1);
        for(int i = 0; i < maxSteps; i++)
        {
            // si ya estoy en colision antes de moverme se rompe
            x += stepX;
            tempPos.Set(x,y);
            if(!NewPositionInBounds(tempPos) || NewPositionOverlapsEntity(tempPos))
            {
                x -= stepX;
                tempPos.Set(x,y);
                stepX = 0;
            }
            y += stepY;
            tempPos.Set(x,y);
            if(!NewPositionInBounds(tempPos) || NewPositionOverlapsEntity(tempPos))
            {
                y -= stepY;
                tempPos.Set(x,y);
                stepY = 0;
            }
        }
        position.Set(x,y);
        movement.Reset();
        map.AddEntity(this);
    }
    public void Update()
    {
        Move();
        // otras cosas
    }
    public void PrintVertices()
    {
        (float x, float y) = position.Vector;
        (float xOffset, float yOffset) = hitbox.Offsets;
        (float width, float height) = hitbox.Dimensions;

        float left   = x + xOffset;
        float top    = y + yOffset;
        float right  = left + width;
        float bottom = top + height;

        Console.WriteLine("v1 " + (left,  top));     // top-left
        Console.WriteLine("v2 " + (right, top));     // top-right
        Console.WriteLine("v3 " + (left,  bottom));  // bottom-left
        Console.WriteLine("v4 " + (right, bottom));  // bottom-right
    }
    public Position Pos => position;
    public (float X,float Y) PosVector => position.Vector;
    public (float VX, float VY) MoveVector => movement.Vector;
}
public class Program
{
    const int CELL_SIZE = 20;
    public static void Main()
    {
        int width = 600;
        int height = 200;
        MapGrid map = new MapGrid(width,height,CELL_SIZE);
        PhysicalEntity player = new PhysicalEntity(200, 140, map, 32, 32);
        PhysicalEntity box = new PhysicalEntity(303,160, map, 32, 32);
        while (true)
        {
            if (Console.KeyAvailable)
            {
                var key = Console.ReadKey(true).Key;
                if (key == ConsoleKey.W) player.AddMovement(new Movement(0,-25)); // arriba a abajo el eje y
                if (key == ConsoleKey.S) player.AddMovement(new Movement(0,25));
                if (key == ConsoleKey.A) player.AddMovement(new Movement(-25,0));
                if (key == ConsoleKey.D) player.AddMovement(new Movement(25,0));
                player.Update();
                map.PrintMap(player);
                player.PrintVertices();
                Console.WriteLine("ahora la box");
                box.PrintVertices();
            }
        }
    }
}