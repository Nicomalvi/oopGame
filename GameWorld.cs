public class GameWorld
{
    private PhysicsSystem physSys;
    private ActorRegistry actorRegistry;

    private SpritePositionSystem spriteSystem;
    private EntityFactory entityFactory;

    public GameWorld(int width, int height, int cellSize)
    {
        var map = new MapGrid(width, height, cellSize);
        physSys = new PhysicsSystem(map);
        actorRegistry = new ActorRegistry();
        spriteSystem = new SpritePositionSystem();
        entityFactory = new EntityFactory(cellSize, physSys, actorRegistry, spriteSystem);
    }

    public void LoadLevel(string chunk, int cols, float originX, float originY)
        => entityFactory.CreateChunk(chunk, cols, originX, originY);

    public void Update(float dt)
    {
        actorRegistry.Update(dt);
        physSys.Update(dt);

        // la factory creó las animaciones, no hay razón para que vivan en otro lado
        entityFactory.UpdateAnimations(dt);
    }

    public void Draw() => spriteSystem.UpdateAndDraw();
}