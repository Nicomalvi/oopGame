using System.Numerics;
using Raylib_cs;
public class Program
{
    const int CELL_SIZE    = 32;
    const int SCREEN_W     = 960;
    const int SCREEN_H     = 640;
    public static void Main()
    {
        // inicio raylib, necesario antes que usar gameworld
        Raylib.InitWindow(SCREEN_W, SCREEN_H, "Physics Debug");
        Raylib.SetTargetFPS(60);

        GameWorld gameWorld = new GameWorld(SCREEN_W, SCREEN_H, CELL_SIZE);
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
        gameWorld.LoadLevel(chunk, cols: 17, originX: 0, originY: 0);
        gameWorld.LoadLevel(chunk, cols: 17, originX: 13*32, originY: 0);
        gameWorld.LoadLevel(chunk, cols: 17, originX: 0, originY: 10*32);
        gameWorld.LoadLevel(chunk, cols: 17, originX: 13*32, originY: 10*32);
        float dt;
        while (!Raylib.WindowShouldClose())
        {
            // AL PRINCIPIO DEL GAME LOOP TRAIGO EL FRAME TIME, TODOS TRABAJAN CON EL MISMO
            dt = Raylib.GetFrameTime();
            gameWorld.Update(dt);

            Raylib.BeginDrawing();
            Raylib.ClearBackground(Color.Black);
            gameWorld.Draw();
            Raylib.EndDrawing();
        }
        Raylib.CloseWindow();
    }
}