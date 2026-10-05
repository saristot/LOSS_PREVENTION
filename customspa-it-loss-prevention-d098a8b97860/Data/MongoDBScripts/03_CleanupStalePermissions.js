// =========================================================================
// MongoDB Script: Remove Stale Permissions
// =========================================================================
// Purpose: Removes permissions that are no longer referenced by any API
//          endpoint, and removes orphaned permission IDs from all roles.
// Run AFTER 01_CreatePermissions.js
// =========================================================================

var db = db.getSiblingDB('LossPrevention');

// Permissions that no longer match any API Permissions() declaration
var stalePermissionNames = [
  "CAN_LOGIN",        // never checked by any endpoint
  "CAN_ADD_USERS",    // replaced by CAN_CREATE_USER
  "CAN_DELETE_USERS", // replaced by CAN_DELETE_USER
  "CAN_UPDATE_USERS"  // replaced by CAN_UPDATE_USER
];

print("========================================");
print("Step 1: Removing stale permissions...");
print("========================================");

var staleIds = [];
stalePermissionNames.forEach(function(name) {
  var perm = db.Permissions.findOne({ PermissionName: name });
  if (perm) {
    staleIds.push(perm._id);
    db.Permissions.deleteOne({ _id: perm._id });
    print("  Deleted: " + name + " (" + perm._id + ")");
  } else {
    print("  Not found (already removed): " + name);
  }
});

print("");
print("========================================");
print("Step 2: Removing orphaned IDs from roles...");
print("========================================");

// Build the set of all valid permission IDs currently in the collection
var validIds = db.Permissions.find({}, { _id: 1 }).toArray().map(p => p._id.toString());

db.Roles.find({}).forEach(function(role) {
  if (!role.Permissions || role.Permissions.length === 0) return;

  var originalCount = role.Permissions.length;
  var cleanPermissions = role.Permissions.filter(function(id) {
    return validIds.includes(id.toString());
  });

  if (cleanPermissions.length !== originalCount) {
    var removed = originalCount - cleanPermissions.length;
    db.Roles.updateOne(
      { _id: role._id },
      { $set: { Permissions: cleanPermissions } }
    );
    print("  Role '" + role.RoleName + "': removed " + removed + " orphaned ID(s)");
  } else {
    print("  Role '" + role.RoleName + "': no orphaned IDs");
  }
});

print("");
print("========================================");
print("Cleanup Complete!");
print("Remaining permissions: " + db.Permissions.countDocuments());
print("========================================");
