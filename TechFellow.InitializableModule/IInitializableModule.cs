namespace TechFellow.InitializableModule
{
    public interface IInitializableModule
    {
        void Initialize();

        void Release();
    }
}
