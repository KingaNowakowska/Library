namespace Projekt_BD
{
    public class ConnectionDetails
    {
        public string Server { get; set; } = "localhost";
        public string Database { get; set; } = "biblioteka";
        public string Uid { get; set; }
        public string Password { get; set; }

        public string GetConnectionString()
        {
            return $"SERVER={Server};DATABASE={Database};UID={Uid};PASSWORD={Password};";
        }
    }
}
