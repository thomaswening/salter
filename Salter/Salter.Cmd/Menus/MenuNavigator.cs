namespace Salter.Cmd.Menus;
internal class MenuNavigator
{
    private readonly Stack<Menu> _menuStack = new();
    public MenuNavigator(Menu rootMenu)
    {
        _menuStack.Push(rootMenu);
        AttachCurrentMenuEvents();
    }

    public event EventHandler? ExitRequested;
    public event EventHandler? MenuChanged;

    public Menu CurrentMenu => _menuStack.Peek();

    private void OnGoBackRequested(object? sender, EventArgs e)
    {
        _menuStack.Pop();
        AttachCurrentMenuEvents();
        MenuChanged?.Invoke(this, EventArgs.Empty);
    }

    private void OnNavigationRequested(object? sender, Menu requestedMenu)
    {
        _menuStack.Push(requestedMenu);
        AttachCurrentMenuEvents();
        MenuChanged?.Invoke(this, EventArgs.Empty);
    }

    private void AttachCurrentMenuEvents()
    {
        CurrentMenu.GoBackRequested += OnGoBackRequested;
        CurrentMenu.ExitRequested += (sender, e) => ExitRequested?.Invoke(sender, e);
        CurrentMenu.NavigationRequested += OnNavigationRequested;
    }
}
