public class Actor
// Actor = interfaz: le paso info a mi behavior y esta me dice como actuar (ya sea modificar mis fisicas, estado...)
{
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