public class PhysicsSystem   
// mediador entre PhysEnt y MapGrid
// evito dependencias ciclicas, utilizo Observer/Mediator pattern
{
    private MapGrid map;
    private List<PhysicalEntity> entities;
    private float GRAVITY;

    public PhysicsSystem(MapGrid map)
    {
        this.map = map;
        this.entities = new List<PhysicalEntity>();
        GRAVITY = map.CellSize*6;
    }
    public void UpdatePhysics(float dt)
    {
        foreach (PhysicalEntity ent in entities)
        {
            (float vx, float vy) = ent.MoveVector;
            if (ent.HasGravity && !EntityOnPlatform(ent))
            {
                float newVy = Math.Min(vy + GRAVITY * dt, GRAVITY);
                ent.SetVelocity(vx, newVy);
                vy = newVy;
            }
            if(vx != 0 || vy != 0)
            {
                Move(ent,dt);
            }
        }
    }
    public void AddEntity(PhysicalEntity ent)
    {
        entities.Add(ent);

        (int x1,int xf,int y1,int yf) = EntityMapCells(ent);
        map.AddEntity(ent,x1,xf,y1,yf);
    }

    public void RemoveEntity(PhysicalEntity ent)
    {
        entities.Remove(ent);

        (int x1,int xf,int y1,int yf) = EntityMapCells(ent);
        map.RemoveEntity(ent,x1,xf,y1,yf);
    }
    private void Move(PhysicalEntity ent, float dt)
    {
        // me voy moviendo de a 1 paso
        // si detecto una colision, chequeo si me hace frenar, chequeo efectos de la colision
        (float vx, float vy) =  ent.MoveVector;

        vx *= dt; // multiplico por frameTime
        vy *= dt;

        if((vx,vy) == (0,0)){return;} // corto acá si la suma de velocidad personal y externa es 0

        (int x1, int xf, int y1, int yf) = EntityMapCells(ent);
        map.RemoveEntity(ent,x1,xf,y1,yf);

        (float x, float y) =    ent.PosVector;

        float maxSteps = Math.Max(Math.Abs(vx), Math.Abs(vy));
        (float stepX, float stepY) = (vx / maxSteps, vy / maxSteps);

        List<PhysicalEntity> entities = new List<PhysicalEntity>();

        for(int i = 0; i < maxSteps; i++)
        {
            // si ya estoy en colision antes de moverme no lo detecto con este sistema
            x += stepX;
            ent.SetPosition(x,y);
            entities = EntityOverlapList(ent);
            //foreach entinty in entities COLISION, luego chequear si me frena dentro del if
            if(!EntityInBounds(ent) || entities.Exists(ent.CollisionStopsMovement))
            {
                x -= stepX;
                ent.SetPosition(x,y);
                stepX = 0;
                ent.SetVelocity(0,vy);
            }
            // todo igual pero ahora con y
            y += stepY;
            ent.SetPosition(x,y);
            entities = EntityOverlapList(ent);
            if(!EntityInBounds(ent) || entities.Exists(ent.CollisionStopsMovement))
            {
                Console.WriteLine("COLLISION");
                y -= stepY;
                ent.SetPosition(x,y);
                stepY = 0;
                ent.SetVelocity(vx,0);
            }
        }
        ent.SetPosition(x,y);

       (x1,xf,y1,yf) = EntityMapCells(ent);
        map.AddEntity(ent,x1,xf,y1,yf);
    }
    private (int,int,int,int) EntityMapCells(PhysicalEntity ent)
    {
        // puede devolver out of bounds !!! 
        (float xOffset, float yOffset) = ent.HitboxOffsets;
        (float width, float height) = ent.HitboxDimensions;
        (float x, float y) = ent.PosVector;
        
        return((int)(x + xOffset)/map.CellSize,(int)(x + xOffset + width)/map.CellSize,
               (int)(y + yOffset)/map.CellSize,(int)(y + yOffset + height)/map.CellSize);
    }
    private bool EntityInBounds(PhysicalEntity ent)
    {
        (float x, float y) = ent.PosVector;
        (float xOffset, float yOffset) = ent.HitboxOffsets;
        (float width, float height) = ent.HitboxDimensions;
        return x + xOffset + width <= map.Width-1 && x + xOffset + width >= 0 &&
               y + yOffset + height <= map.Height-1 && y + yOffset + height >= 0;
    }
    private List<PhysicalEntity> EntityOverlapList(PhysicalEntity ent)
    {
        // me fijo cuales hitbox ocupan alguna celda en la que estoy
        // luego, me fijo si hay colision
        (int startX, int endX, int startY, int endY) = EntityMapCells(ent);
        var res = new List<PhysicalEntity>();
        for (int cx = startX; cx <= endX; cx++)
            for (int cy = startY; cy <= endY; cy++)
                res.AddRange(map.GetCellByIndex(cx, cy));

        return res.Where(physEnt => physEnt != ent &&
                                ent.HitboxLeft   < physEnt.HitboxRight  &&
                                ent.HitboxRight  > physEnt.HitboxLeft   &&
                                ent.HitboxTop    < physEnt.HitboxBottom &&
                                ent.HitboxBottom > physEnt.HitboxTop).ToList();
    }
    public bool EntityOnPlatform(PhysicalEntity ent)
    {
        // chequeo: si mi pos. fuera 1 pixel abajo...
        // choco con alguien? y si choco, mi parte inferior esta arriba de su parte superior?
        // entonces estoy parado encima.
        (float x, float y) = ent.PosVector;
        ent.SetPosition(x,y+1);
        List<PhysicalEntity> collisionList = new List<PhysicalEntity>();
        collisionList = EntityOverlapList(ent);
        bool res = collisionList.Exists(physent => ent.CollisionStopsMovement(physent) && 
                                               ent.HitboxTop <= physent.HitboxTop); // Y CRECE PARA ABAJO !
        ent.SetPosition(x,y);
        return res;
    }
}