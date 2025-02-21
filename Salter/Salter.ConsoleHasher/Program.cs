using System.Text;

using Salter.Core;

namespace Salter.ConsoleHasher;

internal class Program
{
    static void Main(string[] args)
    {
        var passwordHasher = new PasswordHasher();

        while (true)
        {
            try
            {
                Console.Write("Enter a password: ");
                var password = ReadPassword();

                if (string.IsNullOrWhiteSpace(password))
                {
                    Console.WriteLine("Password cannot be empty. Please try again.");
                    continue;
                }

                var hash = passwordHasher.GenerateHash(password, out var salt);

                Console.WriteLine($"Generated Hash: {hash}");
                Console.WriteLine($"Generated Salt: {salt}");

                Console.Write("Do you want to hash another password? (y/n): ");
                var response = Console.ReadLine()?.Trim().ToLower();

                if (response != "y")
                {
                    break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
        }
    }

    private static string ReadPassword()
    {
        var password = new StringBuilder();
        while (true)
        {
            var key = Console.ReadKey(intercept: true);
            if (key.Key == ConsoleKey.Enter)
            {
                Console.WriteLine();
                break;
            }
            else if (key.Key == ConsoleKey.Backspace)
            {
                if (password.Length > 0)
                {
                    password.Remove(password.Length - 1, 1);
                    Console.Write("\b \b");
                }
            }
            else
            {
                password.Append(key.KeyChar);
                Console.Write("*");
            }
        }
        return password.ToString();
    }
}