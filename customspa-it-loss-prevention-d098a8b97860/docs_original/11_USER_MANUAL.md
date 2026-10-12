# Loss Prevention Tool - User Manual & Admin Guide

**Version 1.0**  
**Last Updated: February 12, 2026**

---

## Table of Contents

1. [Introduction](#introduction)
2. [Getting Started](#getting-started)
3. [User Roles and Permissions](#user-roles-and-permissions)
4. [Login and Authentication](#login-and-authentication)
5. [Dashboard Overview](#dashboard-overview)
6. [Report Builder (Query Builder)](#report-builder-query-builder)
7. [AI-Powered Natural Language Queries](#ai-powered-natural-language-queries)
8. [Fraud Detection Features](#fraud-detection-features)
9. [Euclidean Distance Analysis](#euclidean-distance-analysis)
10. [Data Export](#data-export)
11. [Workspace Management](#workspace-management)
12. [Dashboard Creation and Management](#dashboard-creation-and-management)
13. [Admin Features](#admin-features)
14. [Common Workflows](#common-workflows)
15. [Tips and Best Practices](#tips-and-best-practices)
16. [FAQs](#faqs)
17. [Troubleshooting](#troubleshooting)

---

## Introduction

### What is the Loss Prevention Tool?

The Loss Prevention Tool is an enterprise-grade web application designed to detect, analyze, and prevent fraudulent activities and losses in retail and transaction-based environments. Built with modern technologies, it provides real-time fraud detection, comprehensive reporting, and intelligent data analysis capabilities.

### Key Capabilities

- **Advanced Fraud Detection**: Real-time analysis using configurable thresholds
- **Intelligent Data Analysis**: Euclidean distance analysis, AI-powered queries, and pattern matching
- **Flexible Dashboarding**: Drag-and-drop dashboard designer with multiple visualization types
- **Comprehensive Reporting**: Dynamic report builder with expressions and conditional formatting
- **Enterprise Security**: JWT-based authentication with role-based access control
- **Data Management**: Multi-format data ingestion and automated processing

### Who Should Use This Manual?

- **End Users**: Analysts and loss prevention specialists who use the tool daily
- **Administrators**: IT staff and managers responsible for system configuration and user management
- **Data Managers**: Staff responsible for data ingestion and mappings

---

## Getting Started

### System Requirements

**Supported Browsers:**
- Google Chrome (recommended, version 90+)
- Microsoft Edge (version 90+)
- Mozilla Firefox (version 88+)
- Safari (version 14+)

**Recommended:**
- Screen resolution: 1920x1080 or higher
- Stable internet connection
- JavaScript enabled

### Accessing the System

1. Open your web browser
2. Navigate to the application URL provided by your administrator:
   - Development: `http://localhost:5173`
   - Production: `https://your-company-domain.com`
3. You'll be directed to the Login page

### First Login

#### Default Administrator Account

For initial setup, use the default administrator credentials:

- **Username**: `admin`
- **Password**: `Admin@123` (change immediately after first login)

#### First-Time Setup Steps

1. **Login** with default credentials
2. **Change Your Password**:
   - Click on your username in the top-right corner
   - Select "Change Password"
   - Enter a strong password meeting requirements:
     - Minimum 8 characters
     - At least one uppercase letter
     - At least one lowercase letter
     - At least one number
     - At least one special character
3. **Update Your Profile**:
   - Navigate to User Management
   - Update your email address
   - Set your first and last name
4. **Review Permissions**: Familiarize yourself with available features

### Navigation Overview

The main navigation bar appears at the top of the screen after login:

| Menu Item | Description |
|-----------|-------------|
| **Home** | Dashboard and overview |
| **Workspaces** | Manage workspaces and reports |
| **Query** | Report builder and data analysis |
| **Dashboards** | Create and view dashboards |
| **Manage** | Administration features (if authorized) |
| **Notifications** | View alerts and messages |

---

## User Roles and Permissions

### Understanding Roles

The system uses **Role-Based Access Control (RBAC)** to manage user access. Users are assigned one or more roles, and each role has specific permissions.

### Default Roles

#### Administrator
- **Purpose**: Full system access
- **Typical Users**: IT administrators, system managers
- **Permissions**: All 41 permissions (full access to all features)
- **Capabilities**:
  - User, role, and permission management
  - System configuration
  - All data access
  - Dashboard and report management

#### Analyst
- **Purpose**: Data analysis and reporting
- **Typical Users**: Loss prevention analysts, data analysts
- **Key Permissions**:
  - View reports and data
  - Create and manage dashboards
  - Run fraud detection analysis
  - Execute distance analysis
  - Export data
- **Limitations**:
  - Cannot manage users or roles
  - Cannot configure system settings

#### Viewer
- **Purpose**: Read-only access
- **Typical Users**: Managers, auditors
- **Key Permissions**:
  - View dashboards
  - View reports
  - View notifications
- **Limitations**:
  - Cannot create or edit content
  - Cannot export data (configurable)
  - No administrative access

### Permission Categories

The system includes **41 permissions** organized into categories:

#### User Management (4 permissions)
- `CAN_CREATE_USER` - Create new users
- `CAN_VIEW_USER` - View user details
- `CAN_UPDATE_USER` - Update user information
- `CAN_DELETE_USER` - Delete users

#### Role Management (5 permissions)
- `CAN_CREATE_ROLE` - Create new roles
- `CAN_VIEW_ROLE` - View role details
- `CAN_UPDATE_ROLE` - Update roles
- `CAN_DELETE_ROLE` - Delete roles
- `CAN_ASSIGN_ROLE` - Assign users to roles

#### Permission Management (5 permissions)
- `CAN_CREATE_PERMISSION` - Create permissions
- `CAN_VIEW_PERMISSION` - View permissions
- `CAN_UPDATE_PERMISSION` - Update permissions
- `CAN_DELETE_PERMISSION` - Delete permissions
- `CAN_ASSIGN_PERMISSION` - Assign permissions to roles

#### Workspace Management (4 permissions)
- `CAN_CREATE_WORKSPACE` - Create workspaces
- `CAN_VIEW_WORKSPACE` - View workspaces
- `CAN_UPDATE_WORKSPACE` - Update workspaces
- `CAN_DELETE_WORKSPACE` - Delete workspaces

#### Dashboard Management (5 permissions)
- `CAN_CREATE_DASHBOARD` - Create dashboards
- `CAN_VIEW_DASHBOARD` - View dashboards
- `CAN_UPDATE_DASHBOARD` - Update dashboards
- `CAN_DELETE_DASHBOARD` - Delete dashboards
- `CAN_MANAGE_DASHBOARDS` - Full dashboard management

#### Data & Report Management (3 permissions)
- `CAN_CREATE_TRANSACTION` - Create transactions
- `CAN_VIEW_REPORT` - View reports and data
- `CAN_ANALYZE_DATA` - Run data analysis

#### Mapping Management (4 permissions)
- `CAN_CREATE_MAPPINGS` - Create field mappings
- `CAN_VIEW_MAPPINGS` - View field mappings
- `CAN_UPDATE_MAPPINGS` - Update field mappings
- `CAN_DELETE_MAPPINGS` - Delete field mappings

#### Rule Management (5 permissions)
- `CAN_CREATE_RULE` - Create rules
- `CAN_VIEW_RULE` - View rules
- `CAN_UPDATE_RULE` - Update rules
- `CAN_DELETE_RULE` - Delete rules
- `CAN_APPLY_RULE` - Apply rules to data

#### Data Ingestion Management (2 permissions)
- `CAN_MANAGE_DATA_INGESTION` - Configure data ingestion
- `CAN_VIEW_DATA_INGESTION` - View ingestion configuration

#### Notification Management (1 permission)
- `CAN_MANAGE_NOTIFICATIONS` - Manage notifications

#### Group Management (1 permission)
- `CAN_MANAGE_GROUPS` - Manage user groups

#### Fraud Detection Settings (2 permissions)
- `CAN_VIEW_FRAUD_SETTINGS` - View fraud settings
- `CAN_MANAGE_FRAUD_SETTINGS` - Manage fraud settings

### Field-Level Security

In addition to role-based permissions, the system supports **field-level security** (data locking):

- **Lock Field**: Specific field name (e.g., `StoreID`, `Region`)
- **Lock Value**: Specific value (e.g., `STORE001`, `Northeast`)

**Example**: A user with `LockField = "StoreID"` and `LockValue = "STORE001"` can only see data where `StoreID = "STORE001"`.

This ensures users only access data relevant to their location or region.

---

## Login and Authentication

### Logging In

1. Navigate to the application URL
2. Enter your **Username**
3. Enter your **Password**
4. Click **Login**

Upon successful login, you'll be redirected to the Home page.

### Session Management

- **Session Duration**: 1 hour (configurable by administrator)
- **Auto-Logout**: You'll be automatically logged out when your session expires
- **Warning**: You'll receive a warning 5 minutes before session expiry
- **Re-authentication**: After logout, simply log in again to continue

### Forgot Password

If you forget your password:

1. Click **Forgot Password?** on the login page
2. Enter your **email address**
3. Click **Submit**
4. Check your email for a password reset link
5. Click the link (valid for 1 hour)
6. Enter your **new password**
7. Confirm the new password
8. Click **Reset Password**
9. Return to the login page and sign in

**Security Notes:**
- Reset links expire after 1 hour
- Links are single-use only
- You cannot reuse your previous 5 passwords

### Changing Your Password

To change your password while logged in:

1. Navigate to **Manage → Users**
2. Find your username in the list
3. Click **Edit**
4. Enter your **current password**
5. Enter your **new password** (must meet requirements)
6. Confirm the new password
7. Click **Save**

### Logout

To log out:

1. Click your **username** in the top-right corner
2. Select **Logout**
3. You'll be redirected to the login page

---

## Dashboard Overview

### Home Page

After logging in, you'll see the Home page, which provides:

- **Quick Stats**: Key metrics and statistics
- **Recent Activity**: Your recent queries and dashboards
- **Notifications**: Important alerts and messages
- **Shortcuts**: Quick access to common tasks

### Main Navigation

The navigation bar provides access to major features:

- **Home**: Return to the dashboard overview
- **Workspaces**: Organize and manage your work
- **Query**: Build and execute reports
- **Dashboards**: View and create visual dashboards
- **Manage**: Administrative functions (if authorized)

### Notification Center

Click the **bell icon** in the top-right to view notifications:

- **Unread notifications** appear in bold
- Click a notification to view details
- Click **Mark as Read** to clear notifications
- Filter by type: Alerts, Messages, System

### User Menu

Click your **username** in the top-right corner for options:

- **Profile**: View and edit your profile
- **Settings**: Personal preferences
- **Help**: Access documentation
- **Logout**: Sign out of the system

---

## Report Builder (Query Builder)

The Report Builder (Query Builder) is a powerful tool for creating custom reports and analyzing data.

### Accessing the Query Builder

1. Click **Query** in the main navigation
2. You'll see the Query Builder interface with tabs for multiple queries

### Query Builder Interface

#### Tabs System

- **Multiple Tabs**: Work on multiple queries simultaneously
- **Tab Actions**:
  - **+ New Tab**: Create a new query
  - **Tab Name**: Click to rename
  - **X**: Close tab (with unsaved changes warning)

#### Query Builder Sections

1. **Field Selection**
   - Choose fields to include in your report
   - Available fields load from your data schema

2. **Conditions**
   - Add filters to narrow down results
   - Support for AND/OR logic
   - Multiple condition types: equals, contains, greater than, less than, etc.

3. **Grouping**
   - Group results by one or more fields
   - Useful for aggregations and summaries

4. **Sorting**
   - Sort by any field
   - Ascending or descending order
   - Multiple sort levels supported

5. **Expressions**
   - Create calculated fields
   - Use MongoDB aggregation expressions
   - Examples: `$sum`, `$avg`, `$concat`, date calculations

6. **Conditional Formatting**
   - Apply color coding based on values
   - Highlight anomalies and patterns
   - Configure rules with conditions and colors

### Building Your First Query

#### Step 1: Select Fields

1. In the **Select Fields** section, click **Add Field**
2. Choose fields from the dropdown:
   - `TransactionID`
   - `Amount`
   - `Date`
   - `StoreID`
   - `EmployeeID`
3. Click **Add** for each field

#### Step 2: Add Conditions

1. Click **Add Condition** in the Conditions section
2. Select a field (e.g., `Amount`)
3. Choose an operator (e.g., `Greater Than`)
4. Enter a value (e.g., `1000`)
5. Add more conditions as needed
6. Use **AND**/**OR** logic to combine conditions

**Example Condition:**
```
Amount > 1000 AND StoreID = "STORE001"
```

#### Step 3: Add Grouping (Optional)

1. Click **Add Grouping**
2. Select field to group by (e.g., `StoreID`)
3. Choose aggregation type (e.g., `Sum`, `Average`, `Count`)
4. Select field to aggregate (e.g., `Amount`)

#### Step 4: Add Sorting

1. Click **Add Sort**
2. Select field (e.g., `Amount`)
3. Choose direction (Ascending/Descending)

#### Step 5: Execute Query

1. Click **Run Query** button
2. Wait for results to load
3. View results in the data grid below

### Query Results Grid

The results grid displays your data with powerful features:

#### Grid Features

- **Pagination**: Navigate through large result sets
- **Column Sorting**: Click column headers to sort
- **Column Filtering**: Use filters at the top of each column
- **Column Resizing**: Drag column borders to resize
- **Column Reordering**: Drag columns to reorder
- **Cell Selection**: Click cells to select data

#### Grid Actions

- **Export**: Export results to CSV, Excel, or PDF
- **Distance Analysis**: Find similar records (see below)
- **Heatmap**: Generate heatmap visualization
- **Copy**: Copy selected cells to clipboard

### Saving Queries

1. Click **Save Query** button
2. Enter a **Query Name**
3. Choose a **Workspace** (or create new)
4. Add optional **Description**
5. Click **Save**

Saved queries appear in your Workspace for easy access.

### Loading Saved Queries

1. Navigate to **Workspaces**
2. Select a workspace
3. Click on a saved query name
4. Query opens in a new tab

### Query Actions

#### Copy Query
1. Click **Copy Query** button
2. Creates duplicate in a new tab
3. Modify without affecting original

#### Export Query Definition
1. Click **Export Definition**
2. Saves query as JSON file
3. Import later or share with others

#### Delete Query
1. Navigate to Workspaces
2. Find the query
3. Click **Delete** icon
4. Confirm deletion

### Advanced Query Features

#### Using Expressions

Expressions allow you to create calculated fields:

**Example 1: Calculate Tax**
```json
{
  "$multiply": ["$Amount", 0.08]
}
```

**Example 2: Full Name**
```json
{
  "$concat": ["$FirstName", " ", "$LastName"]
}
```

**Example 3: Date Formatting**
```json
{
  "$dateToString": {
    "format": "%Y-%m-%d",
    "date": "$TransactionDate"
  }
}
```

#### Conditional Formatting Rules

Create visual indicators based on data values:

**Example 1: Highlight High Amounts**
- **Condition**: `Amount > 5000`
- **Background Color**: Red
- **Text Color**: White

**Example 2: Color Code Status**
- **Condition**: `Status = "Approved"` → Green
- **Condition**: `Status = "Pending"` → Yellow
- **Condition**: `Status = "Rejected"` → Red

#### Adding Conditional Formatting

1. Click **Add Formatting Rule**
2. Enter **Rule Name**
3. Select **Field** to evaluate
4. Choose **Operator** (equals, greater than, etc.)
5. Enter **Value**
6. Choose **Background Color**
7. Choose **Text Color**
8. Click **Add Rule**

---

## AI-Powered Natural Language Queries

The system includes AI-powered query generation using natural language.

### What is Natural Language Query?

Instead of building queries manually, you can describe what you want in plain English, and the AI generates the query for you.

**Example:**
- Input: "Show me all transactions over $1000 from last month"
- AI generates: MongoDB query with appropriate conditions

### Requirements

- Your administrator must have configured the AI integration
- AI service (Ollama with Qwen 2.5 model) must be running
- You need `CAN_VIEW_REPORT` permission

### Using Natural Language Queries

#### Step 1: Open Natural Language Interface

1. In the Query Builder, click **AI Query** button
2. The Natural Language Query dialog appears

#### Step 2: Describe Your Query

Enter a natural language description in the text box:

**Examples:**
- "Show all refunds greater than $500 in the last 30 days"
- "Find transactions with voids exceeding 10 items grouped by employee"
- "List high-value purchases over $2000 from Store 001"
- "Show me suspicious transactions with unusual discounts"
- "Get daily transaction counts for last week"

#### Step 3: Generate Query

1. Click **Generate Query**
2. Wait for AI to process (5-10 seconds)
3. Review the generated MongoDB query

#### Step 4: Review and Execute

1. The AI-generated query appears in the query builder
2. Review the query logic
3. Make any adjustments if needed
4. Click **Run Query** to execute

### AI Query Tips

#### DO:
✅ Be specific about what you want to see  
✅ Include time ranges (last 30 days, this month, etc.)  
✅ Mention specific thresholds (greater than $500)  
✅ Specify fields to group by or sort by  
✅ Use common terms (refunds, voids, discounts)  

#### DON'T:
❌ Use overly complex sentences  
❌ Combine too many conditions in one request  
❌ Use ambiguous terms  
❌ Expect 100% accuracy (always review generated queries)  

### AI Mode Options

- **Fast Mode**: Optimized for quick responses (may be less accurate)
- **Accurate Mode**: Slower but more precise query generation

### Troubleshooting AI Queries

**Problem**: "AI service unavailable"
- **Solution**: Contact your administrator to verify AI service is running

**Problem**: Generated query doesn't match expectations
- **Solution**: Try rephrasing your request or use manual query builder

**Problem**: Query generates error
- **Solution**: Review generated query, check field names, adjust conditions

---

## Fraud Detection Features

The Loss Prevention Tool includes sophisticated fraud detection capabilities.

### What is Fraud Detection?

Fraud detection uses statistical analysis to identify suspicious patterns in transaction data:

- **High-value transactions**: Unusually large amounts
- **Refund fraud**: Excessive returns
- **Void fraud**: Suspicious voids
- **Discount abuse**: Unauthorized discounts
- **Low-value padding**: Artificially inflated transaction counts

### Accessing Fraud Detection Settings

1. Navigate to **Query Builder**
2. Click **Fraud Settings** button in toolbar
3. Fraud Detection Settings dialog appears

*Note: Requires `CAN_VIEW_FRAUD_SETTINGS` or `CAN_MANAGE_FRAUD_SETTINGS` permission*

### Understanding Fraud Thresholds

Fraud detection uses **percentile-based thresholds** and **statistical measures**:

#### Common Threshold Types

1. **Percentile**: Top/bottom X% of values
   - Example: Top 5% (95th percentile)
   
2. **Standard Deviation Multiplier**: Values X standard deviations from mean
   - Example: 2+ standard deviations above average

3. **Minimum Value**: Absolute minimum to trigger alert
   - Example: Minimum $500

4. **Mean Multiplier**: Values X times the average
   - Example: 1.5x the mean

### Fraud Detection Categories

#### 1. High Value Transactions

**Detects**: Unusually large transactions indicating:
- Major theft
- Unauthorized purchases
- Data entry errors

**Default Settings:**
- Percentile: 95th (top 5%)
- Standard Deviation: 2+
- Minimum Value: $500

**Example**: Transactions over $500 that are in the top 5% and 2+ standard deviations above mean.

#### 2. Extreme High Value

**Detects**: Extreme transactions suggesting:
- Major theft
- Money laundering
- Fraudulent activity

**Default Settings:**
- Percentile: 99th (top 1%)
- Multiplier: 1.3x the 95th percentile
- Minimum Value: $1000

#### 3. Low Value Padding

**Detects**: Suspiciously low transactions:
- Transaction padding schemes
- Employees appearing busy while stealing

**Default Settings:**
- Percentile: 5th (bottom 5%)
- Mean Multiplier: < 0.1 (less than 10% of average)
- Min Multiplier: 2x minimum

#### 4. Refund Amount Fraud

**Detects**: High refund amounts:
- Return fraud
- Receipt fraud
- Fake refunds to steal cash

**Default Settings:**
- Percentile: 95th percentile

#### 5. Refund Count Fraud

**Detects**: Excessive refund counts:
- Organized return fraud
- Employee theft schemes

**Default Settings:**
- Percentile: 95th percentile

#### 6. Void Amount Fraud

**Detects**: High void amounts:
- Voiding items after payment to pocket difference

**Default Settings:**
- Percentile: 95th percentile

#### 7. Void Count Fraud

**Detects**: Excessive voids:
- Scanning fraud
- Transaction manipulation

**Default Settings:**
- Percentile: 90th percentile

#### 8. Discount Amount Abuse

**Detects**: Large discounts:
- Unauthorized discounts
- Employee theft
- Discount abuse

**Default Settings:**
- Percentile: 90th percentile
- Mean Multiplier: 1.5x average

#### 9. Discount Count (Sweethearting)

**Detects**: Frequent discounts:
- Employees giving unauthorized discounts to friends/family

**Default Settings:**
- Percentile: 90th percentile
- Minimum: 1 discount

#### 10. Manual Overrides

**Detects**: Excessive manual overrides:
- Price manipulation
- Unauthorized adjustments

**Default Settings:**
- Percentile: 90th percentile

#### 11. Price Overrides

**Detects**: Price changes:
- Employees manually changing prices

**Default Settings:**
- Percentile: 85th percentile

#### 12. Item Quantity Anomalies

**Detects**: Unusual quantities:
- Bulk purchases
- Scanning fraud (scan 1, take multiple)

**Default Settings:**
- Percentile: 95th percentile
- Multiplier: 1.5x threshold

#### 13. Gift Card Amount

**Detects**: High gift card amounts:
- Gift card fraud
- Money laundering

**Default Settings:**
- Percentile: 95th percentile

#### 14. Gift Card Count

**Detects**: Multiple gift cards:
- Fraud rings
- Money laundering

**Default Settings:**
- Percentile: 90th percentile
- Minimum: 1 card

#### 15. Payment Methods

**Detects**: Multiple payment methods:
- Split tender fraud
- Card testing

**Default Settings:**
- Percentile: 95th percentile
- Minimum: 1 method

### Configuring Fraud Detection

*Note: Requires `CAN_MANAGE_FRAUD_SETTINGS` permission*

#### Step 1: Open Settings

1. Click **Fraud Settings** in Query Builder toolbar
2. Review current thresholds

#### Step 2: Adjust Thresholds

For each fraud type:

1. **Enable/Disable**: Toggle detection on/off
2. **Percentile**: Adjust sensitivity (higher = fewer alerts)
3. **Multipliers**: Adjust statistical thresholds
4. **Minimum Values**: Set absolute minimums

#### Step 3: Save Configuration

1. Click **Save Settings**
2. Settings apply immediately to new queries

### Running Fraud Detection Queries

#### Option 1: Manual Query

1. Open Query Builder
2. Select relevant fields (Amount, RefundCount, etc.)
3. Add conditions based on fraud thresholds
4. Execute query

#### Option 2: Pre-built Fraud Queries

Many organizations create saved queries for common fraud types:

1. Navigate to **Workspaces**
2. Look for "Fraud Detection" workspace
3. Run pre-configured fraud queries

### Interpreting Fraud Results

When reviewing fraud detection results:

1. **High Priority**: Multiple fraud indicators present
2. **Medium Priority**: One or two indicators
3. **Low Priority**: Borderline cases

**Always investigate context:**
- Is this a known high-value customer?
- Is there a valid business reason (bulk order, special event)?
- Does the employee have authorization?
- Is this part of a pattern?

### Best Practices for Fraud Detection

1. **Baseline First**: Run reports for 30-90 days to establish normal patterns
2. **Adjust Thresholds**: Fine-tune based on your business
3. **Regular Reviews**: Check fraud reports daily or weekly
4. **Investigate Patterns**: Look for repeated behavior, not isolated incidents
5. **Document Findings**: Keep records of investigations
6. **Update Settings**: Adjust as your business changes

---

## Euclidean Distance Analysis

Euclidean Distance Analysis finds records similar to a selected record based on field values.

### What is Distance Analysis?

Distance analysis calculates similarity between records using:
- Numeric values (amounts, counts)
- Date/time values
- Categorical values (status, type)

**Use Cases:**
- **Find fraud patterns**: Compare suspicious transaction to find similar ones
- **Detect anomalies**: Identify unusual records
- **Find duplicates**: Locate potential duplicate records
- **Pattern analysis**: Group similar transactions

### How It Works

The system calculates "distance" between your selected record and all other records:

- **0% similarity**: Completely different
- **50-69%**: Moderately similar
- **70-89%**: Very similar
- **90-100%**: Nearly identical (potential duplicate)

### Running Distance Analysis

#### Step 1: Execute a Query

1. Open Query Builder
2. Build and run a query
3. Results appear in the grid

#### Step 2: Select Fields from a Record

In the results grid:

1. Click on field values from **ONE** row
2. Click at least **3 fields** (5-10 recommended)
3. Include the **_id** field
4. Include **numeric fields** (Amount, Count)
5. Include **date fields** when relevant

**Tip**: Selected fields appear highlighted

#### Step 3: Open Distance Analysis

1. Click **Euclidean Distance** button in toolbar
2. Distance Analysis dialog opens

#### Step 4: Configure Analysis

In the dialog:

**Required Fields:**
- **Start Date Field**: Field containing start date
- **End Date Field**: Field containing end date
- **Date Range**: From/To dates for comparison
- **Legend Label Field**: Field to identify records (e.g., TransactionID, CustomerName)

**Optional:**
- **Max Results**: Limit number of similar records returned (default: 100)

#### Step 5: Run Analysis

1. Click **Run Distance Analysis**
2. Wait for processing (5-30 seconds depending on data volume)
3. Results appear in two views:
   - **Radar Chart**: Visual comparison
   - **Results Table**: Detailed similarity scores

### Understanding Distance Results

#### Results Table Columns

| Column | Description | Interpretation |
|--------|-------------|----------------|
| **Rank** | Similarity ranking | 1 = most similar |
| **Overall Score** | Combined similarity | 90-100% = nearly identical |
| **Field Match %** | % of selected fields present | Low % = many missing fields |
| **Distance Match %** | How close actual values are | 100% = identical values |
| **Legend Label** | Record identifier | Helps identify the record |
| **Record Details** | Individual field comparisons | See specific differences |

#### Score Interpretation

**Overall Score Guide:**

- **90-100%**: Nearly identical
  - **Action**: Investigate as potential duplicate
  - **Use Case**: Finding exact matches

- **70-89%**: Very similar
  - **Action**: Strong pattern match
  - **Use Case**: Finding fraud patterns

- **50-69%**: Moderately similar
  - **Action**: Loose pattern match
  - **Use Case**: Exploratory analysis

- **Below 50%**: Not very similar
  - **Action**: Likely unrelated
  - **Use Case**: Establishing what's different

#### Radar Chart

The radar chart visualizes multi-dimensional comparison:

- **Axes**: Each selected field
- **Original Record**: Blue line
- **Similar Record**: Red line
- **Overlap**: More overlap = more similar

### Distance Analysis Tips

#### DO:
✅ Select 5-10 relevant fields  
✅ Include mix of numeric and categorical fields  
✅ Use fields that exist in most records  
✅ Choose meaningful date ranges  
✅ Select good identifier field (TransactionID, CustomerName)  

#### DON'T:
❌ Select fewer than 3 fields  
❌ Include only unique fields (secondary IDs)  
❌ Use extremely narrow date ranges  
❌ Select fields with mostly null values  
❌ Include too many fields (slows processing)  

### Common Distance Analysis Scenarios

#### Scenario 1: Fraud Pattern Detection

**Goal**: Find transactions similar to known fraudulent transaction

**Steps:**
1. Query transactions from last 6 months
2. Select fraudulent transaction fields:
   - Amount
   - StoreID
   - EmployeeID
   - Time
   - PaymentMethod
3. Run distance analysis with 90-day range
4. Review top 20 most similar records
5. Investigate matches with 70%+ similarity

#### Scenario 2: Anomaly Detection

**Goal**: Determine if unusual record is truly unique

**Steps:**
1. Query all records from relevant period
2. Select unusual record fields
3. Run distance analysis
4. If all scores < 50%, record is genuinely unusual
5. If scores > 70%, record fits a pattern

#### Scenario 3: Duplicate Detection

**Goal**: Find potential duplicate entries

**Steps:**
1. Query records to check for duplicates
2. Select key identifying fields (exclude auto-generated IDs)
3. Run distance analysis
4. Investigate records with 90%+ similarity
5. Verify and merge/delete duplicates

### Troubleshooting Distance Analysis

**Problem**: "Must select at least 3 fields"
- **Solution**: Click more field values in results grid

**Problem**: "No similar records found"
- **Solution**: Expand date range or select more common fields

**Problem**: All scores are low
- **Solution**: This might be expected - the record is genuinely unique/unusual

**Problem**: Analysis is slow
- **Solution**: Narrow date range, reduce max results, or select fewer fields

---

## Data Export

Export query results in multiple formats for reporting and analysis.

### Export Formats

The system supports three export formats:

1. **CSV** (Comma-Separated Values)
   - Simple text format
   - Opens in Excel, Google Sheets
   - Best for: Data import, simple reports

2. **Excel** (.xlsx)
   - Microsoft Excel format
   - Preserves formatting
   - Best for: Professional reports, analysis

3. **PDF** (Portable Document Format)
   - Print-ready format
   - Preserves layout
   - Best for: Sharing, printing, archiving

### Exporting Query Results

#### Step 1: Run Your Query

1. Build and execute your query
2. Review results in the grid
3. Ensure data is correct

#### Step 2: Choose Export Format

Click the **Export** dropdown in the toolbar:

- **Export to CSV**
- **Export to Excel**
- **Export to PDF**

#### Step 3: Configure Export Options

Depending on format, you may see options:

**CSV Options:**
- Include headers (Yes/No)
- Delimiter (Comma, Tab, Semicolon)

**Excel Options:**
- Include headers
- Include formatting
- Sheet name

**PDF Options:**
- Page orientation (Portrait/Landscape)
- Page size (Letter, A4)
- Include title
- Include date

#### Step 4: Download

1. Click **Export**
2. File downloads to your browser's download folder
3. Filename format: `QueryResults_YYYY-MM-DD_HHMMSS.{format}`

### Export Best Practices

1. **Preview First**: Always review data before exporting
2. **Limit Rows**: Consider limiting large exports (paginate or filter)
3. **Choose Right Format**:
   - CSV for import to other systems
   - Excel for analysis and formatting
   - PDF for sharing and printing
4. **Meaningful Filenames**: Rename exported files descriptively
5. **Data Security**: Be cautious with exported data containing sensitive information

### Large Data Exports

For queries returning thousands of rows:

**Recommendations:**
- Use CSV format (faster, smaller file size)
- Export in batches using pagination
- Apply filters to reduce result size
- Schedule exports during off-peak hours (if available)

### Export Limitations

- **Maximum Rows**: 50,000 rows per export (configurable by administrator)
- **File Size**: Depends on data volume and format
- **Timeout**: Exports timeout after 5 minutes
- **Permissions**: Requires `CAN_VIEW_REPORT` permission

---

## Workspace Management

Workspaces organize your queries, reports, and dashboards into logical groups.

### What is a Workspace?

A workspace is a container for related work:
- **Queries**: Saved searches
- **Dashboards**: Visual reports
- **Notes**: Documentation
- **Shared Access**: Collaborate with team

**Example Workspaces:**
- Store 001 - Fraud Investigation
- Q4 2025 Analysis
- Employee Productivity Reports
- Refund Fraud Monitoring

### Viewing Workspaces

1. Click **Workspaces** in main navigation
2. See list of your workspaces
3. Click workspace name to open

### Creating a New Workspace

#### Step 1: Open Workspace Dialog

1. Navigate to **Workspaces**
2. Click **Create New Workspace** button

#### Step 2: Enter Workspace Details

- **Workspace Name**: Descriptive name (e.g., "Store 005 Fraud Analysis")
- **Description**: Optional detailed description
- **Owner**: Your username (auto-filled)
- **Created Date**: Auto-generated

#### Step 3: Save

1. Click **Create Workspace**
2. Workspace appears in your list
3. Opens empty workspace

### Managing Workspace Content

#### Adding Queries to Workspace

**Option 1: Save New Query**
1. Build query in Query Builder
2. Click **Save Query**
3. Select workspace from dropdown
4. Enter query name
5. Click **Save**

**Option 2: Move Existing Query**
1. Open workspaces view
2. Drag query from one workspace to another
3. Or right-click query → **Move to Workspace**

#### Adding Dashboards to Workspace

1. Create or edit dashboard
2. In dashboard settings, select **Workspace**
3. Save dashboard

### Organizing Workspace Content

- **Rename Items**: Click name, edit inline
- **Delete Items**: Click delete icon, confirm
- **Reorder**: Drag and drop to reorder
- **Filter**: Use search box to filter items

### Sharing Workspaces

*Note: Requires `CAN_UPDATE_WORKSPACE` permission*

#### Step 1: Open Workspace Settings

1. Open workspace
2. Click **Settings** icon
3. Select **Sharing**

#### Step 2: Add Users or Groups

1. Click **Add User/Group**
2. Search for user or group name
3. Select from dropdown
4. Choose **Permission Level**:
   - **View**: Read-only access
   - **Edit**: Can modify content
   - **Admin**: Full control including sharing

#### Step 3: Save Sharing Settings

1. Click **Save**
2. Users receive notification
3. Workspace appears in their workspace list

### Workspace Best Practices

1. **Organize by Project**: Create workspace per investigation or project
2. **Descriptive Names**: Use clear, searchable names
3. **Regular Cleanup**: Archive or delete old workspaces
4. **Document Purpose**: Use description field effectively
5. **Control Access**: Share only with necessary users

### Deleting a Workspace

*Note: Requires `CAN_DELETE_WORKSPACE` permission*

**Warning**: Deleting a workspace deletes all contained queries and dashboards!

1. Navigate to **Workspaces**
2. Find workspace to delete
3. Click **Delete** icon (trash can)
4. Confirm deletion warning
5. Workspace and all content deleted permanently

**Recommendation**: Export important queries/dashboards before deleting workspace.

---

## Dashboard Creation and Management

Dashboards provide visual, interactive displays of your data.

### What is a Dashboard?

A dashboard is a customizable visual interface containing:
- **Charts**: Bar, line, pie, scatter, etc.
- **Tables**: Tabular data displays
- **Text Blocks**: Titles, descriptions, notes
- **Images**: Logos, diagrams
- **Metrics**: KPIs and statistics

### Viewing Dashboards

1. Click **Dashboards** in main navigation
2. See list of available dashboards
3. Click dashboard name to open

### Creating a New Dashboard

#### Step 1: Start Dashboard Creation

1. Navigate to **Dashboards**
2. Click **Create New Dashboard**
3. Dashboard editor opens

#### Step 2: Configure Dashboard Settings

Click **Settings** icon:

- **Dashboard Name**: Enter descriptive name
- **Description**: Optional details
- **Workspace**: Select workspace (optional)
- **Layout**: Grid or free-form
- **Refresh Interval**: Auto-refresh frequency (optional)

#### Step 3: Add Blocks

Dashboards are composed of **blocks**. Add blocks by clicking **Add Block**:

**Block Types:**
1. **Chart Block**: Visual chart
2. **Table Block**: Data table
3. **Text Block**: Text, titles, notes
4. **Image Block**: Images, logos

### Creating Chart Blocks

#### Step 1: Add Chart Block

1. Click **Add Block** → **Chart**
2. Empty chart placeholder appears

#### Step 2: Configure Chart

Click **Edit** on the chart block:

**Data Tab:**
- **Query**: Select saved query or create new
- **Data Source**: Choose dataset
- **Fields**: Map fields to chart axes

**Chart Tab:**
- **Chart Type**: Bar, Line, Pie, Scatter, Doughnut, Radar
- **Title**: Chart title
- **Legend**: Show/hide, position
- **Colors**: Color scheme

**Options Tab:**
- **Axes Labels**: X and Y axis labels
- **Data Labels**: Show values on chart
- **Tooltips**: Hover information
- **Animation**: Enable/disable

#### Step 3: Save Chart

1. Click **Save**
2. Chart renders with live data
3. Position and resize as needed

### Chart Types and Use Cases

| Chart Type | Best For | Example Use Case |
|------------|----------|------------------|
| **Bar Chart** | Comparing categories | Sales by store |
| **Line Chart** | Trends over time | Daily transaction counts |
| **Pie Chart** | Parts of whole | Transaction types distribution |
| **Scatter Plot** | Correlations | Amount vs. Time |
| **Doughnut** | Parts of whole (alternative to pie) | Payment methods breakdown |
| **Radar Chart** | Multi-dimensional comparison | Employee performance metrics |

### Creating Table Blocks

#### Step 1: Add Table Block

1. Click **Add Block** → **Table**
2. Empty table placeholder appears

#### Step 2: Configure Table

Click **Edit** on table block:

**Data Tab:**
- **Query**: Select query
- **Columns**: Choose columns to display
- **Row Limit**: Max rows to show

**Formatting Tab:**
- **Header Style**: Font, color, size
- **Row Style**: Alternating colors
- **Conditional Formatting**: Color code based on values

**Options Tab:**
- **Pagination**: Enable/disable
- **Sorting**: Allow column sorting
- **Filtering**: Enable column filters

#### Step 3: Save Table

1. Click **Save**
2. Table renders with data
3. Adjust size and position

### Creating Text Blocks

#### Step 1: Add Text Block

1. Click **Add Block** → **Text**
2. Text editor appears

#### Step 2: Enter Content

- **Title**: Optional title
- **Content**: Enter text, markdown supported
- **Formatting**: Bold, italic, lists, links

**Supported Markdown:**
```markdown
# Heading 1
## Heading 2
**Bold text**
*Italic text*
- Bullet point
1. Numbered list
[Link text](URL)
```

#### Step 3: Style Text

- **Font Size**: Adjust size
- **Alignment**: Left, center, right
- **Colors**: Text and background colors
- **Borders**: Add borders if desired

### Creating Image Blocks

#### Step 1: Add Image Block

1. Click **Add Block** → **Image**
2. Image configuration dialog appears

#### Step 2: Add Image

**Options:**
- **Upload Image**: Upload from computer
- **Image URL**: Enter URL of online image
- **Company Logo**: Select from system logos (if configured)

#### Step 3: Configure Display

- **Alt Text**: Accessibility description
- **Size**: Width and height
- **Alignment**: Position within block

### Dashboard Layout

#### Grid Layout (Recommended)

- **Drag and Drop**: Position blocks anywhere
- **Resize**: Drag corners to resize
- **Snap to Grid**: Blocks align to grid
- **Responsive**: Adjusts to screen size

#### Positioning Blocks

1. **Move**: Click and drag block header
2. **Resize**: Drag bottom-right corner
3. **Layer**: Bring to front or send to back
4. **Delete**: Click X in block header

### Dashboard Actions

#### Save Dashboard

1. Click **Save** button (top-right)
2. Dashboard saved with all blocks
3. Success notification appears

#### Edit Mode vs. View Mode

- **Edit Mode**: Drag, resize, configure blocks
- **View Mode**: Display only, blocks locked
- Toggle with **Edit** / **View** button

#### Refresh Dashboard

- **Manual**: Click **Refresh** button
- **Auto**: Set refresh interval in settings
- Refreshes all data queries

#### Share Dashboard

1. Click **Share** button
2. Options:
   - **Share Link**: Copy URL
   - **Add Users**: Grant access to specific users
   - **Make Public**: Allow anyone to view (if enabled)

#### Export Dashboard

1. Click **Export** dropdown
2. Options:
   - **Export as PDF**: Print-ready document
   - **Export as Image**: PNG screenshot
   - **Export Configuration**: JSON file

#### Delete Dashboard

1. Click **Delete** button
2. Confirm deletion
3. Dashboard removed (can be recovered from workspace if needed)

### Dashboard Best Practices

1. **Logical Layout**: Group related information
2. **Consistent Design**: Use same color schemes
3. **Limit Blocks**: 4-8 blocks per dashboard (avoid clutter)
4. **Descriptive Titles**: Clear chart and block titles
5. **Regular Updates**: Refresh data frequently
6. **Performance**: Avoid complex queries on dashboards
7. **Mobile Friendly**: Test on different screen sizes

### Example Dashboard Layouts

#### Executive Summary Dashboard

```
┌──────────────┬──────────────┐
│  KPI Metrics │ Daily Trend  │
│  (Text)      │ (Line Chart) │
├──────────────┼──────────────┤
│ Top Stores   │ Fraud Alerts │
│ (Bar Chart)  │ (Table)      │
└──────────────┴──────────────┘
```

#### Fraud Detection Dashboard

```
┌────────────────────────────┐
│ Fraud Alert Summary (Text) │
├──────────────┬─────────────┤
│ High Value   │ Refund      │
│ Transactions │ Anomalies   │
│ (Table)      │ (Bar Chart) │
├──────────────┴─────────────┤
│ Employee Risk Scores        │
│ (Heatmap)                   │
└─────────────────────────────┘
```

---

## Admin Features

Administrative features are available to users with appropriate permissions.

### User Management

*Requires: `CAN_VIEW_USER`, `CAN_CREATE_USER`, `CAN_UPDATE_USER`, `CAN_DELETE_USER` permissions*

#### Accessing User Management

1. Click **Manage** in main navigation
2. Select **Users**
3. User list appears

#### Creating a New User

**Step 1: Open Create User Dialog**
1. Click **Create New User** button

**Step 2: Enter User Details**
- **Username**: Unique username (required)
- **Email**: Valid email address (required)
- **First Name**: User's first name
- **Last Name**: User's last name
- **Password**: Initial password (must meet requirements)
- **Confirm Password**: Re-enter password

**Step 3: Assign Roles**
- Select one or more roles from dropdown
- Common roles: Administrator, Analyst, Viewer

**Step 4: Configure Field Lock (Optional)**
- **Lock Field**: Field name (e.g., StoreID)
- **Lock Value**: Specific value (e.g., STORE001)
- Limits user to seeing only data where LockField = LockValue

**Step 5: Save User**
1. Click **Create User**
2. User receives welcome email (if SMTP configured)
3. User can now log in

#### Editing a User

1. Find user in list
2. Click **Edit** icon
3. Modify details (password, roles, field lock)
4. Click **Save**

#### Deactivating a User

Instead of deleting, deactivate users:

1. Edit user
2. Uncheck **Active** checkbox
3. Save
4. User cannot log in but data preserved

#### Deleting a User

**Warning**: Permanent action!

1. Find user in list
2. Click **Delete** icon (trash can)
3. Confirm deletion
4. User and associated data deleted

#### Password Reset for Users

As an administrator:

1. Edit user
2. Click **Reset Password**
3. Choose option:
   - **Send Reset Email**: User receives email
   - **Set New Password**: You set password directly

### Role Management

*Requires: Role management permissions*

#### Accessing Role Management

1. Navigate to **Manage → Roles**
2. List of roles appears

#### Creating a New Role

**Step 1: Create Role**
1. Click **Create New Role**
2. Enter **Role Name** (e.g., "Store Manager")
3. Enter **Description** (e.g., "Managers of individual stores")

**Step 2: Assign Permissions**
1. Permission list appears
2. Check permissions to include:
   - User permissions
   - Role permissions
   - Workspace permissions
   - Dashboard permissions
   - Data permissions
   - Mapping permissions
   - Rule permissions
   - Ingestion permissions
   - Notification permissions
   - Group permissions
   - Fraud settings permissions

**Common Permission Sets:**

**Store Manager:**
- CAN_VIEW_DASHBOARD
- CAN_CREATE_DASHBOARD
- CAN_UPDATE_DASHBOARD
- CAN_VIEW_REPORT
- CAN_VIEW_WORKSPACE
- CAN_CREATE_WORKSPACE
- CAN_UPDATE_WORKSPACE

**Analyst:**
- CAN_VIEW_REPORT
- CAN_ANALYZE_DATA
- CAN_VIEW_DASHBOARD
- CAN_CREATE_DASHBOARD
- CAN_UPDATE_DASHBOARD
- CAN_VIEW_FRAUD_SETTINGS
- CAN_VIEW_WORKSPACE
- CAN_CREATE_WORKSPACE

**Administrator:**
- All permissions (41 permissions)

**Step 3: Save Role**
1. Click **Save**
2. Role available for assignment

#### Editing a Role

1. Find role in list
2. Click **Edit**
3. Modify name, description, or permissions
4. Click **Save**

#### Deleting a Role

**Warning**: Users assigned to this role will lose associated permissions!

1. Find role in list
2. Click **Delete**
3. Confirm deletion
4. Reassign users from deleted role

### Permission Management

*Requires: Permission management permissions*

#### Viewing Permissions

1. Navigate to **Manage → Permissions**
2. See list of all 41 permissions
3. View details: Code, Name, Description

#### Permission Reference

See [User Roles and Permissions](#user-roles-and-permissions) section for complete list.

### Group Management

*Requires: `CAN_MANAGE_GROUPS` permission*

Groups organize users for easier permission and notification management.

#### Creating a Group

**Step 1: Open Group Dialog**
1. Navigate to **Manage → Groups**
2. Click **Create New Group**

**Step 2: Enter Group Details**
- **Group Name**: Descriptive name (e.g., "Store Managers - Region 1")
- **Description**: Optional details
- **Group Type**: 
  - Security Group (for permissions)
  - Notification Group (for alerts)
  - Both

**Step 3: Add Members**
1. Click **Add Members**
2. Search for users
3. Select users to add
4. Click **Add**

**Step 4: Save Group**
1. Click **Save**
2. Group created

#### Using Groups

**Assigning Permissions:**
- Instead of assigning role to each user individually
- Assign role to group
- All members inherit permissions

**Sending Notifications:**
- Send notification to group
- All members receive notification

### Mapping Configuration

*Requires: Mapping management permissions*

Mappings define how incoming data fields map to system fields.

#### Accessing Mappings

1. Navigate to **Manage → Mappings**
2. List of mappings appears

#### Creating a New Mapping

**Step 1: Create Mapping**
1. Click **Create New Mapping**
2. Enter **Mapping Name** (e.g., "Store POS System XML")

**Step 2: Define Source and Target**
- **Source Type**: XML, CSV, JSON
- **Source Field**: Field name in source data
- **Target Field**: Field name in system
- **Data Type**: String, Number, Date, Boolean
- **Transform**: Optional transformation (uppercase, trim, etc.)

**Example Mapping:**

| Source Field | Target Field | Data Type | Transform |
|--------------|--------------|-----------|-----------|
| `TXN_ID` | `TransactionID` | String | Trim |
| `TXN_AMT` | `Amount` | Number | None |
| `TXN_DATE` | `Date` | Date | Parse ISO |
| `STORE` | `StoreID` | String | Uppercase |

**Step 3: Test Mapping**
1. Upload sample file
2. Click **Test Mapping**
3. Review mapped output
4. Adjust as needed

**Step 4: Save Mapping**
1. Click **Save**
2. Mapping available for data ingestion

#### Editing a Mapping

1. Find mapping in list
2. Click **Edit**
3. Modify field mappings
4. Save changes

#### Deleting a Mapping

1. Find mapping in list
2. Click **Delete**
3. Confirm deletion

### Rules Configuration

*Requires: Rule management permissions*

Rules automatically enrich or transform data during ingestion.

#### Accessing Rules

1. Navigate to **Manage → Rules**
2. List of rules appears

#### Creating a New Rule

**Step 1: Create Rule**
1. Click **Create New Rule**
2. Enter **Rule Name** (e.g., "Calculate Tax")

**Step 2: Define Rule Logic**

**Rule Types:**
1. **Enrichment**: Add calculated fields
2. **Validation**: Check data quality
3. **Transformation**: Modify existing fields
4. **Routing**: Conditional data routing

**Example Rule - Calculate Tax:**
```json
{
  "name": "CalculateTax",
  "type": "enrichment",
  "condition": "Amount > 0",
  "action": {
    "field": "TaxAmount",
    "expression": "Amount * 0.08"
  }
}
```

**Step 3: Set Execution Order**
- Rules execute in order
- Drag to reorder rules

**Step 4: Save Rule**
1. Click **Save**
2. Rule applies to new data ingestion

### Data Ingestion Setup

*Requires: Data ingestion management permissions*

Configure automated data imports from external sources.

#### Accessing Data Ingestion

1. Navigate to **Manage → Data Ingestion**
2. List of ingestion jobs appears

#### Creating a Data Ingestion Job

**Step 1: Create Job**
1. Click **Create New Job**
2. Enter **Job Name** (e.g., "Daily POS Import")

**Step 2: Configure Source**

**Source Types:**
- **SFTP**: Secure file transfer
- **File System**: Local/network folder
- **HTTP**: Download from URL
- **Database**: Direct database connection (if configured)

**SFTP Configuration:**
- Host: `ftp.yourcompany.com`
- Port: `22`
- Username: `import_user`
- Password: `********`
- Directory: `/exports/daily`
- File Pattern: `*.xml`

**Step 3: Select Mapping**
- Choose mapping configuration
- Maps source fields to system fields

**Step 4: Select Rules**
- Choose rules to apply
- Rules execute during import

**Step 5: Configure Schedule**

**Schedule Options:**
- **Manual**: Run on demand only
- **Daily**: Specify time (e.g., 2:00 AM)
- **Weekly**: Choose day and time
- **Custom**: Cron expression

**Cron Examples:**
- `0 2 * * *` - Daily at 2:00 AM
- `0 2 * * 1` - Weekly on Monday at 2:00 AM
- `0 */4 * * *` - Every 4 hours

**Step 6: Error Handling**
- **On Error**: Continue / Stop / Retry
- **Retry Count**: Max retry attempts
- **Notification**: Send alert on failure

**Step 7: Save Job**
1. Click **Save**
2. Job created and scheduled

#### Running a Job Manually

1. Find job in list
2. Click **Run Now** button
3. Job executes immediately
4. View progress in job log

#### Monitoring Jobs

1. Job list shows status:
   - **Scheduled**: Waiting for next run
   - **Running**: Currently executing
   - **Success**: Last run successful
   - **Failed**: Last run failed
2. Click job name to view details
3. View logs for troubleshooting

### Notification Management

*Requires: `CAN_MANAGE_NOTIFICATIONS` permission*

#### Creating Notifications

**Step 1: Create Notification**
1. Navigate to **Manage → Notifications**
2. Click **Create Notification**

**Step 2: Configure Notification**
- **Title**: Notification title
- **Message**: Notification content
- **Type**: Info, Warning, Error, Success
- **Recipients**:
  - All Users
  - Specific Users
  - Groups
  - Roles

**Step 3: Delivery Options**
- **In-App**: Show in notification center
- **Email**: Send email notification
- **Priority**: Low, Normal, High, Urgent

**Step 4: Send**
1. Click **Send**
2. Notification delivered

#### Viewing Notification History

1. Navigate to **Manage → Notifications**
2. Switch to **History** tab
3. See all past notifications
4. Filter by date, type, or recipient

---

## Common Workflows

### Workflow 1: Daily Fraud Monitoring

**Frequency**: Daily (morning)  
**Duration**: 15-30 minutes

**Steps:**

1. **Login** to system
2. **Navigate** to Workspaces → "Fraud Detection"
3. **Run saved queries**:
   - High Value Transactions
   - Excessive Refunds
   - Void Fraud
   - Discount Abuse
4. **Review results**:
   - Sort by severity
   - Check for patterns
   - Note unusual employees/stores
5. **Export findings**:
   - Export to Excel for detailed analysis
   - Flag items for investigation
6. **Create follow-up tasks**:
   - Add notes to workspace
   - Send notifications to managers
7. **Update dashboard**:
   - Update "Daily Fraud Summary" dashboard
   - Share with management

### Workflow 2: Monthly Store Performance Analysis

**Frequency**: Monthly  
**Duration**: 1-2 hours

**Steps:**

1. **Create new workspace**: "Store Performance - [Month] [Year]"
2. **Build queries**:
   - Total sales by store
   - Transaction counts
   - Average transaction value
   - Top-selling items
   - Employee productivity
3. **Create dashboard**:
   - Add bar chart for sales by store
   - Add line chart for daily trends
   - Add table for top performers
   - Add KPI metrics (total sales, growth %)
4. **Run distance analysis**:
   - Compare current month to previous month
   - Identify stores with unusual patterns
5. **Generate report**:
   - Export dashboard to PDF
   - Export detailed data to Excel
6. **Share with stakeholders**:
   - Email PDF report
   - Grant dashboard access to managers

### Workflow 3: Investigating a Fraud Alert

**Trigger**: Fraud alert notification  
**Duration**: 30 minutes - 2 hours

**Steps:**

1. **Review alert**:
   - Read notification details
   - Note employee, store, transaction details
2. **Open Query Builder**:
   - Build query to retrieve flagged transaction
   - Include all relevant fields
3. **Run Distance Analysis**:
   - Select transaction fields
   - Find similar transactions
   - Look for patterns
4. **Broaden investigation**:
   - Query all transactions for employee in past 90 days
   - Look for repeated behavior
   - Compare to peer employees
5. **Document findings**:
   - Create workspace for investigation
   - Save all queries
   - Add notes and observations
6. **Generate evidence report**:
   - Export transactions to PDF
   - Include distance analysis results
   - Export supporting data to Excel
7. **Take action**:
   - Send notification to management
   - Flag employee in system
   - Update case status

### Workflow 4: Setting Up a New Store

**Trigger**: New store opening  
**Duration**: 1-2 hours

**Steps:**

1. **Create store configuration**:
   - Add store to system (if manual entry required)
   - Configure mappings for new POS system (if different)
2. **Create store users**:
   - Create accounts for store managers
   - Assign "Store Manager" role
   - Set field lock: `StoreID = "NEW_STORE_ID"`
3. **Create store workspace**:
   - Create workspace for new store
   - Pre-populate with standard queries
4. **Create store dashboard**:
   - Clone "Store Template" dashboard
   - Customize for new store
   - Share with store manager
5. **Configure data ingestion**:
   - Set up SFTP job for store data
   - Test ingestion with sample data
   - Schedule daily imports
6. **Train store manager**:
   - Provide login credentials
   - Walk through dashboard
   - Demonstrate key queries
7. **Monitor initial period**:
   - Check data ingestion daily for first week
   - Verify fraud detection is working
   - Address any issues

### Workflow 5: Quarter-End Reporting

**Frequency**: Quarterly  
**Duration**: 4-8 hours

**Steps:**

1. **Create workspace**: "Q[X] [Year] Report"
2. **Run comprehensive queries**:
   - Total transactions
   - Revenue by store, region, category
   - Fraud losses
   - Top performers
   - Worst performers
   - Year-over-year comparisons
3. **Generate visualizations**:
   - Create executive dashboard
   - Charts for each key metric
   - Trend analysis
4. **Run advanced analytics**:
   - Distance analysis on best/worst stores
   - Fraud pattern analysis
   - Employee risk scoring
5. **Compile report**:
   - Export dashboards to PDF
   - Export data tables to Excel
   - Create PowerPoint presentation (external)
6. **Review and QA**:
   - Verify all numbers
   - Cross-check with source systems
   - Peer review
7. **Distribute**:
   - Send to executive team
   - Present at quarterly review meeting
   - Archive in workspace

---

## Tips and Best Practices

### General Usage Tips

#### Performance Optimization

1. **Limit query results**: Use date ranges and filters to reduce data volume
2. **Index important fields**: Request administrator to add indexes for frequently queried fields
3. **Avoid SELECT ***: Only select fields you need
4. **Use pagination**: For large result sets, paginate rather than loading all at once
5. **Close unused tabs**: Close query tabs you're not actively using

#### Query Building Tips

1. **Start simple**: Begin with basic query, add complexity gradually
2. **Test incrementally**: Run query after each change to verify results
3. **Use descriptive names**: Name saved queries clearly
4. **Document complex queries**: Add notes explaining query logic
5. **Save frequently**: Don't lose work - save often

#### Dashboard Design Tips

1. **Less is more**: Don't overcrowd dashboards
2. **Consistent colors**: Use same color scheme throughout
3. **Logical layout**: Group related information
4. **Meaningful titles**: Clear, descriptive titles
5. **Regular refresh**: Set appropriate auto-refresh intervals

### Security Best Practices

1. **Strong passwords**: Use complex, unique passwords
2. **Log out when done**: Don't leave sessions open
3. **Secure exports**: Protect exported files containing sensitive data
4. **Report suspicious activity**: Alert administrator immediately
5. **Review permissions**: Periodically verify your access is appropriate

### Data Analysis Best Practices

1. **Understand your data**: Know what fields mean before analyzing
2. **Verify results**: Cross-check unexpected findings
3. **Consider context**: Don't jump to conclusions
4. **Document methodology**: Record how you derived conclusions
5. **Peer review**: Have colleagues review important findings

### Collaboration Best Practices

1. **Use workspaces**: Organize work by project
2. **Share knowledge**: Document processes for colleagues
3. **Clear naming**: Use consistent naming conventions
4. **Communication**: Use notifications for important findings
5. **Respect permissions**: Don't share data beyond your authorization

### Fraud Detection Best Practices

1. **Establish baselines**: Understand normal patterns before identifying fraud
2. **Multiple indicators**: Look for multiple fraud indicators, not just one
3. **Investigation before accusation**: Gather evidence thoroughly
4. **Pattern recognition**: Look for repeated behavior
5. **Document everything**: Maintain detailed investigation records
6. **Timely action**: Investigate alerts promptly
7. **Continuous improvement**: Refine fraud thresholds based on findings

---

## FAQs

### General Questions

**Q: What browsers are supported?**  
A: Chrome (recommended), Edge, Firefox, and Safari (version 90+ for all).

**Q: Can I use the system on mobile devices?**  
A: The system is optimized for desktop. Mobile viewing is possible but limited.

**Q: How long do sessions last?**  
A: Default is 1 hour. You'll be auto-logged out after expiration.

**Q: Can I have multiple tabs open?**  
A: Yes, the Query Builder supports multiple query tabs.

**Q: Is there a limit to how many queries I can save?**  
A: No hard limit, but keep workspaces organized.

### Login and Authentication

**Q: I forgot my password. What do I do?**  
A: Click "Forgot Password" on login page and follow email instructions.

**Q: My password reset link expired. What now?**  
A: Request a new reset link. Links expire after 1 hour.

**Q: Why can't I log in?**  
A: Possible reasons:
- Incorrect username/password
- Account deactivated
- Account locked (too many failed attempts - wait 15 minutes)
- System maintenance

**Q: How do I change my password?**  
A: Navigate to Manage → Users, edit your account, and change password.

### Permissions and Access

**Q: Why can't I see certain menu items?**  
A: You don't have required permissions. Contact your administrator.

**Q: Why can't I see all data?**  
A: You may have field-level security (data lock) applied. Contact administrator.

**Q: How do I request additional permissions?**  
A: Contact your administrator or manager.

**Q: Can I see who has access to my workspaces?**  
A: Yes, open workspace settings → Sharing tab.

### Queries and Reports

**Q: Why is my query slow?**  
A: Possible reasons:
- Large date range
- No filters applied
- Complex conditions
- Many joins/aggregations
- Database performance issues

**Q: What's the maximum number of rows I can query?**  
A: Default is 50,000 rows. Contact administrator if you need more.

**Q: Can I schedule queries to run automatically?**  
A: Not directly, but administrators can set up scheduled data exports.

**Q: Why do my query results look different than yesterday?**  
A: Data may have been updated, deleted, or new data added.

**Q: Can I query data from external systems?**  
A: Only if data has been ingested into the Loss Prevention database.

### Dashboards

**Q: Can I embed dashboards in other applications?**  
A: This depends on your system configuration. Contact administrator.

**Q: Why isn't my dashboard refreshing?**  
A: Check refresh settings. Ensure auto-refresh is enabled.

**Q: Can I share dashboards with users outside my organization?**  
A: No, dashboards are internal only for security.

**Q: How many blocks can I add to a dashboard?**  
A: No hard limit, but 4-8 blocks recommended for performance.

### Data Export

**Q: What export formats are available?**  
A: CSV, Excel (.xlsx), and PDF.

**Q: Why is my export taking so long?**  
A: Large datasets take time. Consider exporting in smaller batches.

**Q: Can I schedule automated exports?**  
A: Contact administrator about scheduled export options.

**Q: Why can't I export to Excel?**  
A: Verify you have `CAN_VIEW_REPORT` permission. Check browser download settings.

### Fraud Detection

**Q: How do I know if fraud detection is working?**  
A: Run test queries with known fraudulent patterns and verify alerts trigger.

**Q: Can I customize fraud thresholds?**  
A: Yes, if you have `CAN_MANAGE_FRAUD_SETTINGS` permission.

**Q: Why am I getting so many false positives?**  
A: Thresholds may be too sensitive. Adjust percentiles higher.

**Q: How often should I review fraud alerts?**  
A: Best practice is daily review for high-risk areas.

### Distance Analysis

**Q: What's a good similarity score?**  
A: 90%+ = nearly identical, 70-89% = strong match, 50-69% = loose match.

**Q: Why are all my distance scores low?**  
A: The selected record may be genuinely unique/unusual.

**Q: How many fields should I select for distance analysis?**  
A: 5-10 fields recommended for best results.

**Q: Can I compare records from different tables?**  
A: No, distance analysis compares records within the same dataset.

### AI Natural Language Queries

**Q: Is the AI always accurate?**  
A: No, always review generated queries before executing.

**Q: Can I improve AI query generation?**  
A: Use clear, specific language. Avoid ambiguous terms.

**Q: Why is AI query generation unavailable?**  
A: AI service may be offline. Contact administrator.

**Q: What AI model is used?**  
A: Qwen 2.5 14B model running on Ollama (default configuration).

### Administration

**Q: How do I create a new user?**  
A: Navigate to Manage → Users → Create New User (requires permission).

**Q: Can I bulk import users?**  
A: Contact administrator for bulk import options.

**Q: How do I know which permissions a user has?**  
A: View user details → Roles → See assigned role permissions.

**Q: Can users have multiple roles?**  
A: Yes, users can be assigned multiple roles. Permissions are cumulative.

---

## Troubleshooting

### Login Issues

#### Problem: Cannot log in - "Invalid credentials"

**Possible Causes:**
- Incorrect username or password
- Account not active
- Caps Lock enabled

**Solutions:**
1. Verify username spelling
2. Check Caps Lock key
3. Try password reset
4. Contact administrator to verify account is active

---

#### Problem: Account locked after failed attempts

**Cause:** Too many incorrect password attempts

**Solution:**
- Wait 15 minutes for automatic unlock
- Or contact administrator for manual unlock

---

#### Problem: Session expires too quickly

**Cause:** Session timeout configured to 1 hour

**Solution:**
- Contact administrator to extend session timeout (if appropriate)
- Save work frequently
- Enable auto-logout warning (if available)

---

### Query Issues

#### Problem: Query returns no results

**Possible Causes:**
- Filters too restrictive
- Date range too narrow
- Field-level security limiting data
- No data exists matching criteria

**Solutions:**
1. Remove some filters and re-run
2. Expand date range
3. Verify field names are correct
4. Check if you have data access permissions

---

#### Problem: Query is very slow

**Possible Causes:**
- Large date range
- No indexes on queried fields
- Complex aggregations
- Database performance issues

**Solutions:**
1. Narrow date range
2. Add more specific filters
3. Limit number of returned rows
4. Request administrator to add indexes
5. Run during off-peak hours

---

#### Problem: Query error - "Invalid field name"

**Cause:** Field name doesn't exist or misspelled

**Solution:**
- Verify field name in schema
- Check for typos
- Use field selector dropdown instead of typing

---

#### Problem: Conditional formatting not working

**Possible Causes:**
- Rule syntax error
- Field value type mismatch
- Rule order incorrect

**Solutions:**
1. Verify rule condition syntax
2. Ensure field types match (number vs. string)
3. Check rule execution order

---

### Dashboard Issues

#### Problem: Dashboard not loading

**Possible Causes:**
- Network connection issue
- Query timeout
- Browser compatibility

**Solutions:**
1. Refresh page
2. Clear browser cache
3. Try different browser
4. Check network connection
5. Contact administrator if problem persists

---

#### Problem: Chart displays incorrectly

**Possible Causes:**
- Data type mismatch
- Missing data
- Chart configuration error

**Solutions:**
1. Verify data query returns expected results
2. Check chart configuration (axes, data mapping)
3. Try different chart type
4. Refresh dashboard

---

#### Problem: Dashboard won't save

**Possible Causes:**
- Insufficient permissions
- Network issue
- Invalid configuration

**Solutions:**
1. Verify you have `CAN_UPDATE_DASHBOARD` permission
2. Check network connection
3. Simplify dashboard (remove complex blocks)
4. Try saving to different workspace

---

### Data Export Issues

#### Problem: Export fails or times out

**Cause:** Dataset too large

**Solutions:**
1. Reduce date range
2. Add more filters to limit rows
3. Export in batches
4. Use CSV format (faster than Excel/PDF)

---

#### Problem: Export file is empty

**Possible Causes:**
- Query returned no results
- Export permission issue
- Browser download blocker

**Solutions:**
1. Verify query has results before exporting
2. Check browser download settings
3. Disable browser extensions temporarily
4. Try different browser

---

#### Problem: Excel export opens with errors

**Cause:** Data formatting issues or Excel version compatibility

**Solutions:**
1. Update Microsoft Excel
2. Try CSV format instead
3. Open in Google Sheets as alternative

---

### Fraud Detection Issues

#### Problem: Not receiving fraud alerts

**Possible Causes:**
- Thresholds set too high
- No fraudulent activity
- Notification settings incorrect

**Solutions:**
1. Review and lower fraud thresholds
2. Test with known fraudulent data
3. Check notification settings
4. Verify fraud detection is enabled

---

#### Problem: Too many false positive alerts

**Cause:** Thresholds too sensitive

**Solution:**
- Increase percentile thresholds (e.g., 95th to 97th)
- Increase standard deviation multipliers
- Increase minimum value thresholds

---

### Distance Analysis Issues

#### Problem: "Must select at least 3 fields"

**Cause:** Insufficient fields selected from results grid

**Solution:**
- Click on more field values in the results grid
- Ensure at least 3 fields are selected

---

#### Problem: No similar records found

**Possible Causes:**
- Date range too narrow
- Selected record is truly unique
- Fields don't overlap with other records

**Solutions:**
1. Expand date range
2. Select more common fields
3. This may be expected - record is genuinely unusual

---

#### Problem: Distance analysis very slow

**Cause:** Large dataset or wide date range

**Solutions:**
1. Narrow date range
2. Reduce "Max Results" parameter
3. Select fewer fields
4. Run during off-peak hours

---

### AI Query Issues

#### Problem: "AI service unavailable"

**Cause:** AI service not running or not configured

**Solution:**
- Contact administrator to start AI service
- Verify Ollama is running with Qwen model

---

#### Problem: Generated query is incorrect

**Cause:** AI misinterpreted request

**Solutions:**
1. Rephrase your request more clearly
2. Be more specific about fields and conditions
3. Use manual query builder instead
4. Review and edit generated query before executing

---

### Browser Issues

#### Problem: Page layout is broken

**Possible Causes:**
- Browser cache
- Unsupported browser version
- Browser extensions interfering

**Solutions:**
1. Clear browser cache and cookies
2. Update browser to latest version
3. Disable browser extensions
4. Try incognito/private mode

---

#### Problem: Buttons or features not working

**Cause:** JavaScript error or browser compatibility

**Solutions:**
1. Refresh page (F5)
2. Open browser developer console (F12) and check for errors
3. Try different browser
4. Clear cache
5. Report error to administrator with console log

---

### Performance Issues

#### Problem: System is slow overall

**Possible Causes:**
- High server load
- Network latency
- Browser resource usage
- Database performance

**Solutions:**
1. Close unnecessary browser tabs
2. Clear browser cache
3. Use wired internet connection if possible
4. Restart browser
5. Contact administrator if widespread issue

---

### Getting Additional Help

If you cannot resolve an issue using this guide:

1. **Contact Your Administrator**:
   - Provide detailed description of issue
   - Include error messages (screenshot if possible)
   - Describe steps to reproduce

2. **Check System Status**:
   - Ask administrator about known issues
   - Check if system maintenance is scheduled

3. **Provide Context**:
   - What were you trying to do?
   - What happened instead?
   - When did problem start?
   - Does it happen consistently?

4. **Browser Console Logs**:
   - Press F12 to open developer tools
   - Go to Console tab
   - Screenshot any red error messages
   - Provide to administrator

---

## Document Version History

| Version | Date | Changes |
|---------|------|---------|
| 1.0 | February 12, 2026 | Initial release |

---

## Additional Resources

- **System Architecture**: See [02_SYSTEM_ARCHITECTURE.md](02_SYSTEM_ARCHITECTURE.md)
- **API Documentation**: See [05_API_DOCUMENTATION.md](05_API_DOCUMENTATION.md)
- **Security Documentation**: See [09_SECURITY_DOCUMENTATION.md](09_SECURITY_DOCUMENTATION.md)
- **Fraud Detection Details**: See [FRAUD_DETECTION_API.md](FRAUD_DETECTION_API.md)
- **Distance Analysis Guide**: See [DISTANCE_ANALYSIS_QUICK_START.md](DISTANCE_ANALYSIS_QUICK_START.md)

---

**For technical support, contact your system administrator.**
