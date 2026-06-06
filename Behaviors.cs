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
    // recuerdo hace cuanto estoy tocando Jump, asi el largo del salto es variable
    // si suelto boton no puedo volver a variar jump height, pasa a ser double jump
    float jumpTime = 0;
    bool stoppedJumpHeight = false;
    int jumpsMade = 0;
    
    // menor tiempo, mayor cant. pixeles = salto "snappy" veloz
    // mayor tiempo, menor cant. pixeles = salto mas lento, controlable
    // estas cosas también dependen de GRAVITY
    float jumpHeldTime = 0.2f;
    float pixelsPerJumpTick = 32*8;

    // si toco W y suelto -> initialJumpSpeed
    // luego tengo opcion de mantener para algunos pixeles mas
    float initalJumpSpeed = 32*(16+8); // PLACEHOLDER: 32*16 = GRAVITY 

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

                HorizontalMoveCheck(actor,dt,actor.MoveSpeed);
                JumpCheck(actor);
                break;
            case State.idle:
                HorizontalMoveCheck(actor,dt,actor.MoveSpeed);
                JumpCheck(actor);
                break;
            case State.jump:
                // no puedo saltar, movimiento horizontal y para abajo modificado?
                // chequeo si estoy en el piso para volver a idle
                if (actor.OnPlatform)
                {
                    actor.SetState(State.idle);
                    jumpTime = 0;
                    jumpsMade = 0;
                    stoppedJumpHeight = false;
                    break;
                }
                // si no toque el piso...
                // mantener W = salto mas alto
                // o puedo moverme horizontalmente, pero más lento
                HorizontalMoveCheck(actor,dt,actor.MoveSpeed * 0.60f); //placeholder: 60% velocidad normal
                jumpTime += dt;
                if (Raylib.IsKeyDown(KeyboardKey.W) && jumpTime < jumpHeldTime && !stoppedJumpHeight)
                {
                    float vx = actor.MoveVector.VX;
                    jumpTime+=dt;
                    actor.SetVelocity(vx, -pixelsPerJumpTick);
                    break;//SALTAR CORTA TODO
                } 
                stoppedJumpHeight = true;
                break;//SALTAR CORTA TODO
            case State.fall:
                break;
            // attack todavia no
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
        if(dirX != 0) {MoveHorizontal(actor,dirX,moveSpeed);}
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
            if(IsGrounded(actor))
            {
                actor.SetState(State.idle);
            }
        }
    }
    private void JumpCheck(Actor actor)
    {
        if (Raylib.IsKeyPressed(KeyboardKey.W) && actor.OnPlatform)
        {
            jumpsMade++;
            actor.AddVelocity(0,-initalJumpSpeed);
            actor.SetState(State.jump);
        }
    }
    private void MoveHorizontal(Actor actor, int direction, float speed)
    {
        (float vx, float vy) = actor.MoveVector;
        // si me llega una dirección opuesta a la que iba hago un giro rapido
        // en vez de seguir yendo pero un poco mas lento hacia donde ya no quiero
        if (direction != 0 && Math.Sign(direction) != Math.Sign(vx))
            vx = 0;
        
        // si el movimiento esta iniciando, simulo aceleracion (voy de a multiplos de 64)
        vx = Math.Clamp(vx + direction*64, -speed, speed);
        actor.SetVelocity(vx,vy);
    }
    private bool IsOnAir(Actor actor)
    {
        return actor.State == State.jump || actor.State == State.fall;
    }
    private bool IsGrounded(Actor actor)
    {
        return actor.State == State.idle || actor.State == State.walk;
    }
}

// se acerca a los puntos de la lista
//public class PatrolBehavior : Behavior