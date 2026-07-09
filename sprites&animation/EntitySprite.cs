using System.Numerics;
using Raylib_cs;
public class EntitySprite
{
    private Texture2D texture;
    public Vector2 position;
    public Rectangle source;     // parte de la textura que uso (voy a usar sprite sheet)

    public EntitySprite(Texture2D texture, Vector2 position, Rectangle source)
    {
        this.texture = texture;
        this.position = position;   
        this.source = source;
    }
    public void Draw()
    {
        Raylib.DrawTextureRec(texture, source, position, Color.White);
    }
}