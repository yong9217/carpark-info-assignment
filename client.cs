using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Text.Json;

class Client
{
    public static async Task Start()
    {
        var client = new HttpClient();
        var response = await client.GetAsync("http://localhost:5050/FreeParking");
        // Check if the request was successful
        if (response.IsSuccessStatusCode)
        {
            var responseJson = await response.Content.ReadFromJsonAsync<List<Lot>>();

            while(responseJson == null){}

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

        response = await client.GetAsync("http://localhost:5050/NightParking");
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

        double height = 3.2;
        response = await client.GetAsync("http://localhost:5050/gantry/height/" + height);
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