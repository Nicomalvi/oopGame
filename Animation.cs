using Raylib_cs;

public class Animation
{
    private AnimationTimer timer;
    private EntitySprite sprite;
    private float frameWidth;
    private float frameHeight;
    private float sheetRow;

    public Animation(AnimationTimer timer, EntitySprite sprite, float frameWidth, float frameHeight, float sheetRow)
    {
        this.timer = timer;
        this.sprite = sprite;
        this.frameWidth = frameWidth;
        this.frameHeight = frameHeight;
        this.sheetRow = sheetRow;
    }

    public void Tick(float dt)
    {
        if (timer.TickAndFrameChange(dt))
        {
            UpdateSource();
        }
    }

    public void Reset()
    {
        timer.Reset();
        UpdateSource();
    }

    private void UpdateSource()
    {
        sprite.source = new Rectangle(
            timer.CurrentFrame * frameWidth,
            sheetRow * frameHeight,
            frameWidth,
            frameHeight
        );
    }
}