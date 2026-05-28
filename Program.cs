// POR AHORA METO TODO EN 1 ARCHIVO el ultimo proyecto se me fue de las manos y tenia como 20 archivos distintos
using Raylib_cs;
// =====================================================================================================
// MAPA DE HITBOXES
// =====================================================================================================
public class MapGrid
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
// =====================================================================================================
// HITBOX
// =====================================================================================================
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
// =====================================================================================================
// PHYSENT
// =====================================================================================================
public class PhysicalEntity
{
    private float x, y;
    private float maxX, maxY;
    private float vx, vy;
    private Hitbox hitbox;
    private bool affectedByGravity;
    private const float GRAVITY = 800f;
    private MapGrid map;
    public PhysicalEntity(float x, float y, MapGrid map, float width, float height, bool affectedByGravity, float xOffset = 0, float yOffset = 0)
    {
        this.map = map;
        this.maxX = map.Width - 1;
        this.maxY = map.Height - 1;
        this.x    = Math.Clamp(x, 0, maxX);
        this.y    = Math.Clamp(y, 0, maxY);
        this.affectedByGravity = affectedByGravity;
        vx = 0; vy = 0;
        hitbox = new Hitbox(width, height, xOffset, yOffset);
        this.map.AddEntity(this);
    }
    private void AddVelocity(float vx, float vy)
    {
        this.vx += vx;
        this.vy += vy;
        ClampVelocity();
    }
    private void SetVelocity(float vx, float vy)
    {
        this.vx = vx;
        this.vy = vy;
        ClampVelocity();
    }
    private void ClampVelocity()
    {
        // TODO PhysEnt tendra la misma maxima velocidad
        vx = Math.Clamp(vx,-3200,3200);
        vy = Math.Clamp(vy,-3200,3200); 
    }
    private void SetPosition(float nX, float nY){
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
        // me fijo cuales hitbox ocupan alguna celda en la que estoy
        // luego, me fijo si hay colision
        (int startX, int endX, int startY, int endY) = GetHitboxMapCells();
        var res = new List<PhysicalEntity>();
        for (int cx = startX; cx <= endX; cx++)
            for (int cy = startY; cy <= endY; cy++)
                res.AddRange(map.GetCellByIndex(cx, cy));

        return res.Where(ent => ent != this &&
                                HitboxLeft   < ent.HitboxRight  &&
                                HitboxRight  > ent.HitboxLeft   &&
                                HitboxTop    < ent.HitboxBottom &&
                                HitboxBottom > ent.HitboxTop).ToList();
    }
    private bool PositionInBounds()
    {
        (float x, float y) = PosVector;
        (float xOffset, float yOffset) = HitboxOffsets;
        (float width, float height) = HitboxDimensions;
        return x + xOffset + width <= maxX && x + xOffset + width >= 0 &&
               y + yOffset + height <= maxY && y + yOffset + height >= 0;
    }
    public void SetSpeedTowards(float dirX, float dirY, float moveSpeed)
    {
        // paso un punto (x,y) en el mapa
        // el actor intenta ir hacia allí con su velocidad
        if(dirX == 0 && dirY == 0)
        {
            SetVelocity(0, 0);
            return;
        }
        float length = MathF.Sqrt(dirX * dirX + dirY * dirY);
        // hago pitagoras para ver cuanto mide el vector
        // divido el vector por lo que mide asi lo normalizo
        // nuevo vector <= (1,1)
        // multiplico mi velocidad por eso
        // (sino ir por ejemplo de una a (8,5)*velocidad iria rapidisimo)
        dirX /= length;
        dirY /= length;

        SetVelocity(dirX * moveSpeed, dirY * moveSpeed);
    }
    private void Move(float dt)
    {
        // me voy moviendo de a 1 paso
        // si detecto una colision, chequeo si me hace frenar, chequeo efectos de la colision
        (float vx, float vy) =  MoveVector;
        vx *= dt; // multiplico por frameTime
        vy *= dt;
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
                SetVelocity(0,this.vy);
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
                SetVelocity(this.vx,0);
            }
        }
        SetPosition(x,y);
        map.AddEntity(this);
    }
    private bool CollisionStopsMovement(PhysicalEntity ent)
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
    public virtual void Update(float dt)
    {
        if(affectedByGravity){AddVelocity(0,GRAVITY);}
        if(MoveVector.VX != 0 || MoveVector.VY != 0)
        {
            Move(dt);
        }
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

        // posición real con un cuadrado rojo para que se pueda ver
        Raylib.DrawRectangle((int)x, (int)y, 16, 16, new Color(255, 0, 0, 120));
        Raylib.DrawRectangleLines((int)x, (int)y, 16, 16, Color.Red);

        // hitbox
        int rx = (int)(x + xOff);
        int ry = (int)(y + yOff);
        int rw = (int)w;
        int rh = (int)h;
        Raylib.DrawRectangle(rx, ry, rw, rh, Color.White);
        Raylib.DrawRectangleLines(rx, ry, rw, rh, Color.White);
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

    public MapGrid Map => map;
}
public abstract class Behavior
{
    public abstract bool Execute(Actor actor);
}

public class PlayerInputBehavior : Behavior
{
    public override bool Execute(Actor actor)
    {
        float dirX = 0;
        float dirY = 0;
        if (Raylib.IsKeyDown(KeyboardKey.W)) dirY -= 1;
        if (Raylib.IsKeyDown(KeyboardKey.S)) dirY += 1;
        if (Raylib.IsKeyDown(KeyboardKey.A)) dirX -= 1;
        if (Raylib.IsKeyDown(KeyboardKey.D)) dirX += 1;
        actor.GoTowardsDirection(dirX, dirY);
        return true;
    }
}
public class WallBounceBehavior : Behavior
{
    private float dir = 1f;
    public override bool Execute(Actor actor)
    {
        if (actor.MoveVector.VX == 0)
            dir = -dir;
        actor.GoTowardsDirection(dir, 0);
        return true;
    }
}
public class Actor
{
    // controller (decido que hacer con ia x o ia y, o con input de player...)
    private PhysicalEntity body;
    private Behavior behavior;
    private float moveSpeed; 

    public Actor(PhysicalEntity body, float moveSpeed, Behavior behavior)
    {
        this.body = body;
        this.behavior = behavior;
        this.moveSpeed = moveSpeed;  
    }
    public void Update()
    {
        behavior.Execute(this);
    }
    public void GoTowardsDirection(float x, float y)
    {
        Body.SetSpeedTowards(x,y,moveSpeed);
    }
    public PhysicalEntity Body => body;
    public float MoveSpeed => moveSpeed;
    // mismos getters que body para un trabajo mas limpio
    // hace falta? puedo traer el body y ver eso...
    // actor = api publica
    public (float X, float Y)   PosVector  => body.PosVector;
    public (float VX, float VY) MoveVector => body.MoveVector;

    public (float width, float height) HitboxDimensions => body.HitboxDimensions;
    public (float offsetX, float offsetY) HitboxOffsets => body.HitboxOffsets;
    public float HitboxTop    => body.HitboxTop;
    public float HitboxBottom => body.HitboxBottom;
    public float HitboxLeft   => body.HitboxLeft;
    public float HitboxRight  => body.HitboxRight;

    public MapGrid Map => body.Map;
}
// =====================================================================================================
// GAME LOOP
// =====================================================================================================
public class Program
{
    const int CELL_SIZE    = 32;
    const int SCREEN_W     = 960;
    const int SCREEN_H     = 640;
    public static void Main()
    {
        int width = SCREEN_W;
        int height = SCREEN_H;
        MapGrid map = new MapGrid(width,height,CELL_SIZE);
        string topLeftCorner = 
        "11111"+
        "1...."+
        "1...."+
        "1...."+
        "1....";
        string topRightCorner = 
        "11111"+
        "....1"+
        "....1"+
        "....1"+
        "....1";
        string bottomLeftCorner = 
        "1...."+
        "1...."+
        "1...."+
        "1...."+
        "11111";
        string bottomRightCorner = 
        "....1"+
        "....1"+
        "....1"+
        "....1"+
        "11111";
        string[][]chunks = [[topLeftCorner,topRightCorner],
                            [bottomLeftCorner,bottomRightCorner]];
        List<PhysicalEntity> physEntities = map.CreateChunks(chunks,0,0,5,5);
        Raylib.InitWindow(SCREEN_W, SCREEN_H, "Physics Debug");
        Raylib.SetTargetFPS(60);
        float dt;

        PhysicalEntity playerBody = new PhysicalEntity(32,31,map,16,16,true);
        physEntities.Add(playerBody); // placeholder
        Actor playerActor = new Actor(playerBody, 1024, new PlayerInputBehavior());
        List<Actor> actors = [playerActor];

        PhysicalEntity boxBody = new PhysicalEntity(32,64,map,32,32,false);
        physEntities.Add(boxBody);
        Actor boxActor = new Actor(boxBody, 320, new WallBounceBehavior());
        actors.Add(boxActor);
        while (!Raylib.WindowShouldClose())
        {
            // AL PRINCIPIO DEL GAME LOOP TRAIGO EL FRAME TIME, TODOS TRABAJAN CON EL MISMO
            dt = Raylib.GetFrameTime();
            //===================================================================================================================
            // actuan todos
            //===================================================================================================================
            foreach (Actor actor in actors)
            {
                actor.Update();
            }
            foreach (PhysicalEntity entity in physEntities)
            {
                entity.Update(dt);
            }
            //===================================================================================================================
            // renders
            //===================================================================================================================
            Raylib.BeginDrawing();
            Raylib.ClearBackground(Color.Black);
            // grid de referencia (opcional — comentar si molesta)
            for (int col = 0; col <= width; col += CELL_SIZE)
                Raylib.DrawLine(col, 0, col, height, new Color(40, 40, 40, 255));
            for (int row = 0; row <= height; row += CELL_SIZE)
                Raylib.DrawLine(0, row, width, row, new Color(40, 40, 40, 255));
            // hitboxes PLACEHOLDER
            foreach (var ent in physEntities)
                ent.DrawDebug();
            Raylib.EndDrawing();
        }
        Raylib.CloseWindow();
    }
}