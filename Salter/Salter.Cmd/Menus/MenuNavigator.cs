namespace Salter.Cmd.Menus;
internal class MenuNavigator
{
    private readonly Stack<Menu> _menuStack = new();
    public MenuNavigator(Menu rootMenu)
    {
        _menuStack.Push(rootMenu);
        SubscribeToMenuEvents();
    }

    public event EventHandler? ExitRequested;
    public event EventHandler? MenuChanged;

    public Menu CurrentMenu => _menuStack.Peek();

    private void OnGoBackRequested(object? sender, EventArgs e)
    {
        UnsubscribeFromMenuEvents();
        _menuStack.Pop();
        MenuChanged?.Invoke(this, EventArgs.Empty);
    }

    private void OnNavigationRequested(object? sender, Menu requestedMenu)
    {
        _menuStack.Push(requestedMenu);
        SubscribeToMenuEvents();
        MenuChanged?.Invoke(this, EventArgs.Empty);
    }

    private void SubscribeToMenuEvents()
    {
        CurrentMenu.GoBackRequested += OnGoBackRequested;
        CurrentMenu.ExitRequested += (sender, e) => ExitRequested?.Invoke(sender, e);
        CurrentMenu.NavigationRequested += OnNavigationRequested;
    }

    private void UnsubscribeFromMenuEvents()
    {
        CurrentMenu.GoBackRequested -= OnGoBackRequested;
        CurrentMenu.ExitRequested -= (sender, e) => ExitRequested?.Invoke(sender, e);
        CurrentMenu.NavigationRequested -= OnNavigationRequested;
    }
}
