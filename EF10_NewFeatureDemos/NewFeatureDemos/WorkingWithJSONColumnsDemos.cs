using EF10_NewFeatureDemos.ConsoleHelpers;
using EF10_NewFeaturesDbLibrary;
using EF10_NewFeaturesModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.SqlServer;
using System;

namespace EF10_NewFeatureDemos.NewFeatureDemos;

public class WorkingWithJSONColumnsDemos : IAsyncDemo
{
    private InventoryDbContext _db;

    public WorkingWithJSONColumnsDemos(InventoryDbContext context)
    {
        _db = context;
    }

    private List<string> GetMenuOptions()
    {
        return new List<string> {
            "Original JSON Column Demo",
            "Get Contributors by City",
            "Get Contributors with Address having 'P.O. Box'",
            "Exit"
        };
    }

    private async Task<bool> HandleMenuChoiceAsync(int choice)
    {
        RandomUpdateContributors();
        switch (choice)
        {
            case 1:
                await ShowOriginalJSONQueryLogic();
                break;
            case 2:
                await GetContributorsByCity();
                break;
            case 3:
                await GetContributorsWithPOBoxAddress();
                break;
            case 4:
                return false;
            default:
                Console.WriteLine("Invalid choice. Try again.");
                break;
        }
        return true;
    }

    public async Task RunAsync()
    {
        bool shouldContinue = true;
        while (shouldContinue)
        {
            Console.Clear();

            List<string> menuOptions = GetMenuOptions();

            var menuText = MenuGenerator.GenerateMenu("Interceptors Demos", "Please select an operation", menuOptions, 40);

            // Show menu and get user choice
            int choice = UserInput.GetInputFromUser(menuText, shouldConfirm: true, min: 1, max: menuOptions.Count);

            try
            {
                shouldContinue = await HandleMenuChoiceAsync(choice);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
                Console.WriteLine("Press any key to continue...");
                Console.ReadKey();
            }
        }
    }

    private async Task ShowOriginalJSONQueryLogic()
    {
        Console.WriteLine("Prior to JSON type");
        var cityToFind = "Lakeside";
        var contributors = await _db.Contributors
            .FromSqlRaw(
                "SELECT * FROM Contributors WHERE JSON_VALUE([Address], '$.City') = {0}", cityToFind
            )
            .ToListAsync();
        Console.WriteLine(ConsolePrinter.PrintBoxedList(contributors, c => $"{c.ContributorName} - {c.Address?.AddressLine1}, {c.Address?.City}, {c.Address?.State} {c.Address?.PostalCode}"));

        Console.WriteLine("Prior to JSON type completed");
        UserInput.WaitForUserInput();
    }

    private async Task GetContributorsByCity()
    {
        Console.WriteLine("Contributors By City");

        var cityToFind = "Lakeside";

        List<Contributor> contributorsInCity = await _db.Contributors
            .Where(c => c.Address != null
                    && c.Address.City == cityToFind)
            .ToListAsync();

        //-------------------------------------------------
        //print them
        Console.WriteLine($"Contributors in {cityToFind}:");
        Console.WriteLine(ConsolePrinter.PrintBoxedList(contributorsInCity, c => $"{c.ContributorName} - {c.Address?.AddressLine1}, {c.Address?.City}, {c.Address?.State} {c.Address?.PostalCode}"));

        Console.WriteLine("Query JSON type to get Contributor by City");
        UserInput.WaitForUserInput();
    }

    private async Task GetContributorsWithPOBoxAddress()
    {
        Console.WriteLine("Contibutors by POBox Address");

        List<Contributor> contributorsWithPOBox = await _db.Contributors
                                    .Where(c => c.Address != null && c.Address.AddressLine1
                                            != null && c.Address.AddressLine1.StartsWith("PO Box"))
                                    .ToListAsync();

        //-------------------------------------------------
        //print results:
        if (contributorsWithPOBox.Count > 0)
        {
            Console.WriteLine($"Contributors with PO Box addresses:");
            Console.WriteLine(ConsolePrinter.PrintBoxedList(contributorsWithPOBox, c => $"{c.ContributorName} - {c.Address?.AddressLine1}, {c.Address?.City}, {c.Address?.State} {c.Address?.PostalCode}"));
        }

        Console.WriteLine("Get Contributors by POBox Address");
        UserInput.WaitForUserInput();
    }

    private void RandomUpdateContributors()
    {
        var rand = new Random();
        //get all the contributors
        var contributors = _db.Contributors.ToList();
        foreach (var contributor in contributors)
        {
            if (contributor.Address != null) continue;
            // Randomly update the Metadata JSON column
            contributor.Address = CreateRandomAddress();
        }
        _db.SaveChanges();

        Console.WriteLine("Updated contributor metadata with random values.");
    }

    private Address CreateRandomAddress()
    {
        var rand = new Random();
        var streetNumber = rand.Next(100, 9999);
        var streets = new[] { "Main St", "Oak St", "Pine St", "Maple Ave", "Cedar Ln" };
        var cities = new[] { "Blue River", "Lime Springs", "Lakeside", "Hillview", "Greenville" };
        var states = new[] { "CA", "TX", "NY", "FL", "IL" };
        var zipCodes = new[] { "12345", "67890", "54321", "09876", "11223" };
        var POBoxNumbers = new[] { "PO Box 100", "PO Box 200", "PO Box 300", "PO Box 400", "PO Box 500" };
        var usePOBox = rand.Next(0, 5) == 3; // 20% chance to use PO Box
        var addressLine1 = usePOBox ? POBoxNumbers[rand.Next(POBoxNumbers.Length)] : $"{streetNumber} {streets[rand.Next(streets.Length)]}";
        var addressLine2 = usePOBox ? null : (rand.Next(0, 2) == 1 ? $"Apt {rand.Next(1, 1000)}" : "");
        return new Address
        {
            AddressLine1 = addressLine1,
            AddressLine2 = addressLine2,
            City = cities[rand.Next(cities.Length)],
            State = states[rand.Next(states.Length)],
            PostalCode = zipCodes[rand.Next(zipCodes.Length)]
        };
    }
}
