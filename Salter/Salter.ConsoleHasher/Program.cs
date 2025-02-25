using Salter.Core;

namespace Salter.ConsoleHasher;

internal class Program
{    
    static void Main(string[] args)
    {
        var passwordHasher = new PasswordHasher();
        var continueHashing = true;

        while (continueHashing)
        {
            Console.Write("Enter a password: ");
            var password = ReadPassword();

            if (password.Length == 0)
            {
                Console.WriteLine("Password cannot be empty. Please try again.");
                continue;
            }

            var hash = string.Empty;
            var salt = string.Empty;

            try
            {
                hash = passwordHasher.GenerateHash(password, out salt);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
                continue;
            }
            finally
            {
                // Make sure password is cleared even if an exception occurs
                Array.Clear(password, 0, password.Length);
            }

            Console.WriteLine($"Generated Hash: {hash}");
            Console.WriteLine($"Generated Salt: {salt}");

            continueHashing = AskToContinue();
        }
    }
    private static bool AskToContinue()
    {
        Console.Write("Do you want to hash another password? (y/n): ");
        var response = Console.ReadLine()?.Trim().ToLowerInvariant() ?? string.Empty;
        return response == "y";
    }

    private static char[] ReadPassword()
    {
        var password = new List<char>();

        while (true)
        {
            var key = Console.ReadKey(intercept: true);

            if (key.Key == ConsoleKey.Enter)
            {
                Console.WriteLine();
                break;
            }

            if (key.Key == ConsoleKey.Backspace && password.Count > 0)
            {
                password.RemoveAt(password.Count - 1);
                Console.Write("\b \b");
                continue;
            }

            password.Add(key.KeyChar);
            Console.Write("*");
        }

        var passwordArray = password.ToArray();
        SecureClear(password);

        return passwordArray;
    }

    private static void SecureClear(List<char> list)
    {
        // First overwrite each item in the list with null characters
        // because clearing only removes the references to the items from memory
        // but not the items themselves

        for (int i = 0; i < list.Count; i++)
        {
            list[i] = '\0';
        }

        list.Clear();
    }
}
