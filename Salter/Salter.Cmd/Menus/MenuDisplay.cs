namespace Salter.Cmd.Menus;
internal class MenuDisplay
{
    private const string ExitKey = "X";
    private const string GoBackKey = "B";
    private const string MenuItemSeparator = " | ";
    private const string TitleSeparator = "==";

    private readonly MenuNavigator _menuNavigator;

    private bool _isCurrentMenuChanged = false;
    private bool _isExitRequested = false;
    private bool _isStartup = true;

    public MenuDisplay(MenuNavigator menuNavigator)
    {
        _menuNavigator = menuNavigator;
        _menuNavigator.MenuChanged += OnMenuChanged;
        _menuNavigator.ExitRequested += OnExitRequested;
    }

    public async Task StartAsync()
    {
        var isExit = false;
        while (!isExit && !_isExitRequested)
        {
            _isCurrentMenuChanged = false;            

            DisplayCurrentMenu();

            Console.Write("\nChoose an option: ");
            var input = Console.ReadLine() ?? string.Empty;
            Console.WriteLine();

            if (!IsValidUserInput(input))
            {
                Console.WriteLine("Please enter a valid menu item.");
                ConsoleInputHelper.PromptContinue();
                continue;
            }

            isExit = await HandleUserInputAsync(input).ConfigureAwait(false);

            if (!_isCurrentMenuChanged && !_isExitRequested)
            {
                ConsoleInputHelper.PromptContinue();
            }
        }
    }

    private static void ShowWelcomeScreen()
    {
        Console.WriteLine(AppConstants.AppAsciiArt);
        Console.WriteLine();
    }

    private static string GetMenuItemText(string displayIndex, string title) => displayIndex + MenuItemSeparator + title;

    private void DisplayCurrentMenu()
    {
        if (_isStartup)
        {
            ShowWelcomeScreen();
        }

        var currentMenu = _menuNavigator.CurrentMenu;
        var displayTitle = $"{TitleSeparator} {currentMenu.Title} {TitleSeparator}";

        if (!_isStartup)
        {
            Console.Clear();
        }

        if (_isStartup)
        {
            _isStartup = false;
        }

        Console.WriteLine(displayTitle);
        Console.WriteLine();

        for (int i = 0; i < currentMenu.MenuItems.Count; i++)
        {
            var displayIndex = i + 1;
            var menuItemTitle = currentMenu.MenuItems[i].Title;
            var menuItemText = displayIndex + MenuItemSeparator + menuItemTitle;

            Console.WriteLine(menuItemText);
        }

        if (currentMenu.HasGoBack)
        {
            var goBackMenuItemText = GetMenuItemText(GoBackKey.ToString(), "Go Back");
            Console.WriteLine(goBackMenuItemText);
        }

        var exitMenuItemText = GetMenuItemText(ExitKey.ToString(), "Exit");
        Console.WriteLine(exitMenuItemText);
    }

    /// <summary>
    /// Handles the user input and returns a boolean indicating if the application should exit.
    /// </summary>
    private async Task<bool> HandleUserInputAsync(string input)
    {
        var currentMenu = _menuNavigator.CurrentMenu;

        if (input.Equals(ExitKey, StringComparison.OrdinalIgnoreCase))
        {
            currentMenu.Exit();
            return true;
        }

        if (input.Equals(GoBackKey, StringComparison.OrdinalIgnoreCase))
        {
            currentMenu.NavigateToPreviousMenu();
            return true;
        }

        // This is safe because we've already validated the input
        var displayIndex = int.Parse(input);
        await currentMenu.ExecuteMenuItemAsync(displayIndex).ConfigureAwait(false);

        return false;
    }

    private bool IsValidUserInput(string input)
    {
        if (!int.TryParse(input, out var displayDigit))
        {
            return input.Equals(ExitKey, StringComparison.OrdinalIgnoreCase) ||
                   input.Equals(GoBackKey, StringComparison.OrdinalIgnoreCase);
        }

        return _menuNavigator.CurrentMenu.HasItem(displayDigit);
    }

    private void OnExitRequested(object? sender, EventArgs e)
    {
        _isExitRequested = true;
        Console.WriteLine("Exiting...");
    }

    private void OnMenuChanged(object? sender, EventArgs e)
    {
        _isCurrentMenuChanged = true;
        DisplayCurrentMenu();
    }
}
