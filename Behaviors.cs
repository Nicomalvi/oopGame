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

// playerBehavior = hacer lo que diga el input
public class PlayerInputBehavior : Behavior
{
    // recuerdo hace cuanto estoy tocando Jump, asi el largo del salto es variable
    float jumpTime = 0;
    float maxJumpTime = 0.7f;
    int jumpsMade = 0;

    public override bool Execute(Actor actor, float dt)
    {
        // si estoy idle o walk puedo tocar cualquier boton
        // si estoy falling o jump no puedo jump, otras dir. actuan distinto
        switch (actor.State)
        {
            // quizas es mejor meter break en cada casao y posterior al switch meter
            // el codigo que iria SIEMPRE independiente del estado
            // (o eso va en Actor.Update()?)
            case State.walk:
                // en un futuro: caminando + tecla ataque pasa algo, caminando + abajo = slide...
                // caminando + no presiono A,D -> idle
                JumpOrWalkCheck(actor, dt);
                break;
            case State.idle:
                JumpOrWalkCheck(actor, dt);
                break;
            case State.jump:
                // no puedo saltar, movimiento horizontal y para abajo modificado?
                // chequeo si estoy en el piso para volver a idle
                if (actor.OnPlatform)
                {
                    actor.SetState(State.idle);
                    jumpTime = 0;
                    jumpsMade = 0;
                    break;
                }
                // si no toque el piso...
                // puedo seguir sumando impulso hacia arriba
                // asi mantener W = salto mas alto
                jumpTime += dt;
                if (Raylib.IsKeyDown(KeyboardKey.W) && jumpTime < maxJumpTime)
                {
                    float vx = actor.MoveVector.VX;
                    jumpTime+=dt;
                    actor.SetVelocity(vx,-128);
                    break;
                    // RETURN / BREAK, SALTAR CORTA TODO
                }
                break;
            case State.fall:
                // similar a jump POR AHORA
                break;
            // attack todavia no
        }
        return true;
    }

    public void JumpOrWalkCheck(Actor actor, float dt)
    {
        if (Raylib.IsKeyPressed(KeyboardKey.W) && actor.OnPlatform)
        {
            jumpsMade++;
            actor.AddVelocity(0,-128);
            actor.SetState(State.jump);
            return;
            //SALTAR CORTA TODO
        }
        int dirX = 0;
        if (Raylib.IsKeyDown(KeyboardKey.A)) 
        {   
            // facing LEFT
            actor.SetState(State.walk);
            dirX = -1;
        }
        if (Raylib.IsKeyDown(KeyboardKey.D))
        {   
            // facing RIGHT
            actor.SetState(State.walk);
            dirX = 1;
        }
        if(dirX != 0) {actor.VectorBodyMovement(dirX, 0);}
        if(dirX == 0)
        // si no quise moverme en X, genero friccion 
        {
            (float vx, float vy) = actor.MoveVector;
            if (Math.Abs(vx) < 32)
            {
                actor.SetVelocity(0,vy);
            } else
            {
                float friction = MathF.Pow(0.0001f, dt); // friccion depende de TIEMPO
                actor.SetVelocity(vx * friction, vy);
            }
            actor.SetState(State.idle);
        }
        return;
    }
}

// se acerca a los puntos de la lista
//public class PatrolBehavior : Behavior