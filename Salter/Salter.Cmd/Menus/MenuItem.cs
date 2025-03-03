using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Salter.Core.UserManagement;

namespace Salter.Cmd.Menus;

internal class MenuItem
{
    public string Title { get; }
    public Action? Action { get; }
    public Func<Task>? AsyncAction { get; }
    public Predicate<User>? PermissionCheck { get; }
    public Menu? SubMenu { get; }

    private MenuItem(string title, Action? action = null, Func<Task>? asyncAction = null, Predicate<User>? accessCondition = null, Menu? subMenu = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(title, nameof(title));

        Title = title;
        Action = action;
        AsyncAction = asyncAction;
        PermissionCheck = accessCondition;
        SubMenu = subMenu;
    }

    public MenuItem (string title, Action action, Predicate<User>? accessCondition = null, Menu? subMenu = null) 
        : this(title, action, null, accessCondition, subMenu)
    { }

    public MenuItem(string title, Action action, Role requiredRole, Menu? subMenu = null)
        : this(title, action, null, u => u.HasRole(requiredRole), subMenu)
    { }
    

    public MenuItem(string title, Func<Task> asyncAction, Predicate<User>? accessCondition = null, Menu? subMenu = null)
        : this(title, null, asyncAction, accessCondition, subMenu)
    { }

    public MenuItem(string title, Func<Task> asyncAction, Role requiredRole, Menu? subMenu = null)
        : this(title, null, asyncAction, u => u.HasRole(requiredRole), subMenu)
    { }
}
