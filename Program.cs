// POR AHORA METO TODO EN 1 ARCHIVO el ultimo proyecto se me fue de las manos y tenia como 20 archivos distintos
using Raylib_cs;
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
    private List<PhysicalEntity> GetCellEntities(float x, float y)
    {
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
public class Hitbox
{
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
    private float x, y;
    private float maxX, maxY;
    private float vx, vy;
    private Hitbox hitbox;
    private MapGrid map;
    public PhysicalEntity(float x, float y, MapGrid map, float width, float height)
    {
        this.map = map;
        this.maxX = map.Width - 1;
        this.maxY = map.Height - 1;
        this.x    = Math.Clamp(x, 0, maxX);
        this.y    = Math.Clamp(y, 0, maxY);
        vx = 0; vy = 0;
        hitbox = new Hitbox(width, height);
        this.map.AddEntity(this);
    }
    public void AddVelocity(float vx, float vy)
    {
        this.vx += vx;
        this.vy += vy;
    }
    public void resetVelocity(){
        vx = 0;
        vy = 0;
    }
    public void SetPosition(float nX, float nY){
        x = Math.Clamp(nX,0,maxX);
        y = Math.Clamp(nY,0,maxY);
    }
    public (int,int,int,int) GetHitboxMapCells()
    {
        // puede devolver out of bounds !!! 
        (float xOffset, float yOffset) = HitboxOffsets;
        (float width, float height) = HitboxDimensions;
        return((int)(x + xOffset)/map.CellSize,(int)(x + xOffset + width)/map.CellSize,
               (int)(y + yOffset)/map.CellSize,(int)(y + yOffset + height)/map.CellSize);
    }
    public List<PhysicalEntity> HitboxOverlapList()
    {
        // calculo toda (x,y) que ocupa mi hitbox, devuelvo cuales physEnt choca
        (float xOffset, float yOffset) = HitboxOffsets;
        (float width, float height) = hitbox.Dimensions;
        int startX = (int)(x + xOffset) / map.CellSize;
        int endX   = (int)(x + xOffset + width) / map.CellSize;
        int startY = (int)(y + yOffset) / map.CellSize;
        int endY   = (int)(y + yOffset + height) / map.CellSize;
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
        (float x, float y) = PosVector;
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

        float maxSteps = Math.Max(Math.Abs(vx), Math.Abs(vy));
        (float stepX, float stepY) = (vx / maxSteps, vy / maxSteps);

        List<PhysicalEntity> entities = new List<PhysicalEntity>();

        for(int i = 0; i < maxSteps; i++)
        {
            // si ya estoy en colision antes de moverme no lo detecto con este sistema
            x += stepX;
            SetPosition(x,y);
            entities = HitboxOverlapList();
            //foreach entinty in entities COLISION, luego chequear si me frena dentro del if
            if(!PositionInBounds() || entities.Exists(CollisionStopsMovement))
            {
                x -= stepX;
                SetPosition(x,y);
                stepX = 0;
            }
            // todo igual pero ahora con y
            y += stepY;
            SetPosition(x,y);
            entities = HitboxOverlapList();
            if(!PositionInBounds() || entities.Exists(CollisionStopsMovement))
            {
                y -= stepY;
                SetPosition(x,y);
                stepY = 0;
                //this.Collide(that)
            }
        }
        SetPosition(x,y);
        resetVelocity();
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
        SetPosition(x,y+1);
        List<PhysicalEntity> collisionList = new List<PhysicalEntity>();
        collisionList = HitboxOverlapList();
        bool res = collisionList.Exists(ent => CollisionStopsMovement(ent) && 
                                               HitboxBottom <= ent.HitboxTop); // Y CRECE PARA ABAJO !
        SetPosition(x,y);
        return res;
    }
    public void Update()
    {
        // continuar animacion...
        // ticks de veneno... (quizas no va esto aca ahora que hice hijos de la clase)
    }
    public void PrintVertices()
    {
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
    public virtual void DrawDebug()
    {
        (float x, float y)       = PosVector;
        (float xOff, float yOff) = HitboxOffsets;
        (float w, float h)       = HitboxDimensions;
 
        int rx = (int)(x + xOff);
        int ry = (int)(y + yOff);
        int rw = (int)w;
        int rh = (int)h;
 
        // relleno semitransparente + borde sólido
        Raylib.DrawRectangle(rx, ry, rw, rh,
            Color.White);
        Raylib.DrawRectangleLines(rx, ry, rw, rh, Color.White);
        // coordenadas en la esquina superior izquierda del rect
        Raylib.DrawText($"({rx},{ry})", rx + 2, ry + 2, 8, Color.White);
    }
    public (float X, float Y)   PosVector  => (x, y);
    public (float VX, float VY) MoveVector => (vx, vy);

    public (float width, float height) HitboxDimensions => hitbox.Dimensions;
    public (float offsetX, float offsetY) HitboxOffsets => hitbox.Offsets;
    public float HitboxTop    => y + HitboxOffsets.offsetY;
    public float HitboxBottom => y + HitboxOffsets.offsetY + HitboxDimensions.height;
    public float HitboxLeft   => x + HitboxOffsets.offsetX;
    public float HitboxRight  => x + HitboxOffsets.offsetX + HitboxDimensions.width;
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
    const int CELL_SIZE    = 16;
    const int SCREEN_W     = 640;
    const int SCREEN_H     = 240;
    const float MOVE_SPEED = 3f;
    public static void Main()
    {
        int width = SCREEN_W;
        int height = SCREEN_H;
        MapGrid map = new MapGrid(width,height,CELL_SIZE);

        PhysicalEntity player = new Character(0, 0, map, 32, 32, "nico");
        PhysicalEntity box = new Platform(320,128, map, 16, 16);
        PhysicalEntity floor = new Platform(0,192,map,600,8);

        List<PhysicalEntity> entities = new() { floor, box, player };

        Raylib.InitWindow(SCREEN_W, SCREEN_H, "Physics Debug");
        Raylib.SetTargetFPS(60);
        while (!Raylib.WindowShouldClose())
        {
            if (Raylib.IsKeyDown(KeyboardKey.W)) player.AddVelocity(0, -MOVE_SPEED);
            if (Raylib.IsKeyDown(KeyboardKey.S)) player.AddVelocity(0,  MOVE_SPEED);
            if (Raylib.IsKeyDown(KeyboardKey.A)) player.AddVelocity(-MOVE_SPEED, 0);
            if (Raylib.IsKeyDown(KeyboardKey.D)) player.AddVelocity(MOVE_SPEED, 0);
            player.Move();

            Raylib.BeginDrawing();
            Raylib.ClearBackground(Color.Black);
            // grid de referencia (opcional — comentar si molesta)
            for (int col = 0; col <= width; col += CELL_SIZE)
                Raylib.DrawLine(col, 0, col, height, new Color(40, 40, 40, 255));
            for (int row = 0; row <= height; row += CELL_SIZE)
                Raylib.DrawLine(0, row, width, row, new Color(40, 40, 40, 255));
 
            // hitboxes
            foreach (var ent in entities)
                ent.DrawDebug();
 
            // HUD con posición del jugador
            (float px, float py) = player.PosVector;
            Raylib.DrawText($"pos: ({px:F0}, {py:F0})  WASD para mover  ESC para salir",
                            4, 4, 10, Color.White);
 
            Raylib.EndDrawing();

        }
        Raylib.CloseWindow();
    }
}