using Microsoft.EntityFrameworkCore;
using System;
using System.ComponentModel.DataAnnotations.Schema;

//Setup the database structure
public class ParkingContext : DbContext
{
    //Tables
    public DbSet<CarPark> CARPARK { get; set; }
    public DbSet<FreePark> FREE_PARK { get; set; }
    public DbSet<Sys> SYSTEM { get; set; }
    public DbSet<Typ> TYPE { get; set; }
    public string DbPath { get; }

    public ParkingContext()
    {
        DbPath = "C:\\Users\\yong9\\Documents\\node\\carpark-info-assignment\\sqlite\\parking.db";
    }

    //Use sqlite to manipulate database
    protected override void OnConfiguring(DbContextOptionsBuilder options){
        options.UseSqlite($"Data Source={DbPath}");
    }

    //Setup relationships on creation
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<FreePark>().HasOne(c => c.car_park_noFP).WithMany(t => t.FreeParkingSessions);
        modelBuilder.Entity<Typ>().HasMany(c => c.conParksT).WithMany(t => t.LotTypes);
        modelBuilder.Entity<Sys>().HasMany(c => c.conParksS).WithMany(t => t.ParkingSystems);
    }
}

public class CarPark
{
    [System.ComponentModel.DataAnnotations.Key]
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

    [ForeignKey("car_park_noFP")]
    public ICollection<FreePark> FreeParkingSessions { get; set; }
    [ForeignKey("system_id")]
    public ICollection<Sys> ParkingSystems { get; set; }
    [ForeignKey("type_id")]
    public ICollection<Typ> LotTypes { get; set; }

    //Custom ToString for easier debugging
    public override string ToString()
    {
        string output = $"[car_park_no: {this.car_park_no}, address: {this.address}, x_coord: {this.x_coord}, y_coord: {this.y_coord}, short_term_start: {this.short_term_start}, short_term_end: {this.short_term_end}, night_parking: {this.night_parking}, decks: {this.decks}, gantry_height: {this.gantry_height}, basement: {this.basement}, ";
        
        output = output + $"types: {ColToString<Typ>(this.LotTypes)}, systems: {ColToString<Sys>(this.ParkingSystems)}, free: {ColToString<FreePark>(this.FreeParkingSessions)}]";

        return output;
    }

    private string ColToString<T>(ICollection<T> x)
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

public class Sys
{
    [System.ComponentModel.DataAnnotations.Key]
    public int system_id { get; set; }
    public string system_name { get; set; }
    public ICollection<CarPark> conParksS { get; set; }

    //Custom ToString for easier debugging
    public override string ToString()
    {
        return "{id: " + system_id + ",name: " + system_name + "}";
    }
}

public class Typ
{
    [System.ComponentModel.DataAnnotations.Key]
    public int type_id { get; set; }
    public string type_name { get; set; }
    public ICollection<CarPark> conParksT { get; set; }

    //Custom ToString for easier debugging
    public override string ToString()
    {
        return "{id: " + type_id + ",name: " + type_name + "}";
    }
}

public class FreePark
{
    [System.ComponentModel.DataAnnotations.Key]
    public int free_park_instance_id { get; set; }
    public CarPark car_park_noFP { get; set; }
    public int day { get; set; }
    public string start_time { get; set; }
    public string end_time { get; set; }

    //Custom ToString for easier debugging
    public override string ToString()
    {
        return "{id: " + free_park_instance_id + ",day: " + day + ",start: " + start_time + ",end: " + end_time + "}";
    }
}