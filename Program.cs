class Program
{
    public static async Task Main(string[] args)
    {
        DatabaseConnector con = new DatabaseConnector();
        string[] prefixes = {"http://localhost:5050/"};
        HttpListenerServer server = new HttpListenerServer(prefixes,con);
        bool testCSVParser = false;
        bool testCSVAdd = false;

        if (testCSVParser)
        {
            if (testCSVAdd)
            {
                await con.db.Database.EnsureDeletedAsync();
            }
            
            string path = "testSection.csv";
            List<Lot> fromCSV = ParseCSV.ReadFile(path);

            Console.WriteLine("Updating " + fromCSV.Count + " records from " + path + "...");
            con.syncWithCSV(path);
            Console.WriteLine("Done!");
        }
        await con.db.Database.EnsureCreatedAsync();

        Console.WriteLine("Parking Lots with free parking:");
        foreach (CarPark i in con.getFreeParking())
        {
            Console.WriteLine(i);
        }
        Console.WriteLine();

        Console.WriteLine("Parking Lots with night parking:");
        foreach (CarPark i in con.getNightParking())
        {
            Console.WriteLine(i);
        }
        Console.WriteLine();

        float height = 3.21F;
        Console.WriteLine($"Parking Lots which vehicles of height {height} can enter:");
        foreach (CarPark i in con.getMatchHeight(height))
        {
            Console.WriteLine(i);
        }
        Console.WriteLine();

        //Startup http server
        Console.WriteLine("Starting HTTP Server...");
        Console.WriteLine("Press 'q' to quit");

        // Start server in background
        var serverTask = server.StartAsync();
        var clientTask = Client.Start();

        // Wait for quit command
        while (true)
        {
            var key = Console.ReadKey(true);
            if (key.KeyChar == 'q' || key.KeyChar == 'Q')
                break;
        }

        Console.WriteLine("Stopping server...");
        server.Stop();

        try
        {
            await serverTask;
        }
        catch (Exception ex)
        {
            Console.WriteLine("Server stopped with an exception."+ex.Message);
        }

        Console.WriteLine("Server stopped.");
    }
}

