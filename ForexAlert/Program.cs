using System.Text.Json;


namespace ForexAlert;

public static class Program
{
    private static async Task Main()
    {

        bool exit = false;
        do
        {
            Console.WriteLine();
            Console.WriteLine("\nWelcome to Forex Alert");
            Console.WriteLine("Enter 0 to Exit");
            Console.WriteLine("Enter 1 to Get conversion rate between 2 currencies");
            Console.WriteLine("Enter 2 to Set a real-time alert on a pair of currency");

            decimal parsedValue = decimal.Parse(GetInput(""));
            int option = Convert.ToInt32(parsedValue);
            switch (option)
            {
                case 0:
                    exit = true;
                    break;

                case 1:
                    var currencyFrom = GetInput("\nPlease input first currency:");
                    var currencyTo = GetInput("\nPlease input second currency:");

                    float? currentPrice = await GetPriceApiAsync(currencyFrom, currencyTo);
                    currentPrice = (float?)Math.Round((decimal)currentPrice, 2);

                    Console.WriteLine($"\nThe current rate: {currentPrice}");
                    break;

                case 2:
                    currencyFrom = GetInput("\nPlease input first currency:");
                    currencyTo = GetInput("\nPlease input second currency:");

                    float? target = null;
                    if (float.TryParse(GetInput("\nSet a target rate:"), out float tmpValue))
                    {
                        target = tmpValue;
                    }

                    Console.WriteLine($"\nThe target price: {target}");

                    currentPrice = await GetPriceApiAsync(currencyFrom, currencyTo);

                    if (currentPrice < target)
                    {
                        do
                        {
                            currentPrice = await GetPriceApiAsync(currencyFrom, currencyTo);
                            Console.WriteLine($"\nThe current rate: {currentPrice}");
                            if (currentPrice >= target)
                            {
                                Console.WriteLine($"\nConversion rate is now crossing {target}");
                                break;
                            }
                            Thread.Sleep(1000);
                        }
                        while (currentPrice < target);
                    }
                    else
                    {
                        do
                        {
                            currentPrice = await GetPriceApiAsync(currencyFrom, currencyTo);
                            Console.WriteLine($"\nThe current rate: {currentPrice}");
                            if (currentPrice <= target)
                            {
                                Console.WriteLine($"\nConversion rate is now crossing {target}");
                                break;
                            }
                            Thread.Sleep(1000);
                        }
                        while (currentPrice > target);
                    }


                    Console.WriteLine(Math.Round((decimal)currentPrice, 2));
                    break;
            }
        }
        while (!exit);
    }

    private static string GetInput(string inputString)
    {
        Console.WriteLine(inputString);

        var input = Console.ReadLine();
        while (input == null)
        {
            Console.WriteLine("Please try again!");
            input = Console.ReadLine();
        }

        return input;
    }

    private static async Task<float?> GetPriceApiAsync(string currencyFrom, string currencyTo)
    {
        var date = "latest";
        var apiVersion = "v1";
        var endpoint = $"currencies/{currencyFrom}";
        var url = $"https://cdn.jsdelivr.net/npm/@fawazahmed0/currency-api@{date}/{apiVersion}/{endpoint}.json";

        using var client = new HttpClient();

        try
        {
            var response = await client.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();

            JsonDocument jsonDocument = JsonDocument.Parse(content);
            JsonElement root = jsonDocument.RootElement;

            root.TryGetProperty(currencyFrom, out JsonElement currencyFromElement);
            currencyFromElement.TryGetProperty(currencyTo, out JsonElement currencyToElement);

            float.TryParse(currencyToElement.ToString(), out float result);

            return result;
        }
        catch (HttpRequestException e)
        {
            Console.WriteLine("\nException Caught!");
            Console.WriteLine("Message :{0} ", e.Message);
            return null;
        }
    }
}
