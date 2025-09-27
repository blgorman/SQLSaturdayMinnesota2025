
using EF10_NewFeatureDemos.ConsoleHelpers;
using EF10_NewFeaturesDbLibrary;
using EF10_NewFeaturesModels;
using Microsoft.EntityFrameworkCore;

namespace EF10_NewFeatureDemos.NewFeatureDemos;

public class InterceptorsDemos : IAsyncDemo
{
    private InventoryDbContext _db;

    public InterceptorsDemos(InventoryDbContext context)
    {
        _db = context;
    }

    private List<string> GetMenuOptions()
    {
        return new List<string> {
            "LoggingInterceptor",
            "SoftDeleteInterceptor",
            "Exit"
        };
    }

    private async Task<bool> HandleMenuChoiceAsync(int choice)
    {

        switch (choice)
        {
            case 1:
                await ShowLoggingInterceptor();
                _db.ChangeTracker.Clear(); // Detach all tracked entities
                break;
            case 2:
                await ShowSoftDeleteInterceptor();
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

    private async Task ShowLoggingInterceptor()
    {
        Console.WriteLine("Logging Interceptor");
        Console.WriteLine("Run the program without the interceptors turned on and check the logs.");
        Console.WriteLine("Then run the program with the interceptors turned on and check the logs again.");
        Console.WriteLine("With the interceptor on, you should see the SQL commands and query compilation logs.");
        Console.WriteLine("Note that by default this would have been logged, but I configured this to not log unless the interceptor is on.");

        var items = _db.Items
                        .Where(i => EF.Functions.Like(i.ItemName, "%lord%"))
                        .OrderBy(i => i.ItemName)
                        .Select(i => new { i.Id, i.ItemName });

        Console.WriteLine("Executing query...");
        Console.WriteLine("Query results:");
        Console.WriteLine(ConsolePrinter.PrintBoxedList(items, j => $"{j.Id}: {j.ItemName}"));

        Console.WriteLine("Logging Interceptor Completed");

        UserInput.WaitForUserInput();
    }

    private async Task ShowSoftDeleteInterceptor()
    {
        Console.WriteLine("Soft Delete Interceptor");
        Console.WriteLine("When the interceptors are on, all delete operations will be converted to modification (update) and set the IsDeleted flag to '1'");
        Console.WriteLine("Run once without the interceptor for full delete");
        Console.WriteLine("Run again with the interceptor to see soft delete in action");


        Console.WriteLine(ConsolePrinter.PrintFormattedMessage("All Categories with filter status", "All Categories"));
        var categories = await _db.Categories.ToListAsync();
        Console.WriteLine(ConsolePrinter.PrintBoxedList(categories
            , c => $"{c.Id}: {c.CategoryName} [IS Deleted: {c.IsDeleted}] - [Is Active {c.IsActive}]"));
        UserInput.WaitForUserInput();

        var ts = DateTime.Now.ToString("yyyyMMddHHmmss");
        var cat = new Category()
        {
            CategoryName = $"Test Category [{ts}]",
            IsActive = true
        };

        //add it
        _db.Categories.Add(cat);
        await _db.SaveChangesAsync();

        //now delete it (should be a soft delete)
        //(put a breakpoint on SaveChangesAsync in the interceptor to see it hit)
        _db.Categories.Remove(cat);
        await _db.SaveChangesAsync();

        //--------------------------------------------------
        Console.WriteLine(ConsolePrinter.PrintFormattedMessage("Current Categories with filters applied", "Categories no filters"));
        var currentCategories = await _db.Categories.ToListAsync();
        Console.WriteLine(ConsolePrinter.PrintBoxedList(currentCategories
            , c => $"{c.Id}: {c.CategoryName} [IS Deleted: {c.IsDeleted}] - [Is Active {c.IsActive}]"));

        Console.WriteLine("Soft Delete Interceptor Completed");

        UserInput.WaitForUserInput();
    }
}
