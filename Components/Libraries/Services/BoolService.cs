namespace WindowsServerManager.Components.Libraries.Services
{
    public static class BoolService
    {
        public static void Toggle(ref this bool value) => value = !value;
    }
}
