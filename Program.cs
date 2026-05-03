using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;

using var db = new ParkingContext();

Console.WriteLine($"Database path: {db.DbPath}.");

Console.WriteLine("Querying for a lot");
var blog = await db.SYSTEM
    .FirstAsync();

Console.WriteLine(blog.system_id);

ParseCSV.ReadFile("testSection.csv");