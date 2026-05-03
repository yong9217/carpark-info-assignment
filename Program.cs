class Program
{
    public static async Task Main(string[] args)
    {
        //Setup parameters
        int port = 5050;
        string path = "hdb-carpark-information-20220824010400.csv";
        DatabaseConnector con = new DatabaseConnector();
        string[] prefixes = {$"http://localhost:{port}/"};
        HttpListenerServer server = new HttpListenerServer(prefixes,con);
        float height = 3.21F; //Set to change what height function calls (different from the value the API calls, go to client.cs to change that value)
        bool testCSVParser = true; //Set to test the Parser works by updating the database / creating if it does not exist
        bool testCSVAdd = true;    //Set to test the Parser can create and initialize the database by deleting it prior
        bool startAPI = false;     //Set to test the API calls

        if (testCSVParser)
        {
            if (testCSVAdd)
            {
                await con.db.Database.EnsureDeletedAsync();
            }
            await con.db.Database.EnsureCreatedAsync();
            List<Lot> fromCSV = ParseCSV.ReadFile(path);

            Console.WriteLine("Updating " + fromCSV.Count + " records from " + path + "...");
            con.syncWithCSV(path);
            Console.WriteLine("Done!");
        }
    
        //Showing the API calls
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

        Console.WriteLine($"Parking Lots which vehicles of height {height} can enter:");
        foreach (CarPark i in con.getMatchHeight(height))
        {
            Console.WriteLine(i);
        }
        Console.WriteLine();

        if (startAPI)
        {
            //Startup http server
            var serverTask = server.StartAsync();
            var clientTask = new Client(port);
            await clientTask.Start();

            Console.WriteLine("Starting HTTP Server...");
            Console.WriteLine("Press 'q' to quit");

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
}

