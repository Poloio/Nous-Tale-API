namespace Nous_Tale_API.Model
{
    public static class DbInitializer
    {
        public static void Initialize(NousContext context)
        {
            context.Database.EnsureCreated();
        }
    }
}
