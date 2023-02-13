using plane;

namespace PlaneTestProject;

public class Program
{
    public static void Main(string[] args)
    {
        TestPlaneGame game = new TestPlaneGame("Plane Test Project");

        game.Run();

        game.Dispose();
    }
}