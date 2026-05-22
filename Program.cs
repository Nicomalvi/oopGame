// POR AHORA METO TODO EN 1 ARCHIVO el ultimo proyecto se me fue de las manos y tenia como 20 archivos distintos
public enum Facing
{
    right,
    left
}
public enum State
{
    idle,
    walk,
    fall
}
public enum CollisionType
{
    platform,   // choco con TODO
    character,  // choco con plataformas, otros char.
    item,       // choco solo con plataformas
}
public class MapGrid
{
    private List<PhysicalEntity>[][] grid;
    private int cols;
    private int rows;
    private int cellSize;
    private float width;
    private float height;

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
    }
    private List<PhysicalEntity> GetCellEntities(Position pos)
    {
        (float x, float y) = pos.Vector;
        int col = (int)x/cellSize;
        int row = (int)y/cellSize; // chequeo Out of bounds
        return grid[col][row]; // es una referencia, si modifico se modifica el objeto
    }
    public List<PhysicalEntity> GetCellByIndex(int x, int y)
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
        (float xOffset, float yOffset) = HitboxOffsets;
        (float width, float height) = HitboxDimensions;
        return((int)(x + xOffset)/map.CellSize,(int)(x + xOffset + width)/map.CellSize,
               (int)(y + yOffset)/map.CellSize,(int)(y + yOffset + height)/map.CellSize);
    }
    public List<PhysicalEntity> HitboxOverlapList()
    {
        // calculo toda (x,y) que ocupa mi hitbox, devuelvo cuales physEnt choca
        (float nx, float ny) = position.Vector;
        (float xOffset, float yOffset) = HitboxOffsets;
        (float width, float height) = hitbox.Dimensions;
        int startX = (int)(nx + xOffset) / map.CellSize;
        int endX   = (int)(nx + xOffset + width) / map.CellSize;
        int startY = (int)(ny + yOffset) / map.CellSize;
        int endY   = (int)(ny + yOffset + height) / map.CellSize;
        List<PhysicalEntity> res = new List<PhysicalEntity>();
        for (int x = startX; x <= endX; x++)
        {
            for (int y = startY; y <= endY; y++)
            {   
                res.AddRange(map.GetCellByIndex(x,y));
            }
        }
        return res;
    }
    private bool PositionInBounds()
    {
        (float x, float y) = Pos.Vector;
        (float maxX, float maxY) = Pos.Bounds;
        (float xOffset, float yOffset) = HitboxOffsets;
        (float width, float height) = HitboxDimensions;
        return x + xOffset + width <= maxX && x + xOffset + width >= 0 &&
               y + yOffset + height <= maxY && y + yOffset + height >= 0;
    }
    public void Move()
    {
        // me voy moviendo de a 1 paso
        // si detecto una colision, chequeo si me hace frenar, chequeo efectos de la colision
        (float vx, float vy) =  MoveVector;
        if(MoveVector == (0,0)){return;}
        map.RemoveEntity(this);

        (float x, float y) =    PosVector;
        (float maxX, float maxY) = Pos.Bounds;

        float maxSteps = Math.Max(Math.Abs(vx), Math.Abs(vy));
        (float stepX, float stepY) = (vx / maxSteps, vy / maxSteps);

        List<PhysicalEntity> entities = new List<PhysicalEntity>();

        for(int i = 0; i < maxSteps; i++)
        {
            // si ya estoy en colision antes de moverme no lo detecto con este sistema
            x += stepX;
            position.Set(x,y);
            entities = HitboxOverlapList();
            //foreach entinty in entities COLISION, luego chequear si me frena dentro del if
            if(!PositionInBounds() || entities.Exists(CollisionStopsMovement))
            {
                x -= stepX;
                position.Set(x,y);
                stepX = 0;
            }
            // todo igual pero ahora con y
            y += stepY;
            position.Set(x,y);
            entities = HitboxOverlapList();
            if(!PositionInBounds() || entities.Exists(CollisionStopsMovement))
            {
                y -= stepY;
                position.Set(x,y);
                stepY = 0;
                //this.Collide(that)
            }
        }
        position.Set(x,y);
        movement.Reset();
        map.AddEntity(this);
    }
    public virtual bool CollisionStopsMovement(PhysicalEntity ent)
    {
        return this!=ent; // toda PhysEnt por defecto frena al chocar con cualquier entidad
    }                     // distintos hijos tendran overRide de este metodo
    private bool StandingOnPlatform()
    {
        // chequeo: si mi pos. fuera 1 pixel abajo...
        // choco con alguien? y si choco, mi parte inferior esta arriba de su parte superior?
        // entonces estoy parado encima.
        (float x, float y) = PosVector;
        position.Set(x,y+1);
        List<PhysicalEntity> collisionList = new List<PhysicalEntity>();
        collisionList = HitboxOverlapList();
        bool res = collisionList.Exists(ent => CollisionStopsMovement(ent) && 
                                               HitboxBottom <= ent.HitboxTop); // Y CRECE PARA ABAJO !
        position.Set(x,y);
        return res;
    }
    public void Update()
    {
        // continuar animacion...
        // ticks de veneno... (quizas no va esto aca ahora que hice hijos de la clase)
    }
    public void PrintVertices()
    {
        (float x, float y) = position.Vector;
        (float xOffset, float yOffset) = HitboxOffsets;
        (float width, float height) = HitboxDimensions;

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

    public (float width, float height) HitboxDimensions => hitbox.Dimensions;
    public (float offsetX, float offsetY) HitboxOffsets => hitbox.Offsets;
    public float HitboxTop    => PosVector.Y + HitboxOffsets.offsetY;
    public float HitboxBottom => PosVector.Y + HitboxOffsets.offsetY + HitboxDimensions.height;
    public float HitboxLeft   => PosVector.X + HitboxOffsets.offsetX;
    public float HitboxRight  => PosVector.X + HitboxOffsets.offsetX + HitboxDimensions.width;
}
public class Character : PhysicalEntity
{
    private string name;
    private bool hasGravity;
    public Character(float x, float y, MapGrid map, float width, float height,
                     string name, bool hasGravity = true) 
                     : base(x,y,map,width,height) // llamo al constr. de PhysEnt
    {
        this.name = name;
        this.hasGravity = hasGravity;
    }
    public override bool CollisionStopsMovement(PhysicalEntity ent)
    {
        return ent!=this && (ent is Character || ent is Platform);
    }
}
public class Platform : PhysicalEntity
{
    public Platform(float x, float y, MapGrid map, float width, float height) : base(x,y,map,width,height){}
    // en un futuro: mi move hara que se muevan los de arriba mio, etc...
}
public class Program
{
    const int CELL_SIZE = 32;
    public static void Main()
    {
        int width = 600;
        int height = 200;
        MapGrid map = new MapGrid(width,height,CELL_SIZE);
        PhysicalEntity player = new Character(200, 140, map, 32, 32, "nico");
        PhysicalEntity box = new Platform(303,160, map, 16, 16);
        while (true)
        {
            if (Console.KeyAvailable)
            {
                var key = Console.ReadKey(true).Key;
                if (key == ConsoleKey.W) player.AddMovement(new Movement(0,-32)); // arriba a abajo el eje y
                if (key == ConsoleKey.S) player.AddMovement(new Movement(0,32));
                if (key == ConsoleKey.A) player.AddMovement(new Movement(-32,0));
                if (key == ConsoleKey.D) player.AddMovement(new Movement(32,0));
                if (key == ConsoleKey.F) player.AddMovement(new Movement(0,0));
                player.Move();
                map.PrintMap(player);
                player.PrintVertices();
                Console.WriteLine("ahora la box");
                box.PrintVertices();
            }
        }
    }
}