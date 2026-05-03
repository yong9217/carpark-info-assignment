using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Text.Json;

//Proxy for a client making API calls
class Client
{
    private int port;
    public Client(int p)
    {
        port = p;
    }

    public async Task Start()
    {
        //Make a call to each of the APIs

        //Parameter that sets the value requested by the client for gantry height
        double height = 3.2;

        var client = new HttpClient();
        var response = await client.GetAsync($"http://localhost:{port}/FreeParking");
        // Check if the request was successful
        if (response.IsSuccessStatusCode)
        {
            var responseJson = await response.Content.ReadFromJsonAsync<List<Lot>>();

            while(responseJson == null){}

            //Log the received result from the API to console
            Console.WriteLine("Requested Free Parking");
            foreach (Lot cp in responseJson)
            {
                Console.WriteLine(cp);
            }
            Console.WriteLine();
        } else
        {
            Console.WriteLine("Request was unsuccessful");
        }

        response = await client.GetAsync($"http://localhost:{port}/NightParking");
        if (response.IsSuccessStatusCode)
        {
            var responseJson = await response.Content.ReadFromJsonAsync<List<Lot>>();

            while(responseJson == null){}


            Console.WriteLine("Requested Night Parking");
            foreach (Lot cp in responseJson)
            {
                Console.WriteLine(cp);
            }
            Console.WriteLine();
        } else
        {
            Console.WriteLine("Request was unsuccessful");
        }

        response = await client.GetAsync($"http://localhost:{port}/gantry/height/" + height);
        if (response.IsSuccessStatusCode)
        {
            var responseJson = await response.Content.ReadFromJsonAsync<List<Lot>>();

            while(responseJson == null){}

            Console.WriteLine("Requested Enterable Parking with height " + height);
            foreach (Lot cp in responseJson)
            {
                Console.WriteLine(cp);
            }
            Console.WriteLine();
        } else
        {
            Console.WriteLine("Request was unsuccessful");
        }
    }
}