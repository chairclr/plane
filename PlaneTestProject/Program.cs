using plane;

namespace PlaneTestProject;

public class Program
{
    public static void Main(string[] args)
    {
        using TestPlaneGame game = new TestPlaneGame("Epic Window");

        game.Run();
    }
}