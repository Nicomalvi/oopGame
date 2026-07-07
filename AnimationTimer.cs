using Raylib_cs;

public class AnimationTimer
{
    public List<float> frameDurations; // cuantos seg. me quedo en cada frame?
    private float currentFrameDuration;
    private int currentFrame;
    private bool animationFinished;
    
    public AnimationTimer(List<float> frameDurations, float currentFrameDuration, int currentFrame)
    {
        this.frameDurations = frameDurations;
        this.currentFrameDuration = currentFrameDuration;
        this.currentFrame = currentFrame;
        animationFinished = false;
    }
    
    public bool TickAndFrameChange(float dt)
    {
        currentFrameDuration += dt;
        if(currentFrameDuration >= frameDurations[currentFrame])
        {
            currentFrame++;
            if(currentFrame > frameDurations.Count)
            {
                currentFrame = 0; // por defecto loopeo animacion al terminar
                animationFinished = true;
            }
            currentFrameDuration = 0;
            return true;
        }
        return false;
    }

    public void Reset()
    {
        currentFrame = 0;
        currentFrameDuration = 0;
        animationFinished = false;
    }

    public int CurrentFrame => currentFrame;
    public bool AnimationFinished => animationFinished;
}