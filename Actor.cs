using System.Text.RegularExpressions;

public enum State{idle,walk,jump,fall,attack};
public class Actor
// Actor = interfaz: le paso info a mi behavior y esta me dice como actuar (ya sea modificar mis fisicas, estado...)
{
    private PhysicalEntity body;

    // behavior + state = basicamente state machine
    private Behavior behavior;
    private State state;

    private float moveSpeed; 
    //==================================================
    // valores importantes para restringir acciones
    //==================================================
    private float jumpTimer = 0;
    public bool stoppedJumpHeight = false;
    public int jumpsMade = 0;
    

    private float maxJumpHeldTime = 0.175f;
    private float pixelsPerJumpTick = 32*5;
    private float initalJumpSpeed = 32*(0.5f);

    public float attackingTimer = 0;
    private float attackDuration = 1;
    public Actor(PhysicalEntity body, float moveSpeed, Behavior behavior)
    {
        this.body = body;
        this.behavior = behavior;
        this.moveSpeed = moveSpeed;  
        state = State.idle;
    }
    public void Update(float dt)
    {
        UpdateJump(dt);
        UpdateAttack(dt);
        behavior.Execute(this, dt);
    }
    public void SetState(State newState)
    {
        state = newState;
    }
    //================================================================================================
    // funciones comunes entre varios actores
    //================================================================================================
    private void AddVelocity(float vx, float vy)
    // marco mi velocidad personal
    {
        Body.AddVelocity(vx,vy);
    }
    private void SetVelocity(float vx, float vy)
    {
        Body.SetVelocity(vx,vy);
    }
    //===============================
    // funciones comunes terrestres
    //===============================
    public void MoveHorizontal(float dt, int dir, float speed)
    {
        (float vx, float vy) = MoveVector;
        // si me llega una dirección opuesta a la que iba hago un giro rapido
        if (dir != 0 && Math.Sign(dir) != Math.Sign(vx))
            vx = 0;
        
        // si el movimiento esta iniciando, simulo aceleracion (voy de a multiplos de 64)
        vx = Math.Clamp(vx + dir*64, -speed, speed);
        SetVelocity(vx,vy);
    }
    public void Jump()
    {
        if (jumpsMade == 0)
        {
            // caso: recien salto
            jumpsMade++;
            SetState(State.jump);
            AddVelocity(0,-initalJumpSpeed);
        } else
        {
            // caso: ya estoy en el aire
            if(jumpTimer < maxJumpHeldTime && !stoppedJumpHeight)
            {
                float vx = MoveVector.VX;
                SetVelocity(vx, -pixelsPerJumpTick);
                return;
            }
            // else, caso double jump
        }
    }
    public void ApplyHorizontalFriction(float dt)
    {
        (float vx, float vy) = MoveVector;
        if (Math.Abs(vx) < 64)
        {
            SetVelocity(0,vy);
        } else
        {
            float friction = MathF.Pow(0.0001f, dt); // friccion depende de TIEMPO
            SetVelocity(vx * friction, vy);
        }
        if(GroundState())
        {
            SetState(State.idle);
        }
    }
    public bool AirState()
    {
        return State == State.jump | State == State.fall;
    }
    public bool GroundState()
    {
        return State == State.idle || State == State.walk;
    }
    private void UpdateJump(float dt)
    {
        if(State != State.jump)
            return;
        jumpTimer += dt;
        if(OnPlatform)
        {
            jumpTimer = 0;
            jumpsMade = 0;
            stoppedJumpHeight = false;
            SetState(State.idle);
        }
    }
    private void UpdateAttack(float dt)
    {
        if(State != State.attack)
            return;
        attackingTimer += dt;
        if(attackingTimer >= attackDuration)
        {
            attackingTimer = 0;
            SetState(State.idle);
        }
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