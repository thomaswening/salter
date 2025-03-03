using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Salter.Core.UserManagement;

namespace Salter.Cmd.Menus;

internal abstract class Menu(AuthenticationService authService)
{
    protected readonly AuthenticationService _authService = authService;
    protected bool _canProceedToSubMenu = true;

    public event EventHandler? ExitRequested;
    public event EventHandler? GoBackRequested;
    public event EventHandler<Menu>? NavigationRequested;
    public event EventHandler? ReturnToRootRequested;

    public bool HasGoBack { get; protected init; }
    public MenuItem GoBackMenuItem => new("Go Back", () => GoBackRequested?.Invoke(this, EventArgs.Empty));
    public MenuItem ExitMenuItem => new("Exit", () => ExitRequested?.Invoke(this, EventArgs.Empty));

    /// <summary>
    /// All menu items, including base menu items, in the order they should be displayed.
    /// </summary>
    public List<MenuItem> MenuItems => CreateFilteredMenuItems();

    private List<MenuItem> CreateFilteredMenuItems()
    {
        var items = CreateMenuItems();

        var currentUser = _authService.CurrentUser;

        // Filter out items the user does not fulfill the permission check for
        // If no permission check is provided, the item is always shown - even if no user is logged in

        return items.Where(i => i.PermissionCheck is null 
            || currentUser is not null && i.PermissionCheck(currentUser)).ToList();
    }

    public abstract string Title { get; }

    /// <summary>
    /// Implement this method to provide the items specific to the menu.
    /// </summary>
    protected abstract List<MenuItem> CreateMenuItems();

    /// <summary>
    /// Determines if the menu has an item at the specified display index.
    /// </summary>
    public bool HasItem(int displayIndex)
    {
        var index = displayIndex - 1; // Adjust for 1-based display index
        return index >= 0 && index < MenuItems.Count;
    }

    public void Exit()
    {
        OnExit();
        ExitRequested?.Invoke(this, EventArgs.Empty);
    }

    public void NavigateToPreviousMenu()
    {
        OnGoBack();
        GoBackRequested?.Invoke(this, EventArgs.Empty);
    }

    public async Task ExecuteMenuItemAsync(int displayIndex)
    {
        _canProceedToSubMenu = true;

        var index = displayIndex - 1; // Adjust for 1-based display index

        if (index < 0 || index >= MenuItems.Count)
        {
            throw new InvalidOperationException($"Invalid menu item index. Menu: {Title}, Index: {index}");
        }

        var selectedItem = MenuItems[index];

        try
        {
            if (selectedItem.Action is not null)
            {
                selectedItem.Action();
            }
            else if (selectedItem.AsyncAction is not null)
            {
                await selectedItem.AsyncAction().ConfigureAwait(false);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("An unexpected error occurred.");
            Console.WriteLine(ExceptionHelper.UnpackException(ex));
            _canProceedToSubMenu = false;

            // Do not navigate to sub-menu on error
            return;
        }

        if (_canProceedToSubMenu && selectedItem.SubMenu is not null)
        {
            RequestNavigationTo(selectedItem.SubMenu);
        }
    }

    /// <summary>
    /// Is executed before exiting the menu.
    /// </summary>
    protected abstract void OnExit();

    /// <summary>
    /// Is executed before going back to the previous menu.
    /// </summary>
    protected abstract void OnGoBack();

    protected void RequestNavigationTo(Menu menu) => NavigationRequested?.Invoke(this, menu);
}

internal class NavigationRequest(Menu menu) : EventArgs
{
    public Menu Menu { get; } = menu;
}
