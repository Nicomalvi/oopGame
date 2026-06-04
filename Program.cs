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
        string wallRow = "111111111111111111111111111111";
        string[][]chunks = [[wallRow]];
        List<PhysicalEntity> physEntities = map.CreateChunks(chunks,0,0,1,30);
        physEntities.AddRange(map.CreateChunks(chunks,map.Rows-2,0,1,30));
        physEntities.AddRange(map.CreateChunks(chunks,0,0,20,1));
        physEntities.AddRange(map.CreateChunks(chunks,0,29,20,1));

        Raylib.InitWindow(SCREEN_W, SCREEN_H, "Physics Debug");
        Raylib.SetTargetFPS(60);
        float dt;

        // creo al player y una box 
        PhysicalEntity playerBody = new PhysicalEntity(32,31,map,16,16,true);
        physEntities.Add(playerBody); // placeholder

        PhysicalEntity boxBody = new PhysicalEntity(64,64,map,32,32,false);
        physEntities.Add(boxBody);

        Behavior playerBehavior = new PlayerInputBehavior();
        Actor playerActor = new Actor(playerBody, 640, playerBehavior);

        List<Actor> actors = new List<Actor>();
        actors.Add(playerActor);
        while (!Raylib.WindowShouldClose())
        {
            // AL PRINCIPIO DEL GAME LOOP TRAIGO EL FRAME TIME, TODOS TRABAJAN CON EL MISMO
            dt = Raylib.GetFrameTime();
            //===================================================================================================================
            // actuan todos
            //===================================================================================================================
            foreach (Actor actor in actors)
            {
                actor.Update();
            }
            foreach (PhysicalEntity entity in physEntities)
            {
                entity.Update(dt);
            }
            //===================================================================================================================
            // renders
            //===================================================================================================================
            Raylib.BeginDrawing();
            Raylib.ClearBackground(Color.Black);
            // grid de referencia (opcional — comentar si molesta)
            for (int col = 0; col <= width; col += CELL_SIZE)
                Raylib.DrawLine(col, 0, col, height, new Color(40, 40, 40, 255));
            for (int row = 0; row <= height; row += CELL_SIZE)
                Raylib.DrawLine(0, row, width, row, new Color(40, 40, 40, 255));
            // hitboxes PLACEHOLDER
            foreach (var ent in physEntities)
                ent.DrawDebug();
            Raylib.EndDrawing();
        }
        Raylib.CloseWindow();
    }
}