using Microsoft.EntityFrameworkCore;

public class DatabaseConnector
{
    //Import database structure
    public ParkingContext db = new ParkingContext();

    public DatabaseConnector()
    {
        Console.WriteLine($"Database path: {db.DbPath}."); //Report database file path
    }

    //Getter functions for the user stories
    public CarPark[] getFreeParking()
    {
        var res = db.CARPARK.Include(x => x.LotTypes).Include(y => y.ParkingSystems).Include(z => z.FreeParkingSessions).Where(z => z.FreeParkingSessions.Count > 0);

        return res.ToArray();
    }

    public CarPark[] getNightParking()
    {
        var res = db.CARPARK.Include(x => x.LotTypes).Include(y => y.ParkingSystems).Include(z => z.FreeParkingSessions).Where(z => z.night_parking);

        return res.ToArray();
    }

    public CarPark[] getMatchHeight(float h)
    {
        var res = db.CARPARK.Include(x => x.LotTypes).Include(y => y.ParkingSystems).Include(z => z.FreeParkingSessions).Where(z => z.gantry_height < h);

        return res.ToArray();
    }

    //Convert a Lot into a CarPark
    //Different object formats are used as CarPark has circular references due to parking system, lot type and free parking periods which HTTPClient does not like
    private CarPark LotToCarPark(Lot l)
    {
        CarPark output = new CarPark
        {
            car_park_no = l.car_park_no,
            address = l.address,
            x_coord = l.x_coord,
            y_coord = l.y_coord,
            short_term_end = l.short_term_end,
            short_term_start = l.short_term_start,
            night_parking = l.night_parking,
            decks = l.decks,
            gantry_height = l.gantry_height,
            basement = l.basement,
            LotTypes = TypesToLotTypes(l.types),
            ParkingSystems = SystemsToSys(l.systems),
        };
        output.FreeParkingSessions = FreeSessionsToFreePark(l.free, output);

        return output;
    }

    //Process all lot types in Lot to lot types in CarPark
    //Will automatically add missing lots not found in the database
    private ICollection<Typ> TypesToLotTypes(string[] xs)
    {
        ICollection<Typ> output = new List<Typ>{};

        var total = db.TYPE.Count();

        foreach (string lt in xs)
        {
            var res = db.TYPE.Where(m => m.type_name == lt).FirstOrDefault();

            if(res == null)
            {
                total = total + 1;

                output.Add(addType(lt,total));
            } else
            {
                output.Add(res);
            }
        }

        return output;
    }

    //Process all system types in Lot to system types in CarPark
    //Will automatically add missing systems not found in the database
    private ICollection<Sys> SystemsToSys(string[] xs)
    {
        ICollection<Sys> output = new List<Sys>{};

        var total = db.SYSTEM.Count();

        foreach (string sys in xs)
        {
            var res = db.SYSTEM.Where(m => m.system_name == sys).FirstOrDefault();

            if(res == null)
            {
                total = total + 1;
                output.Add(addSystem(sys,total));
            } else
            {
                output.Add(res);
            }
        }

        return output;
    }

    //Process all free parking periods in Lot to free parking periods in CarPark
    //Will automatically add missing free parking periods not found in the database
    private ICollection<FreePark> FreeSessionsToFreePark(FreeSession[] xs, CarPark source)
    {
        ICollection<FreePark> output = new List<FreePark>{};

        var total = db.FREE_PARK.Count();

        foreach (FreeSession fs in xs)
        {
            var res = db.FREE_PARK.Where(m => m.car_park_noFP.car_park_no == source.car_park_no && m.day == fs.day && m.start_time == fs.session.start && m.end_time == fs.session.end).FirstOrDefault();

            if(res == null)
            {
                total = total + 1;

                FreePark newSession = new FreePark
                {
                    free_park_instance_id = total,
                    car_park_noFP = source,
                    day = fs.day,
                    start_time = fs.session.start,
                    end_time = fs.session.end
                };

                output.Add(newSession);
            } else
            {
                output.Add(res);
            }
        }

        return output;
    }

    //Sync a single lot from the CSV with the database
    private void syncSingleLotWithCSV(Lot l)
    {
        var res = db.CARPARK.Where(m => m.car_park_no == l.car_park_no);
        
        if(res.Count() <= 0) //Lot does not exist in database, have to add
        {
            Console.WriteLine("Adding...");
            db.CARPARK.Add(LotToCarPark(l));

        } else //Lot does exist in database, update instead
        {
            Console.WriteLine("Updating...");
            updateToCSV(l, res.First());
        }

        db.SaveChanges();
    }

    //Perform the heavy work of matching the CSV with the database
    private void updateToCSV(Lot l, CarPark cp)
    {
        //skip car_park_no
        cp.address = l.address;
        cp.basement = l.basement;
        cp.decks = l.decks;
        cp.x_coord = l.x_coord;
        cp.y_coord = l.y_coord;
        cp.short_term_end = l.short_term_end;
        cp.short_term_start = l.short_term_start;
        cp.night_parking = l.night_parking;
        cp.gantry_height = l.gantry_height;
        
        //Have to run includes from database otherwise types, systems and free parking won't be loaded
        var loadTypeIncl = db.CARPARK.Include(z => z.LotTypes).Where(x => x.car_park_no == cp.car_park_no);
        var loadSystemIncl = db.CARPARK.Include(z => z.ParkingSystems).Where(x => x.car_park_no == cp.car_park_no);
        var loadFreeIncl = db.CARPARK.Include(z => z.FreeParkingSessions).Where(x => x.car_park_no == cp.car_park_no);

        //Make a copy of types, systems and free parking from database
        List<string> loadTypes = new List<string>{};
        List<string> loadSystems = new List<string>{};
        List<FreeSession> loadFrees = new List<FreeSession>{};

        foreach (var item in loadTypeIncl.First().LotTypes)
        {
            loadTypes.Add(item.type_name);
        }
        foreach (var item in loadSystemIncl.First().ParkingSystems)
        {
            loadSystems.Add(item.system_name);
        }
        foreach (var item in loadFreeIncl.First().FreeParkingSessions)
        {
            loadFrees.Add(new FreeSession
            {
                day = item.day,
                session = new TimeRange
                {
                    start = item.start_time,
                    end = item.end_time
                }
            });
        }

        //Parse the version in the CSV
        var csvTypes = TypesToLotTypes(l.types);
        var csvSystems = SystemsToSys(l.systems);
        var csvFree = FreeSessionsToFreePark(l.free, cp);

        //Match the database with the CSV
        foreach (var item in csvTypes)
        {
            if (!loadTypes.Contains(item.type_name)) //CSV has something the database does not. Have to add
            {
                cp.LotTypes.Add(item);
            }
        }
        foreach (var item in csvSystems)
        {
            if (!loadSystems.Contains(item.system_name))
            {
                cp.ParkingSystems.Add(item);
            }
        }

        var total = db.FREE_PARK.Count();
        foreach (var item in csvFree)
        {
            //"Contains" but with different equality conditions (ie that the free parking day, start and end are the same)
            bool match = false;
            foreach (var q in loadFrees)
            {
                match = match || (q.day == item.day && q.session.start == item.start_time && q.session.end == item.end_time);
            }

            if (!match)
            {
                total = total + 1;

                db.FREE_PARK.Add(new FreePark
                {
                    free_park_instance_id = total,
                    day = item.day,
                    start_time = item.start_time,
                    end_time = item.end_time
                }); 
            }
        }
    }

    //Helper function to add a lot type to the database
    private Typ addType(string t, int init)
    {

        Typ newTyp = new Typ
        {
            type_id = init,
            type_name = t
        };

        db.TYPE.Add(newTyp);

        return newTyp;
    }

    //Helper function to add a system to the database
    private Sys addSystem(string t, int init)
    {
        Sys newSys = new Sys
        {
            system_id = init,
            system_name = t
        };

        db.SYSTEM.Add(newSys);

        return newSys;
    }

    //Intermediate function that sets up the rollback in case anything goes wrong
    private void sync(List<Lot> ls)
    {
        using (var trans = db.Database.BeginTransaction())
        {
            int n = 1;
            foreach (Lot l in ls)
            {
                Console.WriteLine($"Processing ({n} / {ls.Count}): " + l.car_park_no);
                syncSingleLotWithCSV(l);
                n++;
            }

            db.SaveChanges();
            trans.Commit();
        }
    }

    //Main entry point
    public void syncWithCSV(string path)
    {
        sync(ParseCSV.ReadFile(path));
    }

    //Helper function to log array during debugging
    public string ColToString<T>(ICollection<T> x)
    {
        if(x == null)
        {
            return "[]";
        }

        if(x.Count <= 0)
        {
            return "[]";
        }

        string output = "[";
        foreach(var t in x)
        {
            output = output + (t == null ? "null" : t.ToString()) + ", ";
        }
        output = output.Remove(output.Length - 1).Remove(output.Length - 2);

        output = output + "]";

        return output;
    }

    //Static function to convert CarParks (records from database) to versions acceptable for API requests
    public static Lot CarParkToLot(CarPark p)
    {
        List<string> types = new List<string>{};
        List<string> systems = new List<string>{};
        List<FreeSession> frees = new List<FreeSession>{};

        foreach (var t in p.LotTypes)
        {
            types.Add(t.type_name);
        }
        foreach (var pk in p.ParkingSystems)
        {
            systems.Add(pk.system_name);
        }
        foreach (var f in p.FreeParkingSessions)
        {
            frees.Add(new FreeSession
            {
                day = f.day,
                session = new TimeRange
                {
                    start = f.start_time,
                    end = f.end_time
                }
            });
        }

        return new Lot
        {
            car_park_no = p.car_park_no,
            address = p.address,
            x_coord = p.x_coord,
            y_coord = p.y_coord,
            types = types.ToArray(),
            systems = systems.ToArray(),
            short_term_start = p.short_term_start,
            short_term_end = p.short_term_end,
            free = frees.ToArray(),
            night_parking = p.night_parking,
            decks = p.decks,
            gantry_height = p.gantry_height,
            basement = p.basement
        };
    }
}