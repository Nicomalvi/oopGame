using Raylib_cs;

public class AnimationPlayer
{
    private List<AnimationTimer> timers;
    private EntitySprite sprite;
    private float frameWidth;
    private float frameHeight;
    private int widthSign;
    private ActorState currentAction;

    public AnimationPlayer(List<AnimationTimer> timers, EntitySprite sprite, float frameWidth, float frameHeight)
    {
        this.timers = timers;
        this.sprite = sprite;
        this.frameWidth = frameWidth;
        this.frameHeight = frameHeight;
        currentAction = ActorState.idle;
        widthSign = 1;
    }

    public void Tick(float dt)
    {
        if (timers[(int)currentAction].TickAndFrameChange(dt))
        {
            UpdateSource();
        }
    }

    public void Reset()
    {
        timers[(int)currentAction].Reset();
        UpdateSource();
    }

    public void ChangeAnimation(ActorState newAction)
    {
        if (newAction == currentAction) // podria ir en changeAction del actor, a veces quizas quiero reiniciar animaciones
            return;

        currentAction = newAction;
        timers[(int)currentAction].Reset();
        UpdateSource();
    }

    public void SetHorizontalFacing(HorizontalFacing newDir)
    {
        widthSign = newDir == HorizontalFacing.left ? -1 : 1;
    }

    private void UpdateSource()
    {
        sprite.source = new Rectangle(
            timers[(int)currentAction].CurrentFrame * frameWidth,
            (int)currentAction * frameHeight,
            frameWidth * widthSign,
            frameHeight
        );
    }
}