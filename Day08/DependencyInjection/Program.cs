namespace DependencyInjection
{
    public class Hammer
    {
        public void Use()
        {
            System.Console.WriteLine("Hammering the Nails!!");
        }
    }

    public class Saw
    {
        public void Use()
        {
            System.Console.WriteLine("Sawing the Furniture");
        }
    }

    public class Builder
    {
        private Hammer _hammer;
        private Saw _saw;

        // Constructor Dependency Injection
        public Builder(Hammer hammer, Saw saw)
        {
            _hammer = hammer;
            _saw = saw;
        }

        public void BuildHouse()
        {
            _hammer.Use();
            _saw.Use();
            System.Console.WriteLine("House Built");
        }
    }

    public class Program
    {
        public static void Main(string[] args)
        {
            Hammer hammer = new Hammer();
            Saw saw = new Saw();

            Builder builder = new Builder(hammer, saw);
            builder.BuildHouse();
        }
    }
}
