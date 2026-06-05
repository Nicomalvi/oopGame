using System.Numerics;
using Raylib_cs;
public class PhysicalEntity
// PhysEnt = entidad fisica con una hitbox que se puede mover y chocar con otras entidades en el mismo mapa
{
    private float x, y;
    private float maxX, maxY;
    private float vx, vy;
    private Hitbox hitbox;
    private bool affectedByGravity;
    private float GRAVITY = 0;
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
        GRAVITY = map.CellSize*16; // quiero caer 4 bloques por segundo (PLACEHOLDER)
    }
    // ====================================================================================================================
    // funciones basicas: mover la entidad
    // ====================================================================================================================
    public void AddVelocity(float vx, float vy)
    {
        this.vx += vx;
        this.vy += vy;
        ClampVelocity();
    }
    public void SetVelocity(float vx, float vy)
    {
        this.vx = vx;
        this.vy = vy;
        ClampVelocity();
    }
    private void ClampVelocity()
    {
        // TODO PhysEnt tendra la misma maxima velocidad
        vx = Math.Clamp(vx,-12800,12800);
        vy = Math.Clamp(vy,-12800,12800); 
    }
    private void SetPosition(float nX, float nY){
        x = Math.Clamp(nX,0,maxX);
        y = Math.Clamp(nY,0,maxY);
    }
    // ====================================================================================================================
    // informacion de la hitbox en el mapa
    // ====================================================================================================================
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
    // ====================================================================================================================
    // movimiento de una entidad y colisiones
    // ====================================================================================================================
    private void Move(float dt)
    {
        // me voy moviendo de a 1 paso
        // si detecto una colision, chequeo si me hace frenar, chequeo efectos de la colision
        (float vx, float vy) =  MoveVector;

        vx *= dt; // multiplico por frameTime
        vy *= dt;

        if((vx,vy) == (0,0)){return;} // corto acá si la suma de velocidad personal y externa es 0

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
                Console.WriteLine("COLLISION");
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
    public bool StandingOnPlatform()
    {
        // chequeo: si mi pos. fuera 1 pixel abajo...
        // choco con alguien? y si choco, mi parte inferior esta arriba de su parte superior?
        // entonces estoy parado encima.
        (float x, float y) = PosVector;
        SetPosition(x,y+1);
        List<PhysicalEntity> collisionList = new List<PhysicalEntity>();
        collisionList = HitboxOverlapList();
        bool res = collisionList.Exists(ent => CollisionStopsMovement(ent) && 
                                               HitboxTop <= ent.HitboxTop); // Y CRECE PARA ABAJO !
        SetPosition(x,y);
        return res;
    }
    public void Update(float dt)
    {
        if (affectedByGravity && !StandingOnPlatform())
        {
            float newVy = Math.Min(this.vy + GRAVITY * dt, GRAVITY);
            SetVelocity(this.vx, newVy);
        }
        if(MoveVector.VX != 0 || MoveVector.VY != 0)
        {
            Move(dt);
        }
    }
    // ====================================================================================================================
    // debug
    // ====================================================================================================================
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
    public void DrawDebug()
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
    // ====================================================================================================================
    // getters
    // ====================================================================================================================
    public (float X, float Y)   PosVector  => (x, y);
    public (float VX, float VY) MoveVector => (vx, vy);

    public (float width, float height) HitboxDimensions => hitbox.Dimensions;
    public (float offsetX, float offsetY) HitboxOffsets => hitbox.Offsets;
    public float HitboxTop    => y + HitboxOffsets.offsetY;
    public float HitboxBottom => y + HitboxOffsets.offsetY + HitboxDimensions.height;
    public float HitboxLeft   => x + HitboxOffsets.offsetX;
    public float HitboxRight  => x + HitboxOffsets.offsetX + HitboxDimensions.width;

    public bool HasGravity => affectedByGravity;
    public MapGrid Map => map;
}