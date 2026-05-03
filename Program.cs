using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

class DatabaseConnector
{
    public ParkingContext db = new ParkingContext();

    public DatabaseConnector()
    {
        Console.WriteLine($"Database path: {db.DbPath}.");
    }

    public void printFreeParking()
    {
        var res = db.FREE_PARK.Select(m => m.car_park_noFP.car_park_no).Distinct();
        foreach (string item in res)
        {
            Console.WriteLine(item);
        }
    }

    public void printNightParking()
    {
        var res = db.CARPARK.Where(m => m.night_parking);
        foreach (CarPark item in res)
        {
            Console.WriteLine(item.car_park_no);
        }
    }

    public void printMatchHeight(float h)
    {
        var res = db.CARPARK.Where(m => m.gantry_height < h);
        foreach (CarPark item in res)
        {
            Console.WriteLine(item.car_park_no);
        }
    }

    public void printSystem()
    {
        var res = db.SYSTEM;
        foreach (Sys x in res)
        {
            Console.WriteLine(x.system_name);
        }
    }

    public async Task SeedDb()
    {
        Sys system1 = new Sys
        {
            system_id = 1,
            system_name = "testS"
        };
        
        Typ type1 = new Typ
        {
            type_id = 1,
            type_name = "testT"
        };

        CarPark cp1 = new CarPark
        {
            car_park_no = "A1",
            night_parking = true,
            gantry_height = 3.5F,
            ParkingSystems = [system1],
            LotTypes = [type1],

            address = "",
            x_coord = 0F,
            y_coord = 0F,
            short_term_end = "",
            short_term_start = "",
            decks = 0,
            basement = 0
        };

        FreePark fp1 = new FreePark
        {
            free_park_instance_id = 1,
            day = 0,
            start_time = "",
            end_time = "",
            car_park_noFP = cp1
        };

        db.Add(system1);
        db.Add(type1);
        db.Add(cp1);
        db.Add(fp1);

        await db.SaveChangesAsync();
    }

    public static async Task Main(string[] args)
    {
        DatabaseConnector con = new DatabaseConnector();

        await con.db.Database.EnsureDeletedAsync();
        await con.db.Database.EnsureCreatedAsync();

        await con.SeedDb();
        con.printSystem();
        con.printFreeParking();
        con.printMatchHeight(2.2F);
        con.printNightParking();
    }
}