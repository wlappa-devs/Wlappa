namespace AndroidClient
{
    public static class Constants
    {
#if DEBUG
        public const string ServerIp = "10.0.2.2";
        public const int Port = 5000;
#else
        public const string ServerIp = "132.145.240.13";
        public const int Port = 80;
#endif
    }
}