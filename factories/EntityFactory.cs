using Raylib_cs;

public class EntityFactory
{
    private int cellSize;
    private PhysicsSystem physicsSystem;
    private ActorRegistry actorRegistry;

    private Dictionary<char, Texture2D> textures;
    private SpritePositionSystem spriteSystem;
    private List<AnimationController> animationControllers;

    private Dictionary<char, Action<float, float>> factories;

    public EntityFactory(int cellSize, PhysicsSystem physicsSystem,ActorRegistry actorRegistry, SpritePositionSystem spriteSystem)
    {
        this.cellSize = cellSize;
        this.physicsSystem = physicsSystem;
        this.actorRegistry = actorRegistry;
        this.spriteSystem = spriteSystem;
        this.animationControllers = new List<AnimationController>();

        // cargo todas las texturas a la GPU
        textures = new Dictionary<char, Texture2D>();

        textures['@'] = LoadTextureOnce("spritesheets/basic4x4.png");
        textures['1'] = LoadTextureOnce("spritesheets/wallSprite.png");

        factories = new Dictionary<char, Action<float, float>>
        {
            ['1'] = (x, y) => // pared: solo física + sprite
            {
                var body = new PhysicalEntity(x, y, cellSize, cellSize, false);
                physicsSystem.AddEntity(body);
                
                EntitySprite sprite = new EntitySprite(
                    textures['1'], 
                    new System.Numerics.Vector2(x,y),
                    new Rectangle(0, 0, body.HitboxDimensions.width, body.HitboxDimensions.height));
                spriteSystem.AddEntityAndSprite(body, sprite);
            },
            ['@'] = (x, y) => // player: física + actor + sprite + animación
            {
                var body = new PhysicalEntity(x, y, cellSize/2, cellSize/2, true);
                physicsSystem.AddEntity(body);

                var actor = new Actor(body, new PlayerInputBehavior(), [new MoveAbility(320), new JumpAbility()]);
                actorRegistry.AddActor(actor);

                EntitySprite sprite = new EntitySprite(
                    textures['@'], 
                    new System.Numerics.Vector2(x,y),
                    new Rectangle(0, 0, body.HitboxDimensions.width, body.HitboxDimensions.height));
                spriteSystem.AddEntityAndSprite(body, sprite);
                
                // DATOS ANIMATION CONTROLLER
                List<float> placeholderTimes = [0.16f,0.16f,0.16f,0.16f];
                AnimationTimer idleTimer = new AnimationTimer(placeholderTimes);
                AnimationTimer walkTimer = new AnimationTimer(placeholderTimes);
                AnimationTimer jumpTimer = new AnimationTimer(placeholderTimes);
                AnimationTimer fallTimer = new AnimationTimer(placeholderTimes);

                AnimationPlayer animationPlayer = new AnimationPlayer(
                    [idleTimer,walkTimer,jumpTimer,fallTimer],
                    sprite, body.HitboxDimensions.width, body.HitboxDimensions.height);

                animationControllers.Add(new AnimationController(animationPlayer, actor));
            },
        };
    }

    public void CreateChunk(string chunk, int cols, float originX, float originY)
    {
        int rows = chunk.Length / cols;
        for (int row = 0; row < rows; row++)
            for (int col = 0; col < cols; col++)
            {
                char c = chunk[row * cols + col];
                if (!factories.ContainsKey(c)) continue;
                factories[c](originX + col * cellSize, originY + row * cellSize);
            }
    }

    private Texture2D LoadTextureOnce(string path)
    {
        Image img = Raylib.LoadImage(path);
        Texture2D tex = Raylib.LoadTextureFromImage(img);
        Raylib.UnloadImage(img); // ya la subí, no necesito la copia en RAM
        return tex;
    }

    public void UpdateAnimations(float dt)
    {
        foreach (var controller in animationControllers)
        {
            controller.Tick(dt);
        }
    }

}