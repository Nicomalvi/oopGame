using Raylib_cs;
public abstract class Behavior
// behavior = funcion que guarda información y modifica a los actores
// funcionan como una state machine
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
        bool blockingActionTaken = false;
        switch (actor.State)
        {
            case State.walk:
                HorizontalMoveCheck(actor,dt,actor.MoveSpeed);
                blockingActionTaken = AttackCheck(actor, dt);
                if(!blockingActionTaken){JumpCheck(actor);}
                break;
            case State.idle:
                HorizontalMoveCheck(actor,dt,actor.MoveSpeed);
                blockingActionTaken = AttackCheck(actor, dt);
                if(!blockingActionTaken){JumpCheck(actor);}
                break;
            case State.jump:
                HorizontalMoveCheck(actor,dt,actor.MoveSpeed * 1f);
                if (Raylib.IsKeyDown(KeyboardKey.W))
                {
                    actor.Jump();
                } else {actor.stoppedJumpHeight = true;}
                break;//SALTAR CORTA TODO
        }
        return true;
    }

    private void HorizontalMoveCheck(Actor actor, float dt, float moveSpeed)
    {
        int dirX = 0;
        if (Raylib.IsKeyDown(KeyboardKey.A)) 
        {   
            // facing LEFT
            if(actor.State==State.idle){actor.SetState(State.walk);}
            dirX = -1;
        }
        if (Raylib.IsKeyDown(KeyboardKey.D))
        {   
            // facing RIGHT
            if(actor.State==State.idle){actor.SetState(State.walk);}
            dirX = 1;
        }
        if(dirX != 0) {actor.MoveHorizontal(dt,dirX,moveSpeed);}
        if(dirX == 0)
        // si no quise moverme en X, genero friccion 
        {
            actor.ApplyHorizontalFriction(dt);
        }
    }
    private bool JumpCheck(Actor actor)
    {
        if (Raylib.IsKeyPressed(KeyboardKey.W) && actor.OnPlatform)
        {
            actor.Jump();
            return true;
        }
        return false;
    }
    private bool AttackCheck(Actor actor, float dt)
    {
        if(Raylib.IsKeyPressed(KeyboardKey.F) && actor.OnPlatform)
        {
            actor.ApplyHorizontalFriction(dt);
            actor.SetState(State.attack);
            // aca creo la hitbox de daño o depende de la animacion y la creo en cierto momento
            // del attack
            return true;
        }
        return false;
    }
}

// se acerca a los puntos de la lista
//public class PatrolBehavior : Behavior