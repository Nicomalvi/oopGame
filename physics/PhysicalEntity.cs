using System.Numerics;
using Raylib_cs;
public class PhysicalEntity
// PhysEnt = entidad fisica con una hitbox que se puede mover y chocar con otras entidades en el mismo mapa
{
    private float x, y;
    private float vx, vy;
    private Hitbox hitbox;
    private bool affectedByGravity;
    public bool onPlatform = false;
    public PhysicalEntity(float x, float y, float width, float height, bool affectedByGravity, float xOffset = 0, float yOffset = 0)
    {
        this.x    = x;
        this.y    = y;
        this.affectedByGravity = affectedByGravity;
        vx = 0; vy = 0;
        hitbox = new Hitbox(width, height, xOffset, yOffset);
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
    public void SetPosition(float nX, float nY){
        x = nX;
        y = nY;
    }
    public bool CollisionStopsMovement(PhysicalEntity ent)
    {
        return this!=ent; // toda PhysEnt por defecto frena al chocar con cualquier entidad
    }                     // distintos hijos tendran overRide de este metodo
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
    public bool OnPlatform => onPlatform;
}