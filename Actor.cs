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
    public void Update()
    {
        behavior.Execute(this);
    }
    public void SetState(State newState)
    {
        state = newState;
    }
    public void StartAction()
    {
        
    }
    //================================================================================================
    // funciones comunes entre varios actores (moverse y saltar no cambia comportamiento)
    //================================================================================================
    public void AddVelocity(float vx, float vy)
    {
        body.AddVelocity(vx,vy);
    }
    public void MoveTowardsVector(float dirX, float dirY)
    // paso un punto (x,y) en el mapa
    // physEnt SUMA  la velocidad personal en esa direccion
    {
        (float vx, float vy) = MoveVector;
        if(dirX == 0 && dirY == 0)
        // moverme hacia ninguna direccion se interpreta como
        // dejar de moverme en X
        // SI NO TENGO GRAVEDAD dejar de moverme en Y
        {
            vx *= 0.7f;
            if(!body.HasGravity){vy = 0;} 

            Body.SetVelocity(vx,vy);
            return;
        }

        // si me llega una dirección opuesta a la que iba hago un giro rapido
        // en vez de seguir yendo pero un poco mas lento hacia donde ya no quiero
        if (dirX != 0 && Math.Sign(dirX) != Math.Sign(vx))
            vx = 0;
        if (dirY != 0 && Math.Sign(dirY) != Math.Sign(vy))
            vy = 0;

        float length = MathF.Sqrt(dirX * dirX + dirY * dirY);
        dirX /= length;
        dirY /= length;

        // PLACEHOLDER: NECESITO FORMA DE LIMITAR CUANTA VELOCIDAD PUEDO GANAR POR MI CUENTA
        vx += dirX*64;
        vy += dirY*64;

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