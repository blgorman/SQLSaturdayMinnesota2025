using EF10_NewFeatureDemos.ConsoleHelpers;
using EF10_NewFeaturesDbLibrary;
using EF10_NewFeaturesModels;
using Microsoft.EntityFrameworkCore;

namespace EF10_NewFeatureDemos.NewFeatureDemos;

public class BulkUpdateDeleteDemos : IAsyncDemo
{
    private InventoryDbContext _db;

    public BulkUpdateDeleteDemos(InventoryDbContext context)
    {
        _db = context;
    }

    private List<string> GetMenuOptions()
    {
        return new List<string> {
            "Bulk Update Items - All On Sale [original logic]",
            "Bulk Update Items - None On Sale [new Bulk Update] ",
            "Bulk Update Movies - All On Sale [new Bulk Update w/Filter]",
            "Bulk Delete Junk Data - Delete by Filter with one Operation",
            "Bulk Delete All Junk Data - Delete All with one Operation",
            "Exit"
        };
    }

    private async Task<bool> HandleMenuChoiceAsync(int choice)
    {

        switch (choice)
        {
            case 1:
                await BulkUpdateItemsAllOnSaleOriginal();
                _db.ChangeTracker.Clear(); // Detach all tracked entities
                break;
            case 2:
                await BulkUpdateItemsNoneOnSaleNew();
                _db.ChangeTracker.Clear(); // Detach all tracked entities
                break;
            case 3:
                await BulkUpdateMoviesAllOnSaleNew();
                _db.ChangeTracker.Clear(); // Detach all tracked entities
                break;
            case 4:
                await BulkDeleteJunkDataByFilter();
                _db.ChangeTracker.Clear(); // Detach all tracked entities

                break;
            case 5:
                await BulkDeleteJunkDataAll();
                _db.ChangeTracker.Clear(); // Detach all tracked entities

                break;
            case 6:
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

    private async Task BulkUpdateItemsAllOnSaleOriginal()
    {
        Console.WriteLine("Bulk Update All Items On Sale (Original Logic)");
        Console.WriteLine("This is the original logic to update all items to be on sale.");
        Console.WriteLine("We had to fetch all the items, update them in memory, then save the changes.");
        var items = await _db.Items.ToListAsync();
        foreach (var item in items)
        {
            item.IsOnSale = true;
        }
        await _db.SaveChangesAsync();

        Console.WriteLine("It works, but require an O(n) loop and time to do all the items, as well as a transaction to do it and if any fail, they all fail");
        
        var onSaleItems = await _db.Items.Where(i => i.IsOnSale).ToListAsync();
        Console.WriteLine($"Number of items on sale: {onSaleItems.Count}");
        Console.WriteLine(ConsolePrinter.PrintBoxedList(onSaleItems, i => $"{i.Id}: {i.ItemName} - On Sale: {i.IsOnSale}"));

        Console.WriteLine("Bulk Update All Items On Sale (Original Logic) Completed");

        UserInput.WaitForUserInput();
    }

    private async Task BulkUpdateItemsNoneOnSaleNew()
    {
        Console.WriteLine("Bulk Update Items - Nothing is on sale");

        Console.WriteLine("New Bulk update allows for a single operation to update all items");
        int countItems = await _db.Items
                    .ExecuteUpdateAsync(s =>
                        s.SetProperty(i => i.IsOnSale, i => false)
                    );

        Console.WriteLine($"Number of items updated: {countItems}");
        var onSaleItems = await _db.Items.Where(i => i.IsOnSale).ToListAsync();
        Console.WriteLine($"Number of items on sale: {onSaleItems.Count}");
        var items = await _db.Items.ToListAsync();
        Console.WriteLine(ConsolePrinter.PrintBoxedList(items, i => $"{i.Id}: {i.ItemName} - On Sale: {i.IsOnSale}"));

        Console.WriteLine("Bulk Update Items - Nothing is on sale - Completed");
        UserInput.WaitForUserInput();
    }

    private async Task BulkUpdateMoviesAllOnSaleNew()
    {
        Console.WriteLine("Bulk Update With Filter - Movies On Sale");
        Console.WriteLine("New Bulk update allows for a single operation to update all movies to be on sale using a filter");

        int countMovies = await _db.Items
                    .Where(m => m.Category.CategoryName == "Movie")
                    .ExecuteUpdateAsync(s =>
                        s.SetProperty(i => i.IsOnSale, i => true)
                    );

        Console.WriteLine($"Number of items updated: {countMovies}");
        var onSaleItems = await _db.Items.Where(i => i.IsOnSale).ToListAsync();
        Console.WriteLine($"Number of items on sale: {onSaleItems.Count}");
        Console.WriteLine(ConsolePrinter.PrintBoxedList(onSaleItems, i => $"{i.Id}: {i.ItemName} - On Sale: {i.IsOnSale}"));

        Console.WriteLine("Bulk Update With Filter - Movies On Sale - Completed");

        UserInput.WaitForUserInput();
    }

    private async Task BulkDeleteJunkDataByFilter()
    {
        Console.WriteLine("Bulk Delete Junk Data with Filter");

        //Ensure there is junk to delete
        var existingJunk = await _db.JunkToBulkDeletes.AnyAsync();
        if (!existingJunk)
        {
            //create junk to delete
            for (int i = 1; i <= 10; i++)
            {
                var name = i % 2 == 0 ? $"GoodData-{i}" : $"BadData-{i}";  
                _db.JunkToBulkDeletes.Add(new JunkToBulkDelete { Name = name, IsActive = true, IsDeleted = false });
            }
            await _db.SaveChangesAsync();
        }

        var allJunk = await _db.JunkToBulkDeletes.ToListAsync();
        Console.WriteLine($"Number of junk items before delete: {allJunk.Count}");
        Console.WriteLine(ConsolePrinter.PrintBoxedList(allJunk, j => $"{j.Id}: {j.Name}"));

        int countJTD = await _db.JunkToBulkDeletes
                                .Where(j => j.Name.Contains("BadData"))
                                .ExecuteDeleteAsync();

        var current = await _db.JunkToBulkDeletes.ToListAsync();
        Console.WriteLine($"Number of junk items deleted: {countJTD}");
        Console.WriteLine($"Number of junk items remaining: {current.Count}");
        Console.WriteLine(ConsolePrinter.PrintBoxedList(current, j => $"{j.Id}: {j.Name}"));

        Console.WriteLine("Bulk Delete Junk Data With Filter - Completed");

        UserInput.WaitForUserInput();
    }

    private async Task BulkDeleteJunkDataAll()
    {
        Console.WriteLine("Bulk Delete All Junk Data (no filter)");

        int countJTD = await _db.JunkToBulkDeletes
                                .ExecuteDeleteAsync();

        var current = await _db.JunkToBulkDeletes.ToListAsync();
        Console.WriteLine($"Number of junk items deleted: {countJTD}");
        Console.WriteLine($"Number of junk items remaining: {current.Count}");

        Console.WriteLine("Bulk Delete All Junk Data (no filter) - Completed");

        UserInput.WaitForUserInput();
    }
}
