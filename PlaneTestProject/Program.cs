using plane;

namespace PlaneTestProject;

public class Program
{
    public static void Main(string[] args)
    {
        using Plane game = new Plane("Epic Window");

        game.Run();
    }
}
