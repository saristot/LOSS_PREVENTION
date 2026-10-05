// =========================================================================
// MongoDB Script: Add All Permissions to Admin Role
// =========================================================================
// Purpose: Assigns all system permissions to the Admin role
// Prerequisites: 
//   1. Run 01_CreatePermissions.js first
//   2. Admin role must exist (RoleName: "Admin")
// =========================================================================

var db = db.getSiblingDB('LossPrevention');

// Step 1: Get all permission IDs
var permissionIds = db.Permissions.find({}, { _id: 1 }).toArray().map(p => p._id);

print("========================================");
print("?? Found " + permissionIds.length + " permissions");
print("========================================");

// Step 2: Find the Admin role
var adminRole = db.Roles.findOne({ RoleName: "Admin" });

if (!adminRole) {
  print("? ERROR: Admin role not found!");
  print("Please create the Admin role first.");
  print("");
  print("Example:");
  print('db.Roles.insertOne({');
  print('  "RoleName": "Admin",');
  print('  "Description": "Admin Permissions",');
  print('  "Permissions": []');
  print('});');
} else {
  print("? Admin role found: " + adminRole._id);
  print("");
  
  // Step 3: Update Admin role with all permissions
  db.Roles.updateOne(
    { _id: adminRole._id },
    { $set: { Permissions: permissionIds } }
  );
  
  print("========================================");
  print("? Permissions Added to Admin Role!");
  print("========================================");
  print("Role ID: " + adminRole._id);
  print("Total Permissions: " + permissionIds.length);
  print("");
  
  // Step 4: Verify the update
  var updatedRole = db.Roles.findOne({ _id: adminRole._id });
  print("Verification:");
  print("  � Permissions in role: " + updatedRole.Permissions.length);
  print("");
  
  // Step 5: Display all permissions in the Admin role
  print("Permissions List:");
  db.Permissions.find({ _id: { $in: updatedRole.Permissions } }, { PermissionName: 1, PermissionText: 1, _id: 0 })
    .sort({ PermissionName: 1 })
    .forEach(p => {
      print("  � " + p.PermissionName + " - " + p.PermissionText);
    });
}

print("");
print("========================================");
print("? Script Complete!");
print("========================================");
