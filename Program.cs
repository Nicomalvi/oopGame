using System.Numerics;
using Raylib_cs;
public class Program
{
    const int CELL_SIZE    = 32;
    const int SCREEN_W     = 960;
    const int SCREEN_H     = 640;
    public static void Main()
    {
        int width = SCREEN_W;
        int height = SCREEN_H;

        // creo las paredes del nivel inicial
        MapGrid map = new MapGrid(width,height,CELL_SIZE);

        string chunk =
        "111111111111111 1" +
        "1               1" +
        "1   @         111" +
        "1           1  11" +
        "1          111111" +
        "                 " +
        "1       11    111" +
        "1     1         1" +
        "1  111111111111 1" +
        "111111111111111 1";
        var physEntities = map.CreateChunks(chunk, cols: 17, originX: 0, originY: 0);
        physEntities.AddRange(map.CreateChunks(chunk, cols: 17, originX: 13*32, originY: 0));
        physEntities.AddRange(map.CreateChunks(chunk, cols: 17, originX: 0, originY: 10*32));
        physEntities.AddRange(map.CreateChunks(chunk, cols: 17, originX: 13*32, originY: 10*32));

        Raylib.InitWindow(SCREEN_W, SCREEN_H, "Physics Debug");
        Raylib.SetTargetFPS(60);
        float dt;

        PhysicalEntity playerBody = new PhysicalEntity(64,64,16,16,true);
        physEntities.Add(playerBody); // placeholder

        Behavior playerBehavior = new PlayerInputBehavior();
        Actor playerActor = new Actor(playerBody, playerBehavior,[new MoveAbility(320), new JumpAbility()]);

        List<Actor> actors = new List<Actor>();
        actors.Add(playerActor);

        PhysicsSystem physSys = new PhysicsSystem(map);
        physSys.AddEntity(playerBody);
        foreach (PhysicalEntity phys in physEntities)
        {
            physSys.AddEntity(phys);
        }

        // PLACEHOLDER
        // SPRITES DEL PLAYER
        Image playerSpriteSheet = Raylib.LoadImage("4anims4framesTest.png");
        Texture2D playerTexture = Raylib.LoadTextureFromImage(playerSpriteSheet);

        Vector2 playerSpritePos = new Vector2(playerActor.PosVector.X, playerActor.PosVector.Y);
        Vector2 playerSpriteSize = new Vector2(playerBody.HitboxDimensions.width, playerBody.HitboxDimensions.height);
        Rectangle initialPlayerSource = new Rectangle(0, 0, 16, 16);

        EntitySprite playerSprite = new EntitySprite(playerTexture, playerSpritePos, initialPlayerSource);

        // ANIMACIONES DEL PLAYER
        List<float> placeholderTimes = [0.16f,0.16f,0.16f,0.16f];
        AnimationTimer idleTimer = new AnimationTimer(placeholderTimes);
        AnimationTimer walkTimer = new AnimationTimer(placeholderTimes);
        AnimationTimer jumpTimer = new AnimationTimer(placeholderTimes);
        AnimationTimer fallTimer = new AnimationTimer(placeholderTimes);

        AnimationPlayer playerAnimationPlayer = new AnimationPlayer(
            [idleTimer,walkTimer,jumpTimer,fallTimer],
            playerSprite, playerBody.HitboxDimensions.width, playerBody.HitboxDimensions.height);

        AnimationController playerAnimationController = new AnimationController(playerAnimationPlayer, playerActor);

        // SPRITE SYSTEM    
        SpritePositionSystem spritePositionSystem = new SpritePositionSystem();
        spritePositionSystem.AddEntityAndSprite(playerBody,playerSprite);

        while (!Raylib.WindowShouldClose())
        {
            // AL PRINCIPIO DEL GAME LOOP TRAIGO EL FRAME TIME, TODOS TRABAJAN CON EL MISMO
            dt = Raylib.GetFrameTime();
            //===================================================================================================================
            // actuan todos
            //===================================================================================================================

            // orden importante: primero actuan ACTORS
            // entonces x ejemplo player decide saltar, velocidad en Y < 0
            // LUEGO actuan fisicas: si velocidad en Y >= 0, aplicar gravedad constante
            
            foreach (Actor actor in actors)
            {
                actor.Update(dt);
            }
            physSys.UpdatePhysics(dt);
            
            playerAnimationController.Tick(dt);

            //===================================================================================================================
            // renders PLACEHOLDER
            //===================================================================================================================
            Raylib.BeginDrawing();
            Raylib.ClearBackground(Color.Black);

            // grid de referencia
            // for (int col = 0; col <= width; col += CELL_SIZE)
            //    Raylib.DrawLine(col, 0, col, height, new Color(40, 40, 40, 255));
            // for (int row = 0; row <= height; row += CELL_SIZE)
            //    Raylib.DrawLine(0, row, width, row, new Color(40, 40, 40, 255));

            // hitboxes PLACEHOLDER
            foreach (var ent in physEntities)
                if(ent!=playerBody){ent.DrawDebug();}

            // FUTURO RENDER REAL 
            spritePositionSystem.UpdateAndDraw();
            Raylib.EndDrawing();


            //debug
            Console.WriteLine(actors[0].Action);
        }
        Raylib.CloseWindow();
    }
}