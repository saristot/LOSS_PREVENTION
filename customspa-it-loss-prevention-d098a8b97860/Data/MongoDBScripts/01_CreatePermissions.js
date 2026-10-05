// =========================================================================
// MongoDB Script: Create All Required Permissions
// =========================================================================
// Purpose: Creates all permissions needed for the LossPrevention API
// Permission names are derived directly from the API endpoint definitions.
// Run this script in MongoDB Compass or mongosh
// =========================================================================

var db = db.getSiblingDB('LossPrevention');

var permissionsToCreate = [
  // ==================== USER MANAGEMENT ====================
  { "PermissionName": "CAN_CREATE_USER",        "PermissionText": "Create User",        "Description": "Allows the user to create new users" },
  { "PermissionName": "CAN_VIEW_USER",           "PermissionText": "View User",           "Description": "Allows the user to view user details" },
  { "PermissionName": "CAN_UPDATE_USER",         "PermissionText": "Update User",         "Description": "Allows the user to update user information" },
  { "PermissionName": "CAN_DELETE_USER",         "PermissionText": "Delete User",         "Description": "Allows the user to delete users" },

  // ==================== ROLE MANAGEMENT ====================
  { "PermissionName": "CAN_CREATE_ROLE",         "PermissionText": "Create Role",         "Description": "Allows the user to create new roles" },
  { "PermissionName": "CAN_VIEW_ROLE",           "PermissionText": "View Role",           "Description": "Allows the user to view role details" },
  { "PermissionName": "CAN_UPDATE_ROLE",         "PermissionText": "Update Role",         "Description": "Allows the user to update roles" },
  { "PermissionName": "CAN_DELETE_ROLE",         "PermissionText": "Delete Role",         "Description": "Allows the user to delete roles" },
  { "PermissionName": "CAN_ASSIGN_ROLE",         "PermissionText": "Assign Role",         "Description": "Allows the user to add or remove users to/from roles" },

  // ==================== PERMISSION MANAGEMENT ====================
  { "PermissionName": "CAN_CREATE_PERMISSION",   "PermissionText": "Create Permission",   "Description": "Allows the user to create new permissions" },
  { "PermissionName": "CAN_VIEW_PERMISSION",     "PermissionText": "View Permission",     "Description": "Allows the user to view permissions" },
  { "PermissionName": "CAN_UPDATE_PERMISSION",   "PermissionText": "Update Permission",   "Description": "Allows the user to update permissions" },
  { "PermissionName": "CAN_DELETE_PERMISSION",   "PermissionText": "Delete Permission",   "Description": "Allows the user to delete permissions" },
  { "PermissionName": "CAN_ASSIGN_PERMISSION",   "PermissionText": "Assign Permission",   "Description": "Allows the user to add or remove permissions to/from roles" },

  // ==================== WORKSPACE MANAGEMENT ====================
  { "PermissionName": "CAN_VIEW_WORKSPACES",     "PermissionText": "View Workspaces",     "Description": "Allows the user to view workspaces" },
  { "PermissionName": "CAN_MANAGE_WORKSPACES",   "PermissionText": "Manage Workspaces",   "Description": "Allows the user to create, update, and delete workspaces" },

  // ==================== DASHBOARD MANAGEMENT ====================
  { "PermissionName": "CAN_VIEW_DASHBOARD",      "PermissionText": "View Dashboard",      "Description": "Allows the user to view dashboards" },
  { "PermissionName": "CAN_MANAGE_DASHBOARDS",   "PermissionText": "Manage Dashboards",   "Description": "Allows the user to create, update, and delete dashboards" },

  // ==================== DATA & REPORT MANAGEMENT ====================
  { "PermissionName": "CAN_CREATE_TRANSACTION",  "PermissionText": "Create Transaction",  "Description": "Allows the user to create transactions" },
  { "PermissionName": "CAN_VIEW_REPORT",         "PermissionText": "View Report",         "Description": "Allows the user to view reports and data" },

  // ==================== MAPPING MANAGEMENT ====================
  { "PermissionName": "CAN_CREATE_MAPPINGS",     "PermissionText": "Create Mappings",     "Description": "Allows the user to create field mappings" },
  { "PermissionName": "CAN_VIEW_MAPPINGS",       "PermissionText": "View Mappings",       "Description": "Allows the user to view field mappings" },
  { "PermissionName": "CAN_UPDATE_MAPPINGS",     "PermissionText": "Update Mappings",     "Description": "Allows the user to update field mappings" },
  { "PermissionName": "CAN_DELETE_MAPPINGS",     "PermissionText": "Delete Mappings",     "Description": "Allows the user to delete field mappings" },

  // ==================== RULE MANAGEMENT ====================
  { "PermissionName": "CAN_CREATE_RULE",         "PermissionText": "Create Rule",         "Description": "Allows the user to create rules" },
  { "PermissionName": "CAN_VIEW_RULE",           "PermissionText": "View Rule",           "Description": "Allows the user to view rules" },
  { "PermissionName": "CAN_UPDATE_RULE",         "PermissionText": "Update Rule",         "Description": "Allows the user to update rules" },
  { "PermissionName": "CAN_DELETE_RULE",         "PermissionText": "Delete Rule",         "Description": "Allows the user to delete rules" },
  { "PermissionName": "CAN_APPLY_RULE",          "PermissionText": "Apply Rule",          "Description": "Allows the user to apply rules to data" },

  // ==================== DATA INGESTION MANAGEMENT ====================
  { "PermissionName": "CAN_MANAGE_DATA_INGESTION", "PermissionText": "Manage Data Ingestion", "Description": "Allows the user to configure and run data ingestion processes" },
  { "PermissionName": "CAN_VIEW_DATA_INGESTION",   "PermissionText": "View Data Ingestion",   "Description": "Allows the user to view data ingestion configuration" },

  // ==================== NOTIFICATION MANAGEMENT ====================
  { "PermissionName": "CAN_MANAGE_NOTIFICATIONS", "PermissionText": "Manage Notifications", "Description": "Allows the user to create, reply to, mark as read, and delete notifications" },
  { "PermissionName": "CAN_VIEW_NOTIFICATIONS",   "PermissionText": "View Notifications",   "Description": "Allows the user to view notifications" },

  // ==================== GROUP MANAGEMENT ====================
  { "PermissionName": "CAN_MANAGE_GROUPS",       "PermissionText": "Manage Groups",       "Description": "Allows the user to create, update, delete groups and manage members" },
  { "PermissionName": "CAN_VIEW_GROUPS",         "PermissionText": "View Groups",         "Description": "Allows the user to view groups" },

  // ==================== FRAUD DETECTION SETTINGS ====================
  { "PermissionName": "CAN_VIEW_FRAUD_SETTINGS",   "PermissionText": "View Fraud Settings",   "Description": "Allows the user to view fraud detection threshold settings" },
  { "PermissionName": "CAN_MANAGE_FRAUD_SETTINGS", "PermissionText": "Manage Fraud Settings", "Description": "Allows the user to create and update fraud detection threshold settings" }
];

var inserted = 0;
var updated = 0;

print("========================================");
print("Processing Permissions...");
print("========================================");

permissionsToCreate.forEach(function(permission) {
  var existing = db.Permissions.findOne({ PermissionName: permission.PermissionName });
  if (existing) {
    db.Permissions.updateOne(
      { PermissionName: permission.PermissionName },
      { $set: { PermissionText: permission.PermissionText, Description: permission.Description } }
    );
    print("  Updated: " + permission.PermissionName);
    updated++;
  } else {
    db.Permissions.insertOne(permission);
    print("  Created: " + permission.PermissionName);
    inserted++;
  }
});

print("");
print("========================================");
print("Process Complete!");
print("New:     " + inserted);
print("Updated: " + updated);
print("Total:   " + db.Permissions.countDocuments());
print("========================================");
