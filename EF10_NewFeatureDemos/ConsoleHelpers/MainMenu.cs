using EF10_NewFeatureDemos.NewFeatureDemos;
using EF10_NewFeaturesDbLibrary;

namespace EF10_NewFeatureDemos.ConsoleHelpers;

public class MainMenu 
{
    private readonly InventoryDbContext _db;
    private readonly ShowDataDemo _showDataDemo;
    private readonly InterceptorsDemos _interceptorsDemo;
    private readonly QueryFiltersDemos _queryFiltersDemo;
    private readonly BulkUpdateDeleteDemos _bulkUpdateDeleteDemo;
    private readonly LINQEnhancementsDemos _linqEnhancementsDemo;
    private readonly WorkingWithJSONColumnsDemos _workingWithJSONColumnsDemo;

    public MainMenu(InventoryDbContext context)
    {
        _db = context;
        _showDataDemo = new ShowDataDemo(_db);
        _interceptorsDemo = new InterceptorsDemos(_db);
        _queryFiltersDemo = new QueryFiltersDemos(_db);
        _bulkUpdateDeleteDemo = new BulkUpdateDeleteDemos(_db);
        _workingWithJSONColumnsDemo= new WorkingWithJSONColumnsDemos(_db);
        _linqEnhancementsDemo = new LINQEnhancementsDemos(_db);
    }

    public async Task ShowAsync()
    {
        bool shouldContinue = true;
        while (shouldContinue)
        {
            Console.Clear();

            List<string> menuOptions = GetMenuOptions();

            var menuText = MenuGenerator.GenerateMenu("Main Menu", "Please select an operation", menuOptions, 40);

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

    private async Task<bool> HandleMenuChoiceAsync(int choice)
    {
        IAsyncDemo demo = _showDataDemo;
        switch (choice)
        {
            case 1:
                demo = _showDataDemo;
                break;
            case 2:
                demo = _interceptorsDemo;
                break;
            case 3:
                demo = _queryFiltersDemo;
                break;
            case 4:
                demo = _bulkUpdateDeleteDemo;
                break;
            case 5:
                demo = _workingWithJSONColumnsDemo;
                break;
            case 6:
                demo = _linqEnhancementsDemo;
                break;
            case 7:
                return false;
            default:
                return false;
        }

        await demo.RunAsync();
        Console.WriteLine("Press any key to continue...");
        Console.ReadKey();
        return true;
    }

    public List<string> GetMenuOptions()
    {
        return new List<string> {
            "Show the Data",
            "Interceptors and Logging",
            "Query Filters",
            "Bulk Update/Delete",
            "Work with JSON Columns",
            "Linq Enhancements",
            "Exit"
        };
    }

}
