using System.Text.RegularExpressions;

public enum State{idle,walk,jump,fall,attack};
public class Actor
// Actor = interfaz: le paso info a mi behavior y esta me dice como actuar (ya sea modificar mis fisicas, estado...)
{
    private PhysicalEntity body;

    // behavior + state = basicamente state machine
    private Behavior behavior;
    private State state;

    // diferencio la velocidad del actor con velocidades externas
    // velocidad de correr =/= velocidad de estar en un ascensor
    private float moveSpeed; 
    public Actor(PhysicalEntity body, float moveSpeed, Behavior behavior)
    {
        this.body = body;
        this.behavior = behavior;
        this.moveSpeed = moveSpeed;  
        this.state = State.idle;
    }
    public void Update(float dt)
    {
        behavior.Execute(this, dt);
    }
    public void SetState(State newState)
    {
        state = newState;
    }
    public void StartAction()
    {
        
    }
    //================================================================================================
    // funciones comunes entre varios actores
    //================================================================================================
    public void AddVelocity(float vx, float vy)
    // marco mi velocidad personal
    {
        Body.AddVelocity(vx,vy);
    }
    public void SetVelocity(float vx, float vy)
    {
        Body.SetVelocity(vx,vy);
    }
    public PhysicalEntity Body => body;
    public float MoveSpeed => moveSpeed;
    public State State => state;
    // mismos getters que body para un trabajo mas limpio
    // hace falta? puedo traer el body y ver eso...
    // actor = api publica
    public bool OnPlatform => body.StandingOnPlatform();

    public (float X, float Y)   PosVector  => body.PosVector;
    public (float VX, float VY) MoveVector => body.MoveVector;

    public MapGrid Map => body.Map;
}