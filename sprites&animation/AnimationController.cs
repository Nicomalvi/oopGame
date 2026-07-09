public class AnimationController
// mediador entre animaciones y actores
{
    private AnimationPlayer player;
    private Actor actor;
    private Action latestAction;

    public AnimationController(AnimationPlayer player, Actor actor)
    {
        this.player = player;
        this.actor = actor;
        latestAction = actor.Action;
    }

    public void Tick(float dt)
    {
        // cada tick avanzo la animacion y chequeo si debo cambiarla
        player.Tick(dt);
        if(actor.Action != latestAction)
        {
            player.ChangeAnimation(actor.Action);
            latestAction = actor.Action;
        }
    }
}