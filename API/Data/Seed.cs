using System.Globalization;
using System.Text.Json;
using API.Entities;
using CsvHelper;
using Microsoft.EntityFrameworkCore;

namespace API.Data;

public class Seed
{
    public static async Task SeedAirlines(DataContext context)
    {
        if (await context.Airlines.AnyAsync()) return;

        using var reader = new StreamReader("Data/Airlines.csv");
        using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
        var airlines = csv.GetRecords<Airline>();
        foreach (var airline in airlines)
        {
            context.Airlines.Add(airline);
        }
        await context.SaveChangesAsync();
    }

    public static async Task SeedAirports(DataContext context)
    {
        if (await context.Airports.AnyAsync()) return;

        using var reader = new StreamReader("Data/Airports.csv");
        using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
        var aiports = csv.GetRecords<Airport>();
        foreach (var airport in aiports)
        {
            context.Airports.Add(airport);
        }
        await context.SaveChangesAsync();
    }

    public static async Task SeedTailNumbers(DataContext context)
    {
        if (await context.TailNumbers.AnyAsync()) return;

        using var reader = new StreamReader("Data/TailNumbers.csv");
        using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
        var tailNumbers = csv.GetRecords<TailNumber>();
        foreach (var tailNumber in tailNumbers)
        {
            context.TailNumbers.Add(tailNumber);
        }
        await context.SaveChangesAsync();
    }

    public static async Task SeedTraining(DataContext context)
    {
        if (await context.Training.AnyAsync()) return;

        using var reader = new StreamReader("Data/Training.csv");
        using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);

        // Register the ClassMap
        csv.Context.RegisterClassMap<TrainingMap>();

        var trainings = csv.GetRecords<Training>();
        foreach (var training in trainings)
        {
            // Get the related entities
            var airline = await context.Airlines.FindAsync(training.AirlineName);
            var orgAirport = await context.Airports.FindAsync(training.OrgAirport);
            var destAirport = await context.Airports.FindAsync(training.DestAirport);
            var tailNumber = await context.TailNumbers.FindAsync(training.TailNum);

            // Set the navigation properties
            training.AirlineNameNavigation = airline;
            training.OrgAirportNavigation = orgAirport;
            training.DestAirportNavigation = destAirport;
            training.TailNumNavigation = tailNumber;

            context.Training.Add(training);
        }
        await context.SaveChangesAsync();
    }

    public static async Task SeedUsers(DataContext context)
    {
        if (await context.Users.AnyAsync()) return;

        using var reader = new StreamReader("Data/Users.csv");
        using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
        var users = csv.GetRecords<User>();
        foreach (var user in users)
        {
            context.Users.Add(user);
        }
        await context.SaveChangesAsync();
    }

    public static async Task SeedUserPredictions(DataContext context)
    {
        if (await context.UserPredictions.AnyAsync()) return;

        using var reader = new StreamReader("Data/UserPredictions.csv");
        using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);

        // Register the ClassMap
        csv.Context.RegisterClassMap<UserPredictionMap>();

        var userPredictions = csv.GetRecords<UserPrediction>();
        foreach (var prediction in userPredictions)
        {
            // Get the related entities
            var airline = await context.Airlines.FindAsync(prediction.AirlineName);
            var orgAirport = await context.Airports.FindAsync(prediction.OrgAirport);
            var destAirport = await context.Airports.FindAsync(prediction.DestAirport);
            var tailNumber = await context.TailNumbers.FindAsync(prediction.TailNum);
            var username = await context.Users.FindAsync(prediction.Username);

            // Set the navigation properties
            prediction.AirlineNameNavigation = airline;
            prediction.OrgAirportNavigation = orgAirport;
            prediction.DestAirportNavigation = destAirport;
            prediction.TailNumNavigation = tailNumber;
            prediction.UsernameNavigation = username;

            context.UserPredictions.Add(prediction);
        }
        await context.SaveChangesAsync();
    }
}
