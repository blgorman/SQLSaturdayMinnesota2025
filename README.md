# Real World EF Core 10 - New Features Demonstration

This is the demonstration code for the "Real World EF" presentation at SQL Saturday 2025 at St. Paul College in Minnesota. The application showcases Entity Framework Core 10's new features through interactive console demonstrations.

## Slides

You can find the slides here:

[Slides for the talk](https://talkimages.blob.core.windows.net/sqlsaturdaymn2025/RealWorldEF.pptx)

## Quick Start Setup

### Prerequisites
- .NET 10 SDK or later
- SQL Server (LocalDB, Express, or full SQL Server)
- Visual Studio 2022 or VS Code

### Setup Instructions

1. **Clone and Build**
   ```bash
   git clone <repository-url>
   cd SQLSaturdayMinnesota2025
   dotnet build
   ```

2. **Configure Database Connection**
   - Update the connection string in `EF10_NewFeatureDemos/appsettings.json`
   - Default: `Server=localhost;Database=SQLServerSaturday;User Id=sa;Password=your-password-here;TrustServerCertificate=True;MultipleActiveResultSets=False;`

3. **Run Database Migrations** ⚠️ **IMPORTANT**
   ```bash
   cd EF10_NewFeatureDemos
   dotnet ef database update --project ../EF10_NewFeaturesDbLibrary
   ```
   This step is **required** to create the database schema and seed initial data.

4. **Run the Application**
   ```bash
   cd EF10_NewFeatureDemos
   dotnet run
   ```

### Environment Configuration (Optional)
Set these environment variables to customize behavior:
- `USE_INTERCEPTORS=true/false` - Enable/disable interceptors (default: false)
- `LOG_TO_CONSOLE=true/false` - Show EF logging in console (default: false)
- `DOTNET_ENVIRONMENT=Development/Production` - Environment setting

## Demo Categories and Features

The application presents 6 main categories of EF Core 10 demonstrations through an interactive menu system:

### 1. Show the Data
**Purpose**: Displays the current database content to establish baseline understanding
- **What it does**: Queries and displays Items with their Categories, Contributors, and Genres using optimized EF Core queries
- **Why important**: Provides context for other demonstrations and shows initial data structure
- **Key features**: Uses `AsNoTracking()` for read-only operations, demonstrates `string.Join()` for aggregated data

### 2. Interceptors and Logging
**Purpose**: Demonstrates EF Core's interceptor pattern for cross-cutting concerns

#### Sub-demos:
- **LoggingInterceptor**
  - **What it does**: Shows custom command logging and performance monitoring
  - **Why important**: Essential for debugging and performance analysis in production
  - **Key features**: Custom SQL command logging, execution time tracking

- **SoftDeleteInterceptor** 
  - **What it does**: Implements soft delete pattern where records are marked as deleted rather than physically removed
  - **Why important**: Maintains data integrity and enables data recovery in business applications
  - **Key features**: Automatic soft delete handling, transparent to application code
  - **Gotchas**: Creates test data for Query Filters demo - run this first if exploring query filters

### 3. Query Filters
**Purpose**: Demonstrates EF Core 10's enhanced query filtering capabilities

#### Sub-demos:
- **Original Query Operation**
  - **What it does**: Shows traditional all-or-nothing query filter behavior (pre-EF10)
  - **Why important**: Illustrates limitations of previous EF versions
  - **Setup note**: Creates inactive categories for demonstration purposes

- **Query with Named Filters**
  - **What it does**: Showcases EF10's ability to selectively disable specific named query filters
  - **Why important**: Provides fine-grained control over which filters apply to specific queries
  - **Key features**: `IgnoreQueryFilters(new[] { "SoftDelete", "Active" })` for selective filter control
  - **Gotchas**: Requires running SoftDeleteInterceptor demo first to create test data

### 4. Bulk Update/Delete
**Purpose**: Demonstrates EF Core 10's new bulk operation capabilities for improved performance

#### Sub-demos:
- **Bulk Update Items - All On Sale [original logic]**
  - **What it does**: Updates items using traditional EF approach (load, modify, save)
  - **Why important**: Baseline comparison for performance improvements

- **Bulk Update Items - None On Sale [new Bulk Update]**
  - **What it does**: Updates items using new EF10 bulk update operations
  - **Why important**: Dramatically improves performance for large-scale updates
  - **Key features**: Single database round-trip for multiple updates

- **Bulk Update Movies - All On Sale [new Bulk Update w/Filter]**
  - **What it does**: Demonstrates filtered bulk updates using EF10's new capabilities
  - **Why important**: Shows how to combine bulk operations with filtering conditions

- **Bulk Delete Junk Data - Delete by Filter with one Operation**
  - **What it does**: Deletes records matching specific criteria in a single operation
  - **Why important**: Efficient cleanup of unwanted data without loading into memory

- **Bulk Delete All Junk Data - Delete All with one Operation**
  - **What it does**: Removes all junk data records in one database operation
  - **Why important**: Mass cleanup operations with optimal performance

### 5. Work with JSON Columns
**Purpose**: Showcases EF Core's enhanced JSON column support for modern data scenarios

#### Sub-demos:
- **Original JSON Column Demo**
  - **What it does**: Demonstrates basic JSON column querying and manipulation
  - **Why important**: Shows how to work with structured data within relational databases
  - **Key features**: JSON property queries, updates to JSON fields

- **Get Contributors by City**
  - **What it does**: Queries JSON address data to find contributors in specific cities
  - **Why important**: Demonstrates querying within JSON structures for business logic
  - **Key features**: JSON path queries for nested object properties

- **Get Contributors with Address having 'P.O. Box'**
  - **What it does**: Searches within JSON address fields for specific patterns
  - **Why important**: Shows complex JSON querying for data validation and filtering
  - **Setup note**: Randomly updates contributor addresses before each demo run

### 6. LINQ Enhancements
**Purpose**: Demonstrates EF Core 10's improved LINQ translation and query optimization

#### Sub-demos:
- **Show N Plus One Query**
  - **What it does**: Deliberately creates the N+1 query problem for demonstration
  - **Why important**: Illustrates common performance anti-pattern
  - **Educational value**: Shows how inefficient queries look and perform

- **Fix N Plus One Query**
  - **What it does**: Resolves the N+1 problem using proper eager loading
  - **Why important**: Demonstrates correct approach to related data loading
  - **Key features**: `.Include()` and optimized query patterns

- **Prefetch IEnumerable**
  - **What it does**: Shows EF10's improved prefetching capabilities
  - **Why important**: Better memory management and query performance
  - **Key features**: Enhanced enumeration handling

- **One Fetch Only**
  - **What it does**: Demonstrates single-query strategies for complex data retrieval
  - **Why important**: Minimizes database round trips for better performance

- **Show Contributor Data Report [old way -> Group Join and Select Many]**
  - **What it does**: Creates reports using traditional LINQ operations
  - **Why important**: Baseline comparison for EF10 improvements
  - **Complexity**: Shows older, more verbose approaches

- **Show Contributor Data Report [new way -> Left Join]**
  - **What it does**: Same report using EF10's improved left join translation
  - **Why important**: Demonstrates cleaner, more efficient query generation
  - **Key features**: Simplified LINQ syntax with better SQL output

- **Collated Values in the Underlying Query**
  - **What it does**: Shows how EF10 handles collation in queries
  - **Why important**: Database-specific optimizations for string operations

- **Collated Values using Parameters in the Underlying Query**
  - **What it does**: Demonstrates parameterized collation for query plan reuse
  - **Why important**: Performance optimization through query plan caching

## Important Notes and Gotchas

### For Presenters
- **Run migrations first**: The app will fail without proper database setup
- **Demo order matters**: Some demos create data that others depend on
  - Run Interceptors → SoftDeleteInterceptor before Query Filters demos
  - Bulk Operations demos are independent
  - JSON demos self-initialize data
  - LINQ demos are self-contained

- **Connection string**: Update before presenting - default won't work on most systems
- **Environment variables**: Consider setting `USE_INTERCEPTORS=true` and `LOG_TO_CONSOLE=true` for live demonstrations

### Technical Considerations
- **Database requirements**: Requires SQL Server with JSON support (2016+)
- **Performance**: Some demos intentionally show bad patterns (N+1) for educational purposes
- **Data seeding**: Initial migration includes comprehensive test data
- **Logging**: Check `C:\Logs` directory for detailed application logs

### Troubleshooting
- **"Database not found"**: Run `dotnet ef database update --project ../EF10_NewFeaturesDbLibrary` from EF10_NewFeatureDemos directory
- **Connection issues**: Verify SQL Server is running and connection string is correct
- **Missing data**: Some demos depend on others running first - check demo descriptions above
- **EF10 Preview**: This code targets EF Core 10 preview - may require specific .NET version

## Project Structure
- **EF10_NewFeatureDemos**: Main console application with demo implementations
- **EF10_NewFeaturesDbLibrary**: Entity Framework context, migrations, and database setup
- **EF10_NewFeaturesModels**: Entity models and DTOs
- **ConsoleHelpers**: Utility classes for menu generation and user interaction

This demonstration code provides a comprehensive overview of EF Core 10's new features in a hands-on, interactive format perfect for technical presentations and team learning sessions.