# Complete Permissions List

## All Permissions for LossPrevention API

This document lists all 40 permissions that have been created for the system.

---

## User Management (4 permissions)
1. `CAN_CREATE_USER` - Create User - Allows the user to create new users
2. `CAN_VIEW_USER` - View User - Allows the user to view user details
3. `CAN_UPDATE_USER` - Update User - Allows the user to update user information
4. `CAN_DELETE_USER` - Delete User - Allows the user to delete users

---

## Role Management (5 permissions)
5. `CAN_CREATE_ROLE` - Create Role - Allows the user to create new roles
6. `CAN_VIEW_ROLE` - View Role - Allows the user to view role details
7. `CAN_UPDATE_ROLE` - Update Role - Allows the user to update roles
8. `CAN_DELETE_ROLE` - Delete Role - Allows the user to delete roles
9. `CAN_ASSIGN_ROLE` - Assign Role - Allows the user to add or remove users to/from roles

---

## Permission Management (5 permissions)
10. `CAN_CREATE_PERMISSION` - Create Permission - Allows the user to create new permissions
11. `CAN_VIEW_PERMISSION` - View Permission - Allows the user to view permissions
12. `CAN_UPDATE_PERMISSION` - Update Permission - Allows the user to update permissions
13. `CAN_DELETE_PERMISSION` - Delete Permission - Allows the user to delete permissions
14. `CAN_ASSIGN_PERMISSION` - Assign Permission - Allows the user to add or remove permissions to/from roles

---

## Workspace Management (4 permissions)
15. `CAN_CREATE_WORKSPACE` - Create Workspace - Allows the user to create workspaces
16. `CAN_VIEW_WORKSPACE` - View Workspace - Allows the user to view workspaces
17. `CAN_UPDATE_WORKSPACE` - Update Workspace - Allows the user to update workspaces
18. `CAN_DELETE_WORKSPACE` - Delete Workspace - Allows the user to delete workspaces

---

## Dashboard Management (5 permissions)
19. `CAN_CREATE_DASHBOARD` - Create Dashboard - Allows the user to create dashboards
20. `CAN_VIEW_DASHBOARD` - View Dashboard - Allows the user to view dashboards
21. `CAN_UPDATE_DASHBOARD` - Update Dashboard - Allows the user to update dashboards
22. `CAN_DELETE_DASHBOARD` - Delete Dashboard - Allows the user to delete dashboards
23. `CAN_MANAGE_DASHBOARDS` - Manage Dashboards - Allows the user to create, view, update, and delete dashboards

---

## Data & Report Management (3 permissions)
24. `CAN_CREATE_TRANSACTION` - Create Transaction - Allows the user to create transactions
25. `CAN_VIEW_REPORT` - View Report - Allows the user to view reports and data
26. `CAN_ANALYZE_DATA` - Analyze Data - Allows the user to run data analysis (distance, trends, etc.)

---

## Mapping Management (4 permissions)
27. `CAN_CREATE_MAPPINGS` - Create Mappings - Allows the user to create field mappings
28. `CAN_VIEW_MAPPINGS` - View Mappings - Allows the user to view field mappings
29. `CAN_UPDATE_MAPPINGS` - Update Mappings - Allows the user to update field mappings
30. `CAN_DELETE_MAPPINGS` - Delete Mappings - Allows the user to delete field mappings

---

## Rule Management (5 permissions)
31. `CAN_CREATE_RULE` - Create Rule - Allows the user to create rules
32. `CAN_VIEW_RULE` - View Rule - Allows the user to view rules
33. `CAN_UPDATE_RULE` - Update Rule - Allows the user to update rules
34. `CAN_DELETE_RULE` - Delete Rule - Allows the user to delete rules
35. `CAN_APPLY_RULE` - Apply Rule - Allows the user to apply rules to data

---

## Data Ingestion Management (2 permissions)
36. `CAN_MANAGE_DATA_INGESTION` - Manage Data Ingestion - Allows the user to configure and run data ingestion processes
37. `CAN_VIEW_DATA_INGESTION` - View Data Ingestion - Allows the user to view data ingestion configuration

---

## Notification Management (1 permission)
38. `CAN_MANAGE_NOTIFICATIONS` - Manage Notifications - Allows the user to create, view, update, and delete notifications

---

## Group Management (1 permission)
39. `CAN_MANAGE_GROUPS` - Manage Groups - Allows the user to create, view, update, and delete groups

---

## Fraud Detection Settings (2 permissions)
40. `CAN_VIEW_FRAUD_SETTINGS` - View Fraud Settings - Allows the user to view fraud detection threshold settings
41. `CAN_MANAGE_FRAUD_SETTINGS` - Manage Fraud Settings - Allows the user to create and update fraud detection threshold settings

---

## Quick Reference by Permission Name

- CAN_ANALYZE_DATA
- CAN_APPLY_RULE
- CAN_ASSIGN_PERMISSION
- CAN_ASSIGN_ROLE
- CAN_CREATE_DASHBOARD
- CAN_CREATE_MAPPINGS
- CAN_CREATE_PERMISSION
- CAN_CREATE_ROLE
- CAN_CREATE_RULE
- CAN_CREATE_TRANSACTION
- CAN_CREATE_USER
- CAN_CREATE_WORKSPACE
- CAN_DELETE_DASHBOARD
- CAN_DELETE_MAPPINGS
- CAN_DELETE_PERMISSION
- CAN_DELETE_ROLE
- CAN_DELETE_RULE
- CAN_DELETE_USER
- CAN_DELETE_WORKSPACE
- CAN_MANAGE_DASHBOARDS
- CAN_MANAGE_DATA_INGESTION
- CAN_MANAGE_FRAUD_SETTINGS
- CAN_MANAGE_GROUPS
- CAN_MANAGE_NOTIFICATIONS
- CAN_UPDATE_DASHBOARD
- CAN_UPDATE_MAPPINGS
- CAN_UPDATE_PERMISSION
- CAN_UPDATE_ROLE
- CAN_UPDATE_RULE
- CAN_UPDATE_USER
- CAN_UPDATE_WORKSPACE
- CAN_VIEW_DASHBOARD
- CAN_VIEW_DATA_INGESTION
- CAN_VIEW_FRAUD_SETTINGS
- CAN_VIEW_MAPPINGS
- CAN_VIEW_PERMISSION
- CAN_VIEW_REPORT
- CAN_VIEW_ROLE
- CAN_VIEW_USER
- CAN_VIEW_WORKSPACE

---

## Total: 41 Permissions

All of these permissions will be added to the Admin role when you run `02_AddPermissionsToAdminRole.js`.
