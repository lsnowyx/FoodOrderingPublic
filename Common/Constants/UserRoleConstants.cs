namespace Common.Constants;

public static class UserRoleConstants
{
    public static readonly string[] ROLES = new[] { ADMIN_ROLE, USER_ROLE, WORKER_ROLE, MENU_MANAGER_ROLE, ORDER_MANAGER_ROLE };
    public static readonly string[] ADMIN_PANEL_ROLES = new[] { ADMIN_ROLE, MENU_MANAGER_ROLE, ORDER_MANAGER_ROLE };
    public static readonly string[] ADMIN_CREATABLE_ROLES = new[] { MENU_MANAGER_ROLE, ORDER_MANAGER_ROLE };

    public const string ADMIN_ROLE = "Admin";
    public const string USER_ROLE = "User";
    public const string WORKER_ROLE = "Worker";

    // Specific admin roles
    public const string MENU_MANAGER_ROLE = "MenuManager";
    public const string ORDER_MANAGER_ROLE = "OrderManager";
}
