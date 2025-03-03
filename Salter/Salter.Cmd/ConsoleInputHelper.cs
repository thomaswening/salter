using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Salter.Cmd;

internal static class ConsoleInputHelper
{
    private const string DefaultRejectionMessage = "Invalid input. Please try again.";
    private const char ObscurityChar = '*';

    /// <summary>
    /// Gets a secret user input without any validation. The input is obscured with asterisks.
    /// </summary>
    /// <param name="prompt">The prompt to display to the user.</param>
    /// <param name="input">The user input as a char array, so it can be cleared securely.</param>
    /// <returns>True if the input was successfully obtained, false if the user canceled the input.</returns>
    public static bool GetSecretUserInput(string prompt, out char[] input)
    {
        Console.Write(prompt + ": ");
        return ReadUserInput(true, out input);
    }

    /// <summary>
    /// Gets a secret user input and validates it against a predicate. The input is obscured with asterisks.
    /// If the predicate returns false, the user is prompted to enter the input again.
    /// </summary>
    /// <param name="prompt">The prompt to display to the user.</param>
    /// <param name="acceptanceCriterion">The predicate to validate the input against.</param>
    /// <param name="input">The user input that was accepted by the predicate. It is returned as a char array, so it can be cleared securely.</param>
    /// <param name="rejectionMsg">Optional. The message to display when the input is rejected. If not provided, a default message is used.</param>
    /// <returns>True if the input was successfully obtained, false if the user canceled the input.</returns>
    public static bool GetSecretUserInput(string prompt, Predicate<char[]> acceptanceCriterion, out char[] input, string? rejectionMsg = null)
    {
        rejectionMsg ??= DefaultRejectionMessage;

        Console.Write(prompt + ": ");
        var isValid = false;
        input = [];

        while (!isValid)
        {
            if (!ReadUserInput(true, out input))
            {
                Array.Clear(input, 0, input.Length);
                return false;
            }

            isValid = acceptanceCriterion.Invoke(input);
            if (!isValid)
            {
                Array.Clear(input, 0, input.Length);
                Console.WriteLine(rejectionMsg);
                Console.Write(prompt + ": ");
            }
        }

        return true;
    }

    /// <summary>
    /// Gets a secret user input and validates it against a validation function, which provides an error message if the input is invalid.
    /// The input is obscured with asterisks. If the validation function returns false, the user is prompted to enter the input again.
    /// </summary>
    /// <param name="prompt">The prompt to display to the user.</param>
    /// <param name="acceptanceCriterion">The validation function to validate the input against. It provides an error message as an out parameter if the input is invalid.</param>
    /// <param name="input">The user input that was accepted by the validation function. It is returned as a char array, so it can be cleared securely.</param>
    /// <param name="rejectionMsg">Optional. The message to display when the input is rejected. If provided, it is printed before the error message.</param>
    /// <returns>True if the input was successfully obtained, false if the user canceled the input.</returns>
    public static bool GetSecretUserInput(string prompt, ValidationFunction<char[]> acceptanceCriterion, out char[] input, string? rejectionMsg = null)
    {
        Console.Write(prompt + ": ");
        var isValid = false;
        input = [];

        while (!isValid)
        {
            if (!ReadUserInput(true, out input))
            {
                Array.Clear(input, 0, input.Length);
                return false;
            }

            isValid = acceptanceCriterion.Invoke(input, out var errorMessage);
            if (isValid)
            {
                break;
            }

            if (!string.IsNullOrEmpty(errorMessage))
            {
                Console.WriteLine(rejectionMsg);
            }

            Console.WriteLine(errorMessage);
            Console.Write(prompt + ": ");
        }

        return true;
    }

    /// <summary>
    /// Gets a user confirmation (Y/N). If the input is canceled, it is interpreted as a "no".
    /// </summary>
    /// <param name="prompt">The prompt to display to the user.</param>
    /// <returns>True if the user confirmed with 'Y', false otherwise.</returns>
    public static bool GetUserConfirmation(string prompt)
    {
        if (!GetUserInput($"{prompt} (Y/N)", IsYesOrNo, out var choice, "Please enter either 'Y' or 'N'"))
        {
            // Cancelled input is interpreted as 'no'
            return false;
        }

        return new string(choice).Equals("Y", StringComparison.InvariantCultureIgnoreCase);
    }

    /// <summary>
    /// Gets user input without any validation.
    /// </summary>
    /// <param name="prompt">The prompt to display to the user.</param>
    /// <param name="input">The user input as a string.</param>
    /// <returns>True if the input was successfully obtained, false if the user canceled the input.</returns>
    public static bool GetUserInput(string prompt, out string input)
    {
        Console.Write(prompt + ": ");
        if (ReadUserInput(false, out var charArray))
        {
            input = new string(charArray);
            return true;
        }

        input = string.Empty;
        return false;
    }

    /// <summary>
    /// Gets user input and validates it against a predicate. If the predicate returns false, the user is prompted to enter the input again.
    /// </summary>
    /// <param name="prompt">The prompt to display to the user.</param>
    /// <param name="acceptanceCriterion">The predicate to validate the input against.</param>
    /// <param name="input">The user input that was accepted by the predicate.</param>
    /// <param name="rejectionMsg">The message to display when the input is rejected. Optional. If not provided, a default message is used.</param>
    /// <returns>True if the input was successfully obtained, false if the user canceled the input.</returns>
    public static bool GetUserInput(string prompt, Predicate<string> acceptanceCriterion, out string input, string? rejectionMsg = null)
    {
        rejectionMsg ??= DefaultRejectionMessage;

        Console.Write(prompt + ": ");
        var isValid = false;
        input = string.Empty;

        while (!isValid)
        {
            if (!ReadUserInput(false, out var charArray))
            {
                input = string.Empty;
                return false;
            }

            input = new string(charArray);
            isValid = acceptanceCriterion.Invoke(input);

            if (!isValid)
            {
                Console.WriteLine(rejectionMsg);
                Console.Write(prompt + ": ");
            }
        }

        return true;
    }

    /// <summary>
    /// Gets user input and validates it against a validation function, which can provide an error message.
    /// If the validation function returns false, the user is prompted to enter the input again by displaying the error message.
    /// </summary>
    /// <param name="prompt">The prompt to display to the user.</param>
    /// <param name="acceptanceCriterion">The validation function to validate the input against. It provides an error message as an out parameter if the input is invalid.</param>
    /// <param name="input">The user input as a string that was accepted by the validation function.</param>
    /// <param name="rejectionMsg">Optional. The message to display when the input is rejected. If provided, it is printed before the error message.</param>
    /// <returns>True if the input was successfully obtained, false if the user canceled the input.</returns>
    public static bool GetUserInput(string prompt, ValidationFunction<string> acceptanceCriterion, out string input, string? rejectionMsg = null)
    {
        Console.Write(prompt + ": ");
        var isValid = false;
        input = string.Empty;

        while (!isValid)
        {
            if (!ReadUserInput(false, out var charArray))
            {
                input = string.Empty;
                return false;
            }

            input = new string(charArray);
            isValid = acceptanceCriterion.Invoke(input, out var errorMessage);

            if (isValid)
            {
                break;
            }

            if (!string.IsNullOrEmpty(errorMessage))
            {
                Console.WriteLine(rejectionMsg);
            }

            Console.WriteLine(errorMessage);
            Console.Write(prompt + ": ");
        }

        return true;
    }

    internal static void PromptContinue()
    {
        Console.WriteLine("Press any key to continue...");
        Console.WriteLine();
        Console.ReadKey(true);
    }

    private static bool IsYesOrNo(string input)
    {
        return input.Equals("Y", StringComparison.InvariantCultureIgnoreCase)
            || input.Equals("N", StringComparison.InvariantCultureIgnoreCase);
    }

    private static bool ReadUserInput(bool isSecret, out char[] input)
    {
        var inputList = new List<char>();

        while (true)
        {
            var keyInfo = Console.ReadKey(intercept: true);

            if (keyInfo.Key == ConsoleKey.Escape)
            {
                // Cancel input
                if (isSecret)
                    SecureClear(inputList);

                Console.WriteLine();
                Console.WriteLine("Input cancelled by user.");
                input = [];
                return false;
            }

            if (keyInfo.Key == ConsoleKey.Enter)
            {
                // End input
                Console.WriteLine();
                break;
            }

            if (keyInfo.Key == ConsoleKey.Backspace && inputList.Count > 0)
            {
                // Remove the last character from the input
                inputList.RemoveAt(inputList.Count - 1);
                Console.Write("\b \b");
                continue;
            }

            if (keyInfo.Key == ConsoleKey.Backspace)
            {
                // Input is empty, so do nothing
                continue;
            }

            inputList.Add(keyInfo.KeyChar);
            Console.Write(isSecret ? ObscurityChar : keyInfo.KeyChar);
        }

        input = inputList.ToArray();

        if (isSecret)
        {
            SecureClear(inputList);
        }

        return true;
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

/// <summary>
/// Represents a function that validates user input.
/// </summary>
/// <typeparam name="T">The type of the user input to validate.</typeparam>
/// <param name="input">The user input to validate.</param>
/// <param name="errorMessage">The validation error message. If the input is valid, this is an empty string.</param>
public delegate bool ValidationFunction<T>(T input, out string errorMessage);

public class UserInputCanceledException : OperationCanceledException
{
    public UserInputCanceledException() : base("Input cancelled by user.") { }
}