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
        HorizontalMoveCheck(actor, dt);
        AttackCheck(actor);
        JumpCheck(actor);
        return true;
    }

    private void HorizontalMoveCheck(Actor actor, float dt)
    {
        int dirX = 0;
        if (Raylib.IsKeyDown(KeyboardKey.A)) dirX = -1;
        if (Raylib.IsKeyDown(KeyboardKey.D)) dirX = 1;

        if(dirX != 0)
        {
            float moveSpeed = actor.OnPlatform ? actor.MoveSpeed : actor.MoveSpeed*0.60f;
            actor.MoveHorizontal(dt,dirX,moveSpeed);
        }
        else
        {
            actor.ApplyHorizontalFriction(dt);
        }
    }

    private void JumpCheck(Actor actor)
    {
        if (Raylib.IsKeyDown(KeyboardKey.W))
        {
            actor.Jump();
        }
        else if (actor.Action == Action.jump)
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
