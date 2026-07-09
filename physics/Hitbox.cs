public class Hitbox
// Hitbox = por ahora rectangulos
{
    private float xOffset;
    private float yOffset;
    private float width;
    private float height;
    public Hitbox(float width, float height, float xOffset = 0, float yOffset = 0)
    {
        this.width = width;
        this.height = height;
        this.xOffset = xOffset;
        this.yOffset = yOffset;
    }
    public (float x,float y) Offsets => (xOffset, yOffset);
    public (float x, float y) Dimensions => (width, height);
}