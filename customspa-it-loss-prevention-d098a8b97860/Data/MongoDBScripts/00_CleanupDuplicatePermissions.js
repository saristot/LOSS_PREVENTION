// =========================================================================
// MongoDB Script: Clean Up Duplicate Permissions
// =========================================================================
// Purpose: Remove duplicate permissions from the database
// This script will keep only the first occurrence of each permission
// =========================================================================

var db = db.getSiblingDB('LossPrevention');

print("========================================");
print("?? Scanning for Duplicate Permissions...");
print("========================================");
print("");

// Get all permission names with their counts
var duplicates = db.Permissions.aggregate([
  {
    $group: {
      _id: "$PermissionName",
      count: { $sum: 1 },
      ids: { $push: "$_id" }
    }
  },
  {
    $match: {
      count: { $gt: 1 }
    }
  }
]).toArray();

if (duplicates.length === 0) {
  print("? No duplicates found! Database is clean.");
} else {
  print("Found " + duplicates.length + " duplicate permissions:");
  print("");
  
  var totalRemoved = 0;
  
  duplicates.forEach(function(dup) {
    print("  � " + dup._id + " (" + dup.count + " copies)");
    
    // Keep the first one, remove the rest
    var idsToRemove = dup.ids.slice(1); // Skip the first ID
    
    idsToRemove.forEach(function(id) {
      db.Permissions.deleteOne({ _id: id });
      totalRemoved++;
    });
  });
  
  print("");
  print("========================================");
  print("? Cleanup Complete!");
  print("========================================");
  print("Duplicates Removed: " + totalRemoved);
  print("Unique Permissions Remaining: " + db.Permissions.countDocuments());
}

print("");
print("Current Permissions in Database:");
db.Permissions.find({}, { PermissionName: 1, PermissionText: 1, _id: 0 })
  .sort({ PermissionName: 1 })
  .forEach(p => {
    print("  � " + p.PermissionName + " (" + p.PermissionText + ")");
  });

print("");
print("========================================");
print("? Script Complete!");
print("========================================");
