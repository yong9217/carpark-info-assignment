using System;
using System.Text.RegularExpressions;
using System.Globalization;

class RawLot
{
    public string? car_park_no { get; set; }
    public string? address { get; set; }
    public float? x_coord { get; set; }
    public float? y_coord { get; set; }
    public string? car_park_type { get; set; }
    public string? type_of_parking_system { get; set; }
    public string? short_term_parking { get; set; }
    public string? free_parking { get; set; }
    public string? night_parking { get; set; }
    public int? car_park_decks { get; set; }
    public float? gantry_height { get; set; }
    public string? car_park_basement { get; set; }
}

class Lot
{
    public string car_park_no { get; set; }
    public string address { get; set; }
    public float x_coord { get; set; }
    public float y_coord { get; set; }
    public string short_term_start { get; set; }
    public string short_term_end { get; set; }
    public bool night_parking { get; set; }
    public int decks { get; set; }
    public float gantry_height { get; set; }
    public bool basement { get; set; }
    public string[] types { get; set; }
    public string[] systems { get; set; }
    public FreeSession[] free { get; set; }

    public override string ToString()
    {
        string output = $"[car_park_no: {this.car_park_no}, address: {this.address}, x_coord: {this.x_coord}, y_coord: {this.y_coord}, short_term_start: {this.short_term_start}, short_term_end: {this.short_term_end}, night_parking: {this.night_parking}, decks: {this.decks}, gantry_height: {this.gantry_height}, basement: {this.basement}, ";
        
        output = output + $"types: {ArrToString<string>(this.types)}, systems: {ArrToString<string>(this.systems)}, free: {ArrToString<FreeSession>(this.free)}]";

        return output;
    }

    private string ArrToString<T>(T[] x)
    {
        if(x == null)
        {
            return "[]";
        }

        if(x.Length <= 0)
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

class FreeSession
{
    public int day { get; set; }
    public TimeRange session { get; set; }

    public override string ToString()
    {
        return $"[day: {this.day}, start: {this.session.start}, end: {this.session.end}]";
    }
}

class TimeRange
{
    public string start { get; set; }
    public string end { get; set; }

    public override string ToString()
    {
        return $"[start: {this.start}, end: {this.end}]";
    }
}

class ParseCSV
{
    public static List<Lot> ReadFile(string path)
    {
        path = "../../../" + path;

        string[] parkTypes = {"basement", "multi-storey", "surface", "covered", "mechanised"};
        string[] parkSystems = {"electronic", "coupon"};
        string[] defDays = {"sun", "mon", "tues", "wed", "thurs", "fr", "sat", "ph"};

        var lines = File.ReadAllLines(path).Skip(1);
        List<Lot> allLots = new List<Lot>{};

        foreach (var line in lines)
        {
            var cols = line.Split(',');

            try
            {
                for (int i = 0; i < cols.Length; i++)
                {
                    cols[i] = PreProcessRaw(cols[i]);
                }

                TimeRange shortTermRange = ParseShortTerm(cols[6]);

                var lot = new Lot
                {
                    car_park_no = cols[0],
                    address = cols[1],
                    x_coord = float.Parse(cols[2]),
                    y_coord = float.Parse(cols[3]),
                    types = ParseAsEnum(cols[4], parkTypes),
                    systems = ParseAsEnum(cols[5], parkSystems),
                    short_term_start = shortTermRange.start,
                    short_term_end = shortTermRange.end,
                    free = ParseFreeParking(cols[7],defDays),
                    night_parking = MatchWithBool(cols[8], "yes", "no"),
                    decks = int.Parse(cols[9]),
                    gantry_height = float.Parse(cols[10]),
                    basement = MatchWithBool(cols[11], "y", "n")
                };

                allLots.Add(lot);
            } catch (Exception e)
            {
                Console.WriteLine("Error on line: " + line);
                Console.WriteLine("Threw: " + e.Message);
                return new List<Lot>{};
            }
        }

        return allLots;
    }

    private static string PreProcessRaw(string t)
    {
        if(t == null)
        {
            throw new Exception("Could not pre-process null string");
        }
        return t.Trim().ToLower();
    }

    private static bool NotEmptyString(string c)
    {
        if(c == null)
        {
            return false;
        }

        if(c.Trim().Length <= 0)
        {
            return false;
        }

        return true;
    }

    private static string[] ParseAsEnum(string t, string[] enums)
    {
        List<string> output = new List<string>{};

        if (NotEmptyString(t))
        {
            foreach (string type in enums)
            {
                if (t.Contains(type)) output.Add(type);
            }
        } else
        {
            throw new Exception("Unable to parse (" + t + ") as a defined element in " + enums);
        }

        return output.ToArray();
    }

    private static FreeSession[] ParseFreeParking(string t, string[] defDays)
    {
        List<FreeSession> output = new List<FreeSession>{};

        if (NotEmptyString(t))
        {
            if (t.Equals("no"))
            {
                return [];
            }

            int[] days = MapToInts(t,defDays);

            TimeRange tr = ParseTimeRange(t);

            foreach (int d in days)
            {
                output.Add(new FreeSession
                {
                    day = d,
                    session = tr
                });
            }

            return output.ToArray();
        } 

        throw new Exception("Unable to parse (" + t + ") as a free parking session");
    }

    private static Match MatchTimeRange(string t)
    {
        Regex regex = new Regex("\\d{1,2}(\\.\\d{1,2})?(am|pm)-\\d{1,2}(\\.\\d{1,2})?(am|pm)");
        return regex.Match(t);
    }

    private static string ExtractTimeRange(string t)
    {
        Match match = MatchTimeRange(t);

        if (match.Success)
        {
            return match.Value;
        }

        throw new Exception("Unable to parse (" + t + ") as a time range");
    }

    private static TimeRange ParseTimeRange(string t)
    {
        t = ExtractTimeRange(t);
        string[] ranges = t.Split('-');

        return new TimeRange
        {
            start = ParseTime(ranges[0]),
            end = ParseTime(ranges[1])
        };
    }

    private static string ParseTime(string t)
    {
        string outputFormat = "HH:mm:ss";

        if (NotEmptyString(t))
        {
            if(DateTime.TryParseExact(t, "htt", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime res1))
            {
                return res1.ToString(outputFormat);
            }
            
            if (DateTime.TryParseExact(t, "h.mmtt", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime res2))
            {
                return res2.ToString(outputFormat);
            }
        }

        throw new Exception("Unable to parse (" + t + ") as a time");
    }

    private static int[] MapToInts(string t, string[] values)
    {
        List<int> output = new List<int>{};

        for (int i = 0; i < values.Length; i++)
        {
            if(t.Contains(values[i])) output.Add(i);
        }

        return output.ToArray();
    }

    private static TimeRange ParseShortTerm(string t)
    {
        var x = MatchTimeRange(t).Success;

        if (x)
        {
            return ParseTimeRange(t);
        }

        if(t.Equals("whole day"))
        {
            return new TimeRange
            {
                start = "00:00:00",
                end = "24:00:00"
            };
        }

        if (t.Equals("no"))
        {
            return new TimeRange
            {
                start = "00:00:00",
                end = "00:00:00"
            };
        }

        throw new Exception("Unable to parse (" + t + ") as short term parking");
    }

    private static bool MatchWithBool(string t, string trueV, string falseV)
    {
        if (t.Equals(trueV))
        {
            return true;
        }
        if (t.Equals(falseV))
        {
            return false;
        }

        throw new Exception(t + "does not match " + trueV + " or " + falseV);
    }
}