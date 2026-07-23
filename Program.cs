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

        //============================= CREACION DE MAPA, PHYS_SYSTEM =============================
        MapGrid map = new MapGrid(width,height,CELL_SIZE);
        PhysicsSystem physSys = new PhysicsSystem(map);
        EntityFactory entityFactory = new EntityFactory(CELL_SIZE, physSys);
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
        //==============================================================================================

        //============================= CREO EL MAPA ===================================================
        entityFactory.CreateChunk(chunk, cols: 17, originX: 0, originY: 0);
        entityFactory.CreateChunk(chunk, cols: 17, originX: 13*32, originY: 0);
        entityFactory.CreateChunk(chunk, cols: 17, originX: 0, originY: 10*32);
        entityFactory.CreateChunk(chunk, cols: 17, originX: 13*32, originY: 10*32);
        //==============================================================================================
        
        //============================= INICIALIZACION RAYLIB ==========================================
        Raylib.InitWindow(SCREEN_W, SCREEN_H, "Physics Debug");
        Raylib.SetTargetFPS(60);
        float dt;
        //==============================================================================================
        
        //============================== CREACIÓN DEL PLAYER ===========================================
        PhysicalEntity playerBody = new PhysicalEntity(64,64,16,16,true);
        Behavior playerBehavior = new PlayerInputBehavior();
        Actor playerActor = new Actor(playerBody, playerBehavior,[new MoveAbility(320), new JumpAbility()]);

        List<Actor> actors = new List<Actor>();
        actors.Add(playerActor);

        physSys.AddEntity(playerBody); // placeholder para drawDebug

        // PLACEHOLDER
        // SPRITES DEL PLAYER
        Image playerSpriteSheet = Raylib.LoadImage("4anims4framesTest.png");
        Texture2D playerTexture = Raylib.LoadTextureFromImage(playerSpriteSheet);

        Vector2 playerSpritePos = new Vector2(playerActor.PosVector.X, playerActor.PosVector.Y);
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
        //==============================================================================================

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

            // DIBUJO HITBOXES
            foreach (var ent in physSys.Entities)
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