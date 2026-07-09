using System.Numerics;

public class SpritePositionSystem
{
    private List<(PhysicalEntity phys, EntitySprite sprite)> entities;

    public SpritePositionSystem()
    {
        entities = new List<(PhysicalEntity, EntitySprite)>();
    }
    public void AddEntityAndSprite(PhysicalEntity phys, EntitySprite sprite)
    {
        entities.Add((phys,sprite));
    }
    public void Remove(PhysicalEntity phys)
    {
        entities.RemoveAll(p => p.phys == phys);
    }
    public void UpdateSpritePositions()
    {
        foreach (var entity in entities)
        {
            (float x, float y) = entity.phys.PosVector;
            Vector2 newPos = new Vector2(x, y);
            entity.sprite.position = newPos;
        }
    }

    // PLACEHOLDER si actualizar posiciones se hace al final del loop, puedo aprovechar para ir dibujando?
    public void UpdateAndDraw()
    {
        foreach (var entity in entities)
        {
            (float x, float y) = entity.phys.PosVector;
            Vector2 newPos = new Vector2(x, y);
            entity.sprite.position = newPos;
            entity.sprite.Draw();
        }
    }
}