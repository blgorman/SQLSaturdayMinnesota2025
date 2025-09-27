
using EF10_NewFeatureDemos.ConsoleHelpers;
using EF10_NewFeaturesDbLibrary;
using EF10_NewFeaturesModels;
using Microsoft.EntityFrameworkCore;

namespace EF10_NewFeatureDemos.NewFeatureDemos;

public class QueryFiltersDemos : IAsyncDemo
{
    private InventoryDbContext _db;

    public QueryFiltersDemos(InventoryDbContext context)
    {
        _db = context;
    }

    private List<string> GetMenuOptions()
    {
        return new List<string> {
            "Original Query Operation",
            "Query with Named Filters",
            "Exit"
        };
    }

    private async Task<bool> HandleMenuChoiceAsync(int choice)
    {

        switch (choice)
        {
            case 1:
                await ShowNonNamedQueryFilters();
                _db.ChangeTracker.Clear(); // Detach all tracked entities
                break;
            case 2:
                await ShowNamedQueryFilters();
                _db.ChangeTracker.Clear(); // Detach all tracked entities
                break;
            case 3:
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

            var menuText = MenuGenerator.GenerateMenu("Query Filters Demos", "Please select an operation", menuOptions, 40);

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

    private async Task ShowNonNamedQueryFilters()
    {
        Console.WriteLine("Contributors With Query Filters Demo Started");
        Console.WriteLine("Before EF10 it was all or nothing with query filters.");
        Console.WriteLine("This method will ensure that you have at least one inactive Category. You should already have an IsDeleted category or a few from demo 2 [soft delete]");
        Console.WriteLine("Contributors with all query filters disabled");

        //ensure there is at least one category that is NOT ACTIVE and one that is both deleted and inactive
        var stamps = await _db.Categories.IgnoreQueryFilters().FirstOrDefaultAsync(c => c.CategoryName == "Stamps");
        if (stamps == null)
        {
            var cat = new Category()
            {
                CategoryName = "Stamps",
                IsActive = false,
                IsDeleted = false
            };
            _db.Categories.Add(cat);
            await _db.SaveChangesAsync();
        }
        else if (stamps.IsActive)
        { 
            stamps.IsActive = false;
            await _db.SaveChangesAsync();
        }

        var coins = await _db.Categories.IgnoreQueryFilters().FirstOrDefaultAsync(c => c.CategoryName == "Coins");
        if (coins == null)
        {
            var cat = new Category()
            {
                CategoryName = "Coins",
                IsActive = false,
                IsDeleted = true
            };
            _db.Categories.Add(cat);
            await _db.SaveChangesAsync();
        }
        else if (!coins.IsDeleted || coins.IsActive)
        {
            coins.IsDeleted = true;
            coins.IsActive = false;
            await _db.SaveChangesAsync();
        }

        Console.WriteLine(ConsolePrinter.PrintFormattedMessage("No Filters Ignored", "Filters Applied"));
        var filteredCategories = await _db.Categories.ToListAsync();
        Console.WriteLine(ConsolePrinter.PrintBoxedList(filteredCategories, c => $"{c.Id}: {c.CategoryName} [IS Deleted: {c.IsDeleted}] - [Is Active {c.IsActive}]"));
        UserInput.WaitForUserInput();

        Console.WriteLine(ConsolePrinter.PrintFormattedMessage("Ignore Query Filters", "No Filters Applied"));
        var categories = await _db.Categories
                               .IgnoreQueryFilters()
                               .ToListAsync();

        Console.WriteLine(ConsolePrinter.PrintBoxedList(categories, c => $"{c.Id}: {c.CategoryName} [IS Deleted: {c.IsDeleted}] - [Is Active {c.IsActive}]"));


        Console.WriteLine("Contributors With Query Filters Completed");

        UserInput.WaitForUserInput();
    }

    private async Task ShowNamedQueryFilters()
    {
        Console.WriteLine("Categories with Named Query Filters Demo Started");
        Console.WriteLine("When the filters for IsDeleted are disabled, you don't see any records that have been soft-deleted.");
        Console.WriteLine("Please make sure that you have run the Soft Delete Interceptor demo first to create some soft-deleted records.");

        Console.WriteLine(ConsolePrinter.PrintFormattedMessage("Categories with no filter disabled [not soft deleted && active]",
            "All Filters Applied [no visible inactive or soft-deleted categories]"));
        var categories = await _db.Categories.ToListAsync();
        Console.WriteLine(ConsolePrinter.PrintBoxedList(categories, c => $"{c.Id}: {c.CategoryName} [IS Deleted: {c.IsDeleted}] - [Is Active {c.IsActive}]"));
        UserInput.WaitForUserInput();


        Console.WriteLine(ConsolePrinter.PrintFormattedMessage("Categories with both filters disabled [any state for soft delete or active]"
                                                                , "Active and SoftDelete Ignored [All Items Shown]"));
        // get all records, ignoring both filters
        List<Category> allIncludingDeletedAndActive = await _db.Categories
                                                        .IgnoreQueryFilters(new[] { "SoftDelete", "Active" })
                                                        .ToListAsync();
        

        Console.WriteLine(ConsolePrinter.PrintBoxedList(allIncludingDeletedAndActive
            , c => $"{c.Id}: {c.CategoryName} [IS Deleted: {c.IsDeleted}] - [Is Active {c.IsActive}]"));
        UserInput.WaitForUserInput();

        //show soft deleted (will still ignore inactive)

        Console.WriteLine(ConsolePrinter.PrintFormattedMessage("Categories with soft delete filter disabled [any soft-delete state, but only active items]"
                                                                , "Soft-delete filter off, active on [no inactive items shown]"));
        List<Category> categoriesAndDeleted = await _db.Categories
                                                        .IgnoreQueryFilters(new[] { "SoftDelete" })
                                                        .ToListAsync();
        Console.WriteLine(ConsolePrinter.PrintBoxedList(categoriesAndDeleted
            , c => $"{c.Id}: {c.CategoryName} [IS Deleted: {c.IsDeleted}] - [Is Active {c.IsActive}]"));
        UserInput.WaitForUserInput();

        //show inactive (will still ignore soft deleted)
        Console.WriteLine(ConsolePrinter.PrintFormattedMessage("Categories with active filter disabled [any active state, but only non-soft-deleted]"
                                                , "Active filter off, soft-delete on [no soft-deleted items shown, any active state]"));
        List<Category> categoriesAndActive = await _db.Categories
                                                        .IgnoreQueryFilters(new[] { "Active" })
                                                        .ToListAsync();

        Console.WriteLine(ConsolePrinter.PrintBoxedList(categoriesAndActive
            , c => $"{c.Id}: {c.CategoryName} [IS Deleted: {c.IsDeleted}] - [Is Active {c.IsActive}]"));



        Console.WriteLine("Categories with Named Query Filters Completed");

        UserInput.WaitForUserInput();
    }
}
