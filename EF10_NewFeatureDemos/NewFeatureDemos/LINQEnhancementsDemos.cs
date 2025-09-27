using Castle.Core.Resource;
using EF10_NewFeatureDemos.ConsoleHelpers;
using EF10_NewFeaturesDbLibrary;
using EF10_NewFeaturesModels;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace EF10_NewFeatureDemos.NewFeatureDemos;

public class LINQEnhancementsDemos : IAsyncDemo
{
    private InventoryDbContext _db;

    public LINQEnhancementsDemos(InventoryDbContext context)
    {
        _db = context;
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

    private List<string> GetMenuOptions()
    {
        return new List<string> {
            "Show N Plus One Query",
            "Fix N Plus One Query",
            "Prefetch IEnumerable",
            "One Fetch Only",
            "Show Contributor Data Report [old way -> Group Join and Select Many",
            "Show Contributor Data Report [new way -> Left Join]",
            "Something else?",
            "Better than something else?",
            "Exit"
        };
    }

    private async Task<bool> HandleMenuChoiceAsync(int choice)
    {

        switch (choice)
        {
            case 1:
                await ShowNPlusOneQuery();
                _db.ChangeTracker.Clear(); // Detach all tracked entities
                break;
            case 2:
                await FixNPlusOneQuery();
                _db.ChangeTracker.Clear(); // Detach all tracked entities
                break;
            case 3:
                await PrefetchIEnumerable();
                _db.ChangeTracker.Clear(); // Detach all tracked entities
                break;
            case 4:
                await OneFetchOnly();
                _db.ChangeTracker.Clear(); // Detach all tracked entities
                break;
            case 5:
                await ShowContributorDataReportOldLogic();
                _db.ChangeTracker.Clear(); // Detach all tracked entities
                break;
            case 6:
                await ShowContributorDataReportNewLogic();
                _db.ChangeTracker.Clear(); // Detach all tracked entities
                break;
            case 7:
                await SomethingElseImSure();
                _db.ChangeTracker.Clear(); // Detach all tracked entities
                break;
            case 8:
                await BetterThanSomethingElse();
                _db.ChangeTracker.Clear(); // Detach all tracked entities
                break;
            case 9:
                return false;
            default:
                Console.WriteLine("Invalid choice. Try again.");
                break;
        }
        return true;
    }

    private async Task ShowNPlusOneQuery()
    {
        string heading = "N Plus One Query";
        Console.WriteLine(heading);

        var items = await _db.Items.ToListAsync();
        foreach (var item in items)
        {
            //requires an additional database call on each iteration
            var categoryName = item.Category?.CategoryName ?? "No Category";
            Console.WriteLine($"{item.ItemName} is in category {categoryName}");
        }

        Console.WriteLine($"{heading} - Completed");
        UserInput.WaitForUserInput();
    }

    private async Task FixNPlusOneQuery()
    {
        string heading = "Fix N Plus One Query";
        Console.WriteLine(heading);

        var items = await _db.Items.Include(x => x.Category)
                                .ToListAsync();
        foreach (var item in items)
        {
            //no extra call since already loaded
            var categoryName = item.Category?.CategoryName ?? "No Category";
            Console.WriteLine($"{item.ItemName} is in category {categoryName}");
        }


        Console.WriteLine($"{heading} - Completed");
        UserInput.WaitForUserInput();
    }

    private async Task PrefetchIEnumerable()
    {
        //note, this is not linq enhancements, this is just mis-coding the query
        //also note that I do this all the time, but code reviewers are important
        string heading = "Pre-Fetching an IEnumerable, then operating on it on the client side";
        Console.WriteLine(heading);
        int howMany = 10;

        //todo: perhaps group this better for iteration
        var items = await _db.Items.Include(x => x.Category)
            .GroupBy(i => new { i.ItemName, i.Category.CategoryName })
            .AsAsyncEnumerable()
            .Select(g => g.First())
            .OrderBy(i => i.Category.CategoryName)
            .Take(howMany)
            .ToListAsync();

        Console.WriteLine(ConsolePrinter.PrintBoxedList(items, x => $"Category {x.Category.CategoryName} has item with " +
                                                                        $"ID: {x.Id} - Name {x.ItemName}"));

        Console.WriteLine($"{heading} - Completed");

        UserInput.WaitForUserInput();
    }

    private async Task OneFetchOnly()
    {
        string heading = "Fetch only once";
        Console.WriteLine(heading);
        int howMany = 10;

        var items = await _db.Items
                    .Select(i => new { i.Id, i.ItemName, i.Category.CategoryName })
                    .Distinct()
                    .OrderBy(i => i.CategoryName)
                    .Take(howMany)
                    .Select(i => new { Id = i.Id, ItemName = i.ItemName, Category = i.CategoryName })
                    .ToListAsync();

        Console.WriteLine(ConsolePrinter.PrintBoxedList(items, x => $"Category {x.Category} has item with " +
                                                                        $"ID: {x.Id} - Name {x.ItemName}"));

        Console.WriteLine($"{heading} - Completed");

        UserInput.WaitForUserInput();
    }

    private async Task ShowContributorDataReportOldLogic()
    {
        string heading = "Contributor Data Report (using Group Join, Select Many, and Group By)";
        Console.WriteLine(heading);

        //ensure a couple exist with no item matches -> count 0
        EnsureContributorsWithNoItemsExist();

        //the old way to get contributors with no items associated
        //this is the code from listing 14-20:
        var cdr = _db.Contributors
            .GroupJoin(
                _db.ItemContributors,
                c => c.Id,
                ic => ic.ContributorId,
                (c, ics) => new { Contributor = c, ItemContributors = ics }
            )
            .SelectMany(
                x => x.ItemContributors.DefaultIfEmpty(),
                (x, ic) => new { x.Contributor, ItemContributor = ic }
            )
            .GroupBy(x => x.Contributor)
            .Select(g => new
            {
                ContributorName = g.Key.ContributorName,
                ItemCount = g.Count(x => x.ItemContributor != null)  // counts items safely
            })
            .ToList();

        Console.WriteLine("Contributors and their item counts (old way):");
        Console.WriteLine(ConsolePrinter.PrintBoxedList(cdr, x => $"{x.ContributorName}: {x.ItemCount} items"));

        Console.WriteLine($"{heading} - Completed");

        UserInput.WaitForUserInput();
    }

    private async Task ShowContributorDataReportNewLogic()
    {
        string heading = "Contributor Data Report (using Left Join, Group By, and Select)";
        Console.WriteLine(heading);

        //Ensure a couple of contributors with no items exist

        var cdr = _db.Contributors
                    .LeftJoin(
                        _db.ItemContributors,
                        c => c.Id,
                        ic => ic.ContributorId,
                        (c, ic) => new { Contributor = c, ItemContributor = ic }
                    )
                    .GroupBy(x => x.Contributor)
                    .Select(g => new
                    {
                        ContributorName = g.Key.ContributorName,
                        ItemCount = g.Count(x => x.ItemContributor != null) // → 0 when none
                    })
                    .ToList();

        Console.WriteLine("Contributors and their item counts (new way):");
        Console.WriteLine(ConsolePrinter.PrintBoxedList(cdr, x => $"{x.ContributorName}: {x.ItemCount} items")); 
        Console.WriteLine($"{heading} - Completed");
        UserInput.WaitForUserInput();
    }

    private async Task SomethingElseImSure()
    {
        string heading = "Something Else";
        Console.WriteLine(heading);

        Console.WriteLine($"{heading} - Completed");

        UserInput.WaitForUserInput();
    }

    private async Task BetterThanSomethingElse()
    {
        string heading = "Better than Something Else";
        Console.WriteLine(heading);

        Console.WriteLine($"{heading} - Completed");

        UserInput.WaitForUserInput();
    }

    private void EnsureContributorsWithNoItemsExist()
    {
        var contributorNames = new[] { "No Items Contributor 1", "No Items Contributor 2" };
        foreach (var name in contributorNames)
        {
            var exists = _db.Contributors.Any(c => c.ContributorName == name);
            if (!exists)
            {
                _db.Contributors.Add(new EF10_NewFeaturesModels.Contributor
                {
                    ContributorName = name,
                    IsActive = true
                });
            }
        }
        _db.SaveChanges();
    }
}
