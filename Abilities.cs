// ================================================================================
// Ability = capacidad que un Actor puede tener o no. Cada una guarda SU propia data
// ================================================================================
public abstract class Ability
{
    // no todas las abilities necesitan tickear cada frame (ej. Attack instantaneo si),
    // pero dejamos el hook virtual para las que si (Jump, Attack con timers, etc.)
    public virtual void Tick(Actor actor, float dt) {}
}

// ===============================
// funciones comunes terrestres
// ===============================
public class MoveAbility : Ability
{
    private float moveSpeed;

    public MoveAbility(float moveSpeed)
    {
        this.moveSpeed = moveSpeed;
    }

    public void MoveHorizontal(Actor actor, float dt, int dir, float speed)
    {
        if(!CanMove(actor)){return;}
        (float vx, float vy) = actor.MoveVector;
        // si me llega una dirección opuesta a la que iba hago un giro rapido
        if (dir != 0 && Math.Sign(dir) != Math.Sign(vx))
            vx = 0;

        // si el movimiento esta iniciando, simulo aceleracion (voy de a multiplos de 64)
        vx = Math.Clamp(vx + dir*64, -speed, speed);
        actor.SetVelocity(vx,vy);
        if(actor.currentAction==Action.idle){actor.currentAction = Action.walk;}
    }

    public void ApplyHorizontalFriction(Actor actor, float dt)
    {
        (float vx, float vy) = actor.MoveVector;
        if (Math.Abs(vx) < 64)
        {
            actor.SetVelocity(0,vy);
        } else
        {
            float friction = MathF.Pow(0.00005f, dt); // friccion depende de TIEMPO
            actor.SetVelocity(vx * friction, vy);
        }
    }

    private bool CanMove(Actor actor)
    {
        return actor.currentAction != Action.attack;
    }

    public float MoveSpeed => moveSpeed;
}

public class JumpAbility : Ability
{
    //==================================================
    // valores importantes para restringir acciones
    //==================================================
    private float jumpTimer = 0;
    public bool stoppedJumpHeight = false;
    public int jumpsMade = 0;

    private float maxJumpHeldTime = 0.175f;
    private float pixelsPerJumpTick = 32*5;
    private float initalJumpSpeed = 32*(0.5f);

    public void Jump(Actor actor)
    {
        if(!CanJump(actor)){return;}
        if (jumpsMade == 0)
        {
            // caso: recien salto
            jumpsMade++;
            actor.currentAction = Action.jump;
            actor.AddVelocity(0,-initalJumpSpeed);
        } else
        {
            // caso: ya estoy en el aire
            if(actor.currentAction == Action.jump && jumpTimer < maxJumpHeldTime && !stoppedJumpHeight)
            {
                float vx = actor.MoveVector.VX;
                actor.SetVelocity(vx, -pixelsPerJumpTick);
                return;
            }
            // else, caso double jump
        }
    }

    // intencion: "solté la tecla de salto" -> la ability decide que hacer con eso
    public void OnJumpKeyReleased()
    {
        stoppedJumpHeight = true;
    }

    public override void Tick(Actor actor, float dt)
    {
        UpdateAirActions(actor, dt);
    }

    private void UpdateAirActions(Actor actor, float dt)
    {
        if(actor.currentAction != Action.jump && actor.currentAction != Action.fall)
            return;
        jumpTimer += dt;
        if(actor.OnPlatform)
        {
            jumpTimer = 0;
            jumpsMade = 0;
            stoppedJumpHeight = false;
            actor.currentAction = Action.idle;
            return;
        }
        if(actor.MoveVector.VY >= 0)
        {
            actor.currentAction = Action.fall;
        }
    }

    private bool CanJump(Actor actor)
    {
        return actor.currentAction != Action.attack;
    }
}

public class AttackAbility : Ability
{
    public float attackingTimer = 0;
    private float attackDuration = 1;

    public void Attack(Actor actor)
    {
        if(!CanAttack(actor)){return;}
        actor.currentAction = Action.attack;
    }

    public override void Tick(Actor actor, float dt)
    {
        UpdateAttackAction(actor, dt);
    }

    private void UpdateAttackAction(Actor actor, float dt)
    {
        if(actor.currentAction != Action.attack)
            return;
        attackingTimer += dt;
        if(attackingTimer >= attackDuration)
        {
            attackingTimer = 0;
            actor.currentAction = Action.idle;
        }
    }

    private bool CanAttack(Actor actor)
    {
        return actor.currentAction != Action.jump && actor.currentAction != Action.fall;
    }
}