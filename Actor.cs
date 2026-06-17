using System.Text.RegularExpressions;

public enum Action{idle,walk,jump,fall,attack};
public class Actor
// Actor = interfaz: le paso info a mi behavior y esta me dice como actuar (ya sea modificar mis fisicas, estado...)
{
    private PhysicalEntity body;

    private Behavior behavior;
    public Action currentAction;

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
        currentAction = Action.idle;
    }
    public void Update(float dt)
    {
        UpdateAirActions(dt);
        UpdateAttackAction(dt);
        behavior.Execute(this, dt);
    }
    //================================================================================================
    // funciones comunes entre varios actores
    //================================================================================================
    private void AddVelocity(float vx, float vy)
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
        if(!CanMove()){return;}
        (float vx, float vy) = MoveVector;
        // si me llega una dirección opuesta a la que iba hago un giro rapido
        if (dir != 0 && Math.Sign(dir) != Math.Sign(vx))
            vx = 0;
        
        // si el movimiento esta iniciando, simulo aceleracion (voy de a multiplos de 64)
        vx = Math.Clamp(vx + dir*64, -speed, speed);
        SetVelocity(vx,vy);
        if(currentAction==Action.idle){currentAction = Action.walk;}
    }
    public void ApplyHorizontalFriction(float dt)
    {
        (float vx, float vy) = MoveVector;
        if (Math.Abs(vx) < 64)
        {
            SetVelocity(0,vy);
        } else
        {
            float friction = MathF.Pow(0.00005f, dt); // friccion depende de TIEMPO
            SetVelocity(vx * friction, vy);
        }
    }
    public void Jump()
    {
        if(!CanJump()){return;}
        if (jumpsMade == 0)
        {
            // caso: recien salto
            jumpsMade++;
            currentAction = Action.jump;
            AddVelocity(0,-initalJumpSpeed);
        } else
        {
            // caso: ya estoy en el aire
            if(currentAction == Action.jump && jumpTimer < maxJumpHeldTime && !stoppedJumpHeight)
            {
                float vx = MoveVector.VX;
                SetVelocity(vx, -pixelsPerJumpTick);
                return;
            }
            // else, caso double jump
        }
    }
    public void Attack()
    {
        if(!CanAttack()){return;}
        currentAction = Action.attack;
    }
    public bool AircurrentAction()
    {
        return currentAction == Action.jump | currentAction == Action.fall;
    }
    public bool GroundcurrentAction()
    {
        return currentAction == Action.idle || currentAction == Action.walk;
    }
    private void UpdateAirActions(float dt)
    {
        if(currentAction != Action.jump && currentAction != Action.fall)
            return;
        jumpTimer += dt;
        if(OnPlatform)
        {
            jumpTimer = 0;
            jumpsMade = 0;
            stoppedJumpHeight = false;
            currentAction = Action.idle;
            return;
        }
        if(MoveVector.VY >= 0)
        {
            currentAction = Action.fall;
        }
    }
    private void UpdateAttackAction(float dt)
    {
        if(currentAction != Action.attack)
            return;
        attackingTimer += dt;
        if(attackingTimer >= attackDuration)
        {
            attackingTimer = 0;
            currentAction = Action.idle;
        }
    }
    private bool CanAttack()
    {
        return currentAction != Action.jump && currentAction != Action.fall;
    }
    private bool CanMove()
    {
        return currentAction != Action.attack;
    }
    private bool CanJump()
    {
        return currentAction != Action.attack;
    }
    public PhysicalEntity Body => body;
    public float MoveSpeed => moveSpeed;
    public Action Action => currentAction;
    // mismos getters que body para un trabajo mas limpio
    // hace falta? puedo traer el body y ver eso...
    // actor = api publica
    public bool OnPlatform => body.OnPlatform;

    public (float X, float Y)   PosVector  => body.PosVector;
    public (float VX, float VY) MoveVector => body.MoveVector;
}