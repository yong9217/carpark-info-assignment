class DatabaseConnector
{
    public ParkingContext db = new ParkingContext();

    public DatabaseConnector()
    {
        Console.WriteLine($"Database path: {db.DbPath}.");
    }

    public static async Task Main(string[] args)
    {
        DatabaseConnector con = new DatabaseConnector();

        // await con.db.Database.EnsureDeletedAsync();
        await con.db.Database.EnsureCreatedAsync();

        // using (var trans = con.db.Database.BeginTransaction())
        // {
        //     con.addType("test1");

        //     foreach (var item in con.db.TYPE)
        //     {
        //         Console.WriteLine(item.type_name);
        //     }

        //     trans.Commit();
        // }

        string path = "testSection.csv";

        List<Lot> fromCSV = ParseCSV.ReadFile(path);

        Console.WriteLine("Updating " + fromCSV.Count + " records from " + path + "...");

        con.syncWithCSV(fromCSV);

        Console.WriteLine("Done!");
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
            basement = false
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

    private void syncSingleLotWithCSV(Lot l)
    {
        var res = db.CARPARK.Where(m => m.car_park_no == l.car_park_no);
        
        if(res.Count() <= 0)
        {
            Console.WriteLine("Adding...");
            var x = LotToCarPark(l);
            db.CARPARK.Add(x);

        } else
        {
            Console.WriteLine("Updating...");
            updateToCSV(l, res.First());
        }

        db.SaveChanges();
    }

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
        

        // cp.LotTypes = TypesToLotTypes(l.types);
        var x = TypesToLotTypes(l.types);

        foreach (var item in cp.LotTypes)
        {
            Console.WriteLine(item);
        }
        Console.WriteLine("---------------------------------");
        foreach (var item in x)
        {
            Console.WriteLine(item);
        }

        cp.ParkingSystems = SystemsToSys(l.systems);
        cp.FreeParkingSessions = FreeSessionsToFreePark(l.free,cp);

        
    }

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

    public void syncWithCSV(List<Lot> ls)
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
}