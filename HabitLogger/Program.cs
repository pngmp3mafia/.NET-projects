using Microsoft.Data.Sqlite;

namespace HabitLogger;

internal class Program
{
    private static string _connectionString = "Data Source=habit-tracker.db";
    private static string[] _tableNames = ["drinking_water", "running", "study_coding"];

    private static void Main(string[] args)
    {
        try
        {
            CreateTables(_tableNames);
            PopulateRandomData(5, _tableNames);
            GetUserInput();
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
            throw;
        }
    }

    private static string GetInput()
    {
        var input = Console.ReadLine();
        while (input == null)
        {
            Console.WriteLine("Please try again!");
            input = Console.ReadLine();
        }

        return input;
    }

    private static void CreateTables(string[] tableNames)
    {
        using (var connection = new SqliteConnection(_connectionString))
        {
            connection.Open();

            var clearCmd = connection.CreateCommand();
            foreach (var table in tableNames)
            {
                clearCmd.CommandText = $"DELETE FROM {table};";
                clearCmd.ExecuteNonQuery();
            }

            var tableCmd = connection.CreateCommand();
            foreach (var tableName in tableNames)
            {
                tableCmd.CommandText =
                    $@"CREATE TABLE IF NOT EXISTS {tableName} (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        Date TEXT,
                        Quantity INTEGER
                        )";

                tableCmd.ExecuteNonQuery();
            }

            connection.Close();
        }
    }

    private static void PopulateRandomData(int numberOfEntry, string[] tableNames)
    {
        var random = new Random();

        using (var connection = new SqliteConnection(_connectionString))
        {
            connection.Open();
            foreach (var habitName in tableNames)
                for (var i = 0; i < numberOfEntry; i++)
                {
                    var date = DateTime.Now.AddDays(-random.Next(100)).ToString("yyyy-M-d dddd");
                    var quantity = random.Next(10);

                    var insertCmd = connection.CreateCommand();
                    insertCmd.CommandText = $@"
                            INSERT INTO {habitName} (date, quantity)
                            VALUES ($date, $quantity);
                            ";
                    insertCmd.Parameters.AddWithValue("$date", date);
                    insertCmd.Parameters.AddWithValue("$quantity", quantity);
                    insertCmd.ExecuteNonQuery();
                }

            connection.Close();
        }
    }

    private static void ViewAllRecord(string[] tableNames)
    {
        using (var connection = new SqliteConnection(_connectionString))
        {
            connection.Open();
            var viewCmd = connection.CreateCommand();
            foreach (var table in tableNames)
            {
                viewCmd.CommandText = $@"
                        SELECT * FROM {table};
                    ";

                Console.WriteLine(table);
                using (var reader = viewCmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var date = reader.GetString(1);
                        var quantity = reader.GetInt32(2);
                        Console.WriteLine($"{date}: {quantity}");
                    }
                }

                Console.WriteLine();
            }

            connection.Close();
        }
    }

    private static void InsertRecord()
    {
        Console.WriteLine("Which activity you would like to lodge?");
        var habitName = GetInput();

        Console.WriteLine("Which date is it?");
        Console.WriteLine(DateTime.TryParse(GetInput(), out var dateTime));
        var date = dateTime.ToString("yyyy-M-d dddd");
        Console.WriteLine(date);

        Console.WriteLine("How many times?");
        int.TryParse(GetInput(), out var quantity);

        using (var connection = new SqliteConnection(_connectionString))
        {
            connection.Open();
            var insertCmd = connection.CreateCommand();
            insertCmd.CommandText = $@"
                    INSERT INTO {habitName} (date, quantity)
                    VALUES ($date, $quantity);";
            insertCmd.Parameters.AddWithValue("$date", date);
            insertCmd.Parameters.AddWithValue("$quantity", quantity);
            insertCmd.ExecuteNonQuery();
            connection.Close();
        }
    }

    private static void DeleteRecord()
    {
        Console.WriteLine("Which activity you would like to delete?");
        var habitName = GetInput();

        Console.WriteLine("Which date is it?");
        Console.WriteLine(DateTime.TryParse(GetInput(), out var dateTime));
        var date = dateTime.ToString("yyyy-M-d dddd");
        Console.WriteLine(date);

        using (var connection = new SqliteConnection(_connectionString))
        {
            connection.Open();
            var insertCmd = connection.CreateCommand();
            insertCmd.CommandText = $@"
                    DELETE FROM {habitName}
                    WHERE date = $date;";
            insertCmd.Parameters.AddWithValue("$date", date);
            insertCmd.ExecuteNonQuery();
            connection.Close();
        }
    }

    private static void UpdateRecord()
    {
        Console.WriteLine("Which activity you would like to update?");
        var habitName = GetInput();

        Console.WriteLine("Which date is it?");
        DateTime.TryParse(GetInput(), out var dateTime);
        var date = dateTime.ToString("yyyy-M-d dddd");

        Console.WriteLine("What is the new quantity?");
        int.TryParse(GetInput(), out var quantity);

        using (var connection = new SqliteConnection(_connectionString))
        {
            connection.Open();
            var insertCmd = connection.CreateCommand();
            insertCmd.CommandText = $@"
                UPDATE {habitName}
                SET quantity = $quantity
                WHERE date = $date;";
            insertCmd.Parameters.AddWithValue("$quantity", quantity);
            insertCmd.Parameters.AddWithValue("$date", date);
            insertCmd.ExecuteNonQuery();
            connection.Close();
        }
    }


    private static void GetUserInput()
    {
        Console.Clear();
        var closeApp = false;
        while (closeApp == false)
        {
            Console.WriteLine();
            Console.WriteLine("MAIN MENU");
            Console.WriteLine();
            Console.WriteLine("What would you like to do?");
            Console.WriteLine();
            Console.WriteLine("Type 0 to Close Application.");
            Console.WriteLine("Type 1 to View All Records.");
            Console.WriteLine("Type 2 to Insert Record.");
            Console.WriteLine("Type 3 to Delete Record.");
            Console.WriteLine("Type 4 to Update Record.");
            Console.WriteLine("---------------------------------");

            var valid = false;
            do
            {
                var input = GetInput();
                valid = decimal.TryParse(input, out _);
                if (decimal.TryParse(input, out _))
                    switch (input)
                    {
                        case "0":
                            closeApp = true;
                            break;
                        case "1":
                            Console.WriteLine("case 1");
                            ViewAllRecord(_tableNames);
                            break;
                        case "2":
                            Console.WriteLine("Please insert a record:");
                            InsertRecord();
                            break;
                        case "3":
                            Console.WriteLine("Deleting a record:");
                            DeleteRecord();
                            break;
                        case "4":
                            Console.WriteLine("Updating a record:");
                            UpdateRecord();
                            break;
                        default:
                            break;
                    }
                else if (input == null)
                    Console.WriteLine("Please input your option below");
                else if (!decimal.TryParse(input, out _))
                    Console.WriteLine("Please input a decimal number from 0 to 7.");
            } while (!valid);
        }

        Console.WriteLine("Closing application...");
    }
}