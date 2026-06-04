using Raylib_cs;
public abstract class Behavior
// behavior = funcion que guarda información y modifica a los actores
// funcionan como una state machine
{
    public abstract bool Execute(Actor actor);
}

// =====================================================================================================================
// defino behaviors que voy a usar
// =====================================================================================================================

// playerBehavior = hacer lo que diga el input
public class PlayerInputBehavior : Behavior
{
    // recuerdo hace cuanto estoy tocando Jump, asi el largo del salto es variable
    int jumpHeld = 0;
    // en un futuro deberia haber un bool tambien para diferenciar entre double jump y long jump

    public override bool Execute(Actor actor)
    {
        // si estoy idle o walk puedo tocar cualquier boton
        // si estoy falling o jump no puedo jump, otras dir. actuan distinto
        switch (actor.State)
        {
            // quizas es mejor meter break en cada casao y posterior al switch meter
            // el codigo que iria SIEMPRE independiente del estado
            // (o eso va en Actor.Update()?)
            case State.walk:
                // lo mismo que idle POR AHORA, por eso no uso el break
                // break;

                // en un futuro: caminando + tecla ataque pasa algo, caminando + abajo = slide...
            case State.idle:
                // leer input teclado
                // si comienzo a moverme, ir aplicando velocidad de a poco para que haya aceleracion
                // dependiendo de que haga modifico o no el state
                if (Raylib.IsKeyPressed(KeyboardKey.W) && actor.OnPlatform)
                {
                    jumpHeld = 1;
                    actor.AddVelocity(0,-960);
                    actor.SetState(State.jump);
                    break;
                    // RETURN / BREAK, SALTAR CORTA TODO
                }
                if (Raylib.IsKeyDown(KeyboardKey.S))
                {
                    //crouch...
                }
                int dirX = 0;
                if (Raylib.IsKeyDown(KeyboardKey.A)) 
                {   
                    // facing LEFT
                    // velocidad NEGATIVA
                    actor.SetState(State.walk);
                    dirX = -1;
                }
                if (Raylib.IsKeyDown(KeyboardKey.D))
                {   
                    // facing RIGHT
                    // velocidad POSITIVA
                    actor.SetState(State.walk);
                    dirX = 1;
                }
                actor.MoveTowardsVector(dirX,0); // asi en caso de que sea 0, causo friccion al frenar
                break;
            case State.jump:
                // no puedo saltar, movimiento horizontal y para abajo modificado?
                // chequeo si estoy en el piso para volver a idle
                if (actor.OnPlatform)
                {
                    actor.SetState(State.idle);
                    jumpHeld = 0;
                    break;
                }
                // si no toque el piso...
                // puedo seguir sumando impulso hacia arriba
                // asi mantener W = salto mas alto
                if (Raylib.IsKeyDown(KeyboardKey.W) && jumpHeld < 6)
                {
                    jumpHeld++;
                    actor.AddVelocity(0,-960);
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
}

// se acerca a los puntos de la lista
//public class PatrolBehavior : Behavior