namespace FurnitureEPR.Application.Security;

public static class PermissionNames
{
    public const string WorkflowManage = "workflow.manage";
    public const string WorkflowMove = "workflow.move";
    public const string WorkflowQualityControl = "workflow.quality_control";
    public const string WorkflowComplete = "workflow.complete";
    public const string WorkflowViewTasks = "workflow.view_tasks";

    public const string OrdersViewAll = "orders.view_all";
    public const string OrdersCreate = "orders.create";
    public const string OrdersEditDraft = "orders.edit_draft";

    public const string IdentityManageUsers = "identity.manage_users";
    public const string IdentityManageMasterData = "identity.manage_master_data";
    public const string IdentityManageRoles = "identity.manage_roles";
    public const string IdentityManageClaims = "identity.manage_claims";
}
