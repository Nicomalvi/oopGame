using Raylib_cs;
public abstract class Behavior
// leo estado del mundo -> decido que hacer
{
    public abstract bool Execute(Actor actor, float dt);
}
// =====================================================================================================================
// defino behaviors que voy a usar
// =====================================================================================================================
// PLACEHOLDER cellsize = 32, en un futuro sera una var. global

// playerBehavior = hacer lo que diga el input
public class PlayerInputBehavior : Behavior
{
    public override bool Execute(Actor actor, float dt)
    {
        // esto podria ser mas restrictivo
            // ejemplo: horizontalMove marca un posible estado final como idle
            // no hago nada en attackCheck, jumpCheck cambia ese estado a jump
            // cambie de estado 1 sola vez, no hice pasos innecesarios en sistema de animaciones 

            // ademas ahora mismo D + F = moverme y atacar en el mismo turno,
            // acciones más arriba tienen prioridad y se pueden acumular
        HorizontalMoveCheck(actor, dt);
        AttackCheck(actor);
        JumpCheck(actor);
        return true;
    }

    private void HorizontalMoveCheck(Actor actor, float dt)
    {
        // INTENTO moverme horizontalmente -> cambio facing
        int dirX = 0;
        if (Raylib.IsKeyDown(KeyboardKey.A)) {
            dirX = -1;
            actor.facing = HorizontalFacing.left;
        }
        if (Raylib.IsKeyDown(KeyboardKey.D)) {
            dirX = 1;
            actor.facing = HorizontalFacing.right;
        }
        if(dirX != 0)
        {
            float moveSpeed = actor.OnPlatform ? actor.MoveSpeed : actor.MoveSpeed*0.60f;
            actor.MoveHorizontal(dt,dirX,moveSpeed);
        }
        else
        {
            actor.ApplyHorizontalFriction(dt);
            if(actor.GroundCurrentAction()){actor.SwitchAction(ActorState.idle);}
        }
    }

    private void JumpCheck(Actor actor)
    {
        if (Raylib.IsKeyDown(KeyboardKey.W))
        {
            actor.Jump();
        }
        else if (actor.Action == ActorState.jump)
        {
            actor.ReleaseJump();
        }
    }

    private void AttackCheck(Actor actor)
    {
        if(Raylib.IsKeyPressed(KeyboardKey.F))
        {
            actor.Attack();
        }
    }
}
