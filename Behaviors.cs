using Raylib_cs;
public abstract class Behavior
// behavior = funcion que guarda información y modifica a los actores
{
    public abstract bool Execute(Actor actor);
}
// =====================================================================================================================
// defino behaviors que voy a usar
// =====================================================================================================================

// playerBehavior = hacer lo que diga el input
public class PlayerInputBehavior : Behavior
{
    public override bool Execute(Actor actor)
    {
        float dirX = 0;
        float dirY = 0;
        // if (actor.Body.StandingOnPlatform() && Raylib.IsKeyDown(KeyboardKey.W)) {actor.Jump();return true;}
        if (Raylib.IsKeyDown(KeyboardKey.S)) dirY += 1;
        if (Raylib.IsKeyDown(KeyboardKey.A)) dirX -= 1;
        if (Raylib.IsKeyDown(KeyboardKey.D)) dirX += 1;
        actor.GoTowardsDirection(dirX, dirY);
        return true;
    }
}
public class WallBounceBehavior : Behavior
{
    private float dir = 1f;
    public override bool Execute(Actor actor)
    {
        if (actor.MoveVector.VX == 0)
            dir = -dir;
        actor.GoTowardsDirection(dir, 0);
        return true;
    }
}