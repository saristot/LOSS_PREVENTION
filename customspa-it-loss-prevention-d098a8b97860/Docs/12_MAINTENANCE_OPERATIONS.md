# Loss Prevention Tool - Maintenance & Operations Guide

## Table of Contents
1. [Maintenance Schedule](#maintenance-schedule)
2. [Database Maintenance](#database-maintenance)
3. [Backup & Restore](#backup--restore)
4. [Performance Monitoring](#performance-monitoring)
5. [Log Management](#log-management)
6. [Security Monitoring](#security-monitoring)
7. [Capacity Planning](#capacity-planning)
8. [Upgrade Procedures](#upgrade-procedures)
9. [Disaster Recovery](#disaster-recovery)
10. [Health Checks](#health-checks)
11. [Troubleshooting](#troubleshooting)

## Maintenance Schedule

### Daily Tasks

**Critical (Required)**:
- [ ] Review system health dashboards
- [ ] Check backup completion status
- [ ] Monitor disk space usage (alert if >80%)
- [ ] Review authentication failure logs
- [ ] Verify data ingestion completion

**Commands**:
```bash
# Check system status
systemctl status lossprevention-api
systemctl status mongod

# Check disk usage
df -h | grep -E '(Filesystem|/var|/data)'

# Check MongoDB status
mongosh --eval "db.serverStatus()"

# Review failed logins (last 24 hours)
tail -n 1000 /var/log/lossprevention/api.log | grep "LOGIN_FAILED"
```

### Weekly Tasks

- [ ] Review application logs for errors
- [ ] Analyze slow query logs
- [ ] Check MongoDB index usage
- [ ] Review user access patterns
- [ ] Test backup restore procedure
- [ ] Update security patches (if available)
- [ ] Review fraud detection alerts

**Scripts**:
```bash
# Analyze slow queries (MongoDB)
mongosh LossPrevention --eval "
  db.setProfilingLevel(1, { slowms: 100 });
  db.system.profile.find().sort({ts:-1}).limit(10).pretty();
"

# Check index usage
mongosh LossPrevention --eval "
  db.ReportData.aggregate([{\$indexStats:{}}])
"
```

### Monthly Tasks

- [ ] Perform full database maintenance
- [ ] Review and optimize indexes
- [ ] Archive old logs
- [ ] Capacity planning review
- [ ] Security audit
- [ ] User access review
- [ ] Performance baseline comparison
- [ ] Update documentation
- [ ] Test disaster recovery plan

### Quarterly Tasks

- [ ] Major version upgrades (if available)
- [ ] Comprehensive security scan
- [ ] Disaster recovery drill
- [ ] Performance tuning review
- [ ] Infrastructure scaling assessment
- [ ] Vendor review (MongoDB, Azure, etc.)

## Database Maintenance

### MongoDB Maintenance

#### 1. Index Maintenance

**Review Index Usage**:
```javascript
// Connect to MongoDB
mongosh LossPrevention

// Check all indexes
db.ReportData.getIndexes()

// Check index statistics
db.ReportData.aggregate([{ $indexStats: {} }])

// Find unused indexes (ops.count == 0)
db.ReportData.aggregate([
  { $indexStats: {} },
  { $match: { "ops": 0 } }
])
```

**Drop Unused Indexes**:
```javascript
// CAREFUL: Only drop indexes not being used
db.ReportData.dropIndex("unused_index_name")
```

**Create New Indexes**:
```javascript
// For commonly queried fields
db.ReportData.createIndex({ "TransactionDate": 1 })
db.ReportData.createIndex({ "StoreID": 1, "TransactionDate": -1 })

// For text search
db.ReportData.createIndex({ "CustomerName": "text", "ItemDescription": "text" })
```

#### 2. Database Compaction

**Check Database Size**:
```javascript
db.stats(1024*1024)  // Size in MB
```

**Compact Collections** (reduces disk space):
```javascript
// During low-traffic period
db.runCommand({ compact: 'ReportData', force: true })
db.runCommand({ compact: 'Workspaces' })
db.runCommand({ compact: 'Dashboards' })
```

#### 3. Data Retention

**Verify TTL Index**:
```javascript
db.ReportData.getIndexes().find(idx => idx.expireAfterSeconds)
```

**Manual Data Cleanup** (if TTL not working):
```javascript
// Delete records older than 180 days
const cutoffDate = new Date();
cutoffDate.setDate(cutoffDate.getDate() - 180);

db.ReportData.deleteMany({
  _processedAt: { $lt: cutoffDate }
})
```

#### 4. Check Replica Set Health

```javascript
// Replication status
rs.status()

// Check replication lag
rs.printSlaveReplicationInfo()

// Check oplog size
db.oplog.rs.stats(1024*1024)
```

### Maintenance Scripts

**Complete Maintenance Script** (weekly-maintenance.sh):
```bash
#!/bin/bash
# Loss Prevention Database Maintenance
# Run weekly during low-traffic period

DATE=$(date +%Y%m%d)
LOG_FILE="/var/log/lossprevention/maintenance-$DATE.log"

echo "Starting database maintenance: $(date)" | tee -a $LOG_FILE

# 1. Check index usage
echo "Checking index usage..." | tee -a $LOG_FILE
mongosh LossPrevention --eval "
  db.ReportData.aggregate([
    { \$indexStats: {} },
    { \$project: { name: 1, 'accesses.ops': 1 } }
  ])
" | tee -a $LOG_FILE

# 2. Rebuild indexes
echo "Rebuilding indexes..." | tee -a $LOG_FILE
mongosh LossPrevention --eval "db.ReportData.reIndex()" | tee -a $LOG_FILE

# 3. Check disk usage
echo "Checking disk usage..." | tee -a $LOG_FILE
df -h /data | tee -a $LOG_FILE

# 4. Compact database (optional, requires downtime)
# mongosh LossPrevention --eval "db.runCommand({ compact: 'ReportData', force: true })"

echo "Maintenance completed: $(date)" | tee -a $LOG_FILE
```

**Make executable**:
```bash
chmod +x /usr/local/bin/weekly-maintenance.sh
```

**Schedule with cron**:
```bash
# Run every Sunday at 2 AM
crontab -e
0 2 * * 0 /usr/local/bin/weekly-maintenance.sh
```

## Backup & Restore

### Automated Backup Script

**backup-lossprevention.sh**:
```bash
#!/bin/bash
# Loss Prevention Backup Script

# Configuration
BACKUP_DIR="/backups/lossprevention"
RETENTION_DAYS=30
DATE=$(date +%Y%m%d_%H%M%S)
MONGO_URI="mongodb://backup_user:password@localhost:27017/LossPrevention?authSource=admin"

# Create backup directory
mkdir -p $BACKUP_DIR/$DATE

# Backup MongoDB
echo "Starting MongoDB backup..."
mongodump --uri="$MONGO_URI" --out=$BACKUP_DIR/$DATE --gzip

# Backup configuration files
echo "Backing up configuration files..."
cp /etc/lossprevention/appsettings.json $BACKUP_DIR/$DATE/
cp /etc/nginx/sites-available/lossprevention $BACKUP_DIR/$DATE/

# Create archive
echo "Creating compressed archive..."
cd $BACKUP_DIR
tar -czf lossprevention-backup-$DATE.tar.gz $DATE/
rm -rf $DATE/

# Upload to cloud storage (optional)
# az storage blob upload --file lossprevention-backup-$DATE.tar.gz --container backups

# Delete old backups
echo "Cleaning up old backups..."
find $BACKUP_DIR -name "*.tar.gz" -mtime +$RETENTION_DAYS -delete

echo "Backup completed: $BACKUP_DIR/lossprevention-backup-$DATE.tar.gz"
```

**Schedule Backups**:
```bash
# Daily backup at 1 AM
0 1 * * * /usr/local/bin/backup-lossprevention.sh >> /var/log/lossprevention/backup.log 2>&1
```

### Restore Procedure

**Full Restore**:
```bash
#!/bin/bash
# Restore from backup

BACKUP_FILE="/backups/lossprevention/lossprevention-backup-20260212_010000.tar.gz"
RESTORE_DIR="/tmp/restore"

# Extract backup
mkdir -p $RESTORE_DIR
tar -xzf $BACKUP_FILE -C $RESTORE_DIR

# Stop services
systemctl stop lossprevention-api

# Restore MongoDB
mongorestore --uri="mongodb://localhost:27017" --gzip $RESTORE_DIR/*/dump/LossPrevention

# Restore configuration
cp $RESTORE_DIR/*/appsettings.json /etc/lossprevention/

# Start services
systemctl start lossprevention-api

# Verify
curl -k https://localhost:443/swagger

echo "Restore completed"
```

**Selective Restore** (Single Collection):
```bash
# Restore only Users collection
mongorestore --uri="mongodb://localhost:27017/LossPrevention" \
  --collection=Users \
  --gzip \
  /backups/restore/dump/LossPrevention/Users.bson.gz
```

## Performance Monitoring

### Key Metrics to Monitor

| Metric | Target | Alert Threshold | Critical Threshold |
|--------|--------|-----------------|-------------------|
| API Response Time | < 200ms | > 500ms | > 2s |
| Database Query Time | < 100ms | > 500ms | > 2s |
| CPU Usage | < 70% | > 80% | > 90% |
| Memory Usage | < 75% | > 85% | > 95% |
| Disk Usage | < 70% | > 80% | > 90% |
| Failed Logins | < 10/hour | > 50/hour | > 100/hour |
| API Error Rate | < 1% | > 5% | > 10% |

### Monitoring Tools

#### Application Insights (Azure)

**Configure**:
```json
{
  "ApplicationInsights": {
    "ConnectionString": "InstrumentationKey=..."
  }
}
```

**Key Queries**:
```kusto
// Average response time by endpoint
requests
| where timestamp > ago(1h)
| summarize avg(duration), count() by name
| order by avg_duration desc

// Failed requests
requests
| where timestamp > ago(24h) and success == false
| summarize count() by name, resultCode
| order by count_ desc

// Slow queries
dependencies
| where timestamp > ago(1h) and type == "MongoDB"
| where duration > 1000
| project timestamp, name, duration, data
```

#### Prometheus + Grafana

**MongoDB Exporter**:
```yaml
# docker-compose.yml
mongodb-exporter:
  image: percona/mongodb_exporter:latest
  environment:
    - MONGODB_URI=mongodb://localhost:27017
  ports:
    - "9216:9216"
```

**Prometheus Configuration**:
```yaml
scrape_configs:
  - job_name: 'mongodb'
    static_configs:
      - targets: ['mongodb-exporter:9216']
  
  - job_name: 'lossprevention-api'
    static_configs:
      - targets: ['api:5000']
```

### Custom Health Check Script

**health-check.sh**:
```bash
#!/bin/bash
# Health Check Script

API_URL="https://localhost:443"
MONGO_URI="mongodb://localhost:27017"

# Check API health
API_STATUS=$(curl -s -o /dev/null -w "%{http_code}" $API_URL/swagger)
if [ "$API_STATUS" != "200" ]; then
  echo "ERROR: API not responding (Status: $API_STATUS)"
  # Send alert
  exit 1
fi

# Check MongoDB
MONGO_STATUS=$(mongosh --eval "db.adminCommand('ping')" --quiet 2>&1)
if [[ ! "$MONGO_STATUS" =~ "ok" ]]; then
  echo "ERROR: MongoDB not responding"
  # Send alert
  exit 1
fi

# Check disk space
DISK_USAGE=$(df -h /data | awk 'NR==2 {print $5}' | sed 's/%//')
if [ "$DISK_USAGE" -gt 80 ]; then
  echo "WARNING: Disk usage at ${DISK_USAGE}%"
fi

echo "All systems healthy"
exit 0
```

## Log Management

### Log Locations

| Component | Location |
|-----------|----------|
| API Logs | `/var/log/lossprevention/api.log` |
| MongoDB Logs | `/var/log/mongodb/mongod.log` |
| Nginx Logs | `/var/log/nginx/access.log`, `/var/log/nginx/error.log` |
| Systemd Logs | `journalctl -u lossprevention-api` |

### Log Rotation

**Configure logrotate** (/etc/logrotate.d/lossprevention):
```
/var/log/lossprevention/*.log {
    daily
    rotate 30
    compress
    delaycompress
    missingok
    notifempty
    create 0644 www-data www-data
    sharedscripts
    postrotate
        systemctl reload lossprevention-api > /dev/null 2>&1 || true
    endscript
}
```

### Log Analysis

**Find errors in last hour**:
```bash
tail -n 10000 /var/log/lossprevention/api.log | grep ERROR
```

**Count errors by type**:
```bash
grep ERROR /var/log/lossprevention/api.log | cut -d' ' -f4 | sort | uniq -c | sort -rn
```

**Monitor logs in real-time**:
```bash
tail -f /var/log/lossprevention/api.log | grep -E '(ERROR|WARN)'
```

## Security Monitoring

### Security Checklist

**Daily**:
- [ ] Review failed login attempts
- [ ] Check for unusual API activity
- [ ] Monitor authentication token usage

**Weekly**:
- [ ] Review user permission changes
- [ ] Analyze access patterns
- [ ] Check for suspicious data exports

**Monthly**:
- [ ] Security patch review
- [ ] Vulnerability scan
- [ ] Access review (remove inactive users)

### Security Monitoring Script

```bash
#!/bin/bash
# Security monitoring

# Failed login attempts (last 24 hours)
echo "=== Failed Logins ==="
grep "LOGIN_FAILED" /var/log/lossprevention/api.log | \
  tail -n 100 | \
  cut -d' ' -f7 | \
  sort | uniq -c | sort -rn

# Permission changes
echo "=== Permission Changes ==="
grep "PERMISSION_CHANGED" /var/log/lossprevention/api.log | tail -n 20

# Large data exports
echo "=== Large Exports ==="
grep "EXPORT" /var/log/lossprevention/api.log | \
  grep "records > 10000"
```

## Capacity Planning

### Growth Projections

**Calculate Monthly Growth**:
```javascript
// MongoDB shell
use LossPrevention

// Document count by month
db.ReportData.aggregate([
  {
    $group: {
      _id: {
        year: { $year: "$_processedAt" },
        month: { $month: "$_processedAt" }
      },
      count: { $sum: 1 }
    }
  },
  { $sort: { "_id.year": -1, "_id.month": -1 } }
])

// Storage size by collection
db.getCollectionNames().forEach(function(col) {
  var stats = db[col].stats(1024*1024);
  print(col + ": " + stats.storageSize + " MB");
})
```

### Scaling Thresholds

| Resource | Scale Up When | Recommended Action |
|----------|---------------|-------------------|
| CPU | > 70% sustained | Add more cores or horizontal scale |
| Memory | > 80% sustained | Increase RAM or optimize queries |
| Disk | > 75% full | Add storage or implement archiving |
| Database | Query time > 1s | Add indexes, optimize queries, or shard |
| API | Response time > 500ms | Scale horizontally or optimize code |

## Upgrade Procedures

### .NET API Upgrade

```bash
# 1. Backup current version
cp -r /var/www/lossprevention-api /var/www/lossprevention-api.backup

# 2. Pull new code
cd /path/to/source
git pull origin main

# 3. Build new version
dotnet publish -c Release -o /tmp/new-release

# 4. Stop service
systemctl stop lossprevention-api

# 5. Deploy new version
cp -r /tmp/new-release/* /var/www/lossprevention-api/

# 6. Update configuration (if needed)
nano /var/www/lossprevention-api/appsettings.json

# 7. Start service
systemctl start lossprevention-api

# 8. Verify
curl -k https://localhost:443/swagger

# 9. If issues, rollback
# systemctl stop lossprevention-api
# rm -rf /var/www/lossprevention-api
# mv /var/www/lossprevention-api.backup /var/www/lossprevention-api
# systemctl start lossprevention-api
```

### MongoDB Upgrade

```bash
# 1. Backup database
mongodump --uri="mongodb://localhost:27017/LossPrevention" --out=/backups/pre-upgrade

# 2. Check compatibility
mongosh --eval "db.adminCommand({ getParameter: 1, featureCompatibilityVersion: 1 })"

# 3. Stop MongoDB
sudo systemctl stop mongod

# 4. Upgrade packages
sudo apt-get update
sudo apt-get install -y mongodb-org

# 5. Start MongoDB
sudo systemctl start mongod

# 6. Verify version
mongosh --eval "db.version()"

# 7. Update feature compatibility
mongosh --eval "db.adminCommand({ setFeatureCompatibilityVersion: '7.0' })"
```

## Disaster Recovery

### Recovery Time Objectives (RTO)

| Scenario | RTO | Recovery Steps |
|----------|-----|----------------|
| API Failure | 15 minutes | Restart service or failover to standby |
| Database Failure | 1 hour | Restore from backup or promote replica |
| Server Failure | 4 hours | Provision new server, restore backup |
| Data Center Outage | 8 hours | Failover to DR site |

### Disaster Recovery Drill

**Quarterly Test Procedure**:

1. **Announce Drill**
   - Notify team
   - Set start time (low-traffic period)

2. **Simulate Failure**
   ```bash
   # Stop all services
   systemctl stop lossprevention-api
   systemctl stop mongod
   ```

3. **Execute Recovery**
   - Follow recovery procedures
   - Document time taken
   - Note any issues

4. **Verify Functionality**
   - Test user login
   - Test data queries
   - Test critical workflows

5. **Document Results**
   - Actual RTO vs. target
   - Issues encountered
   - Improvements needed

## Health Checks

### API Health Check Endpoint

**Implement** (if not present):
```csharp
public class HealthCheckEndpoint : EndpointWithoutRequest
{
    private readonly IMongoClient _mongoClient;
    
    public override void Configure()
    {
        Get("/health");
        AllowAnonymous();
    }
    
    public override async Task HandleAsync(CancellationToken ct)
    {
        var health = new
        {
            status = "healthy",
            timestamp = DateTime.UtcNow,
            checks = new Dictionary<string, string>()
        };
        
        try
        {
            // Check MongoDB
            await _mongoClient.GetDatabase("LossPrevention")
                .RunCommandAsync<BsonDocument>(new BsonDocument("ping", 1));
            health.checks["mongodb"] = "healthy";
        }
        catch
        {
            health.checks["mongodb"] = "unhealthy";
        }
        
        await SendOkAsync(health, ct);
    }
}
```

### External Monitoring

**UptimeRobot** or similar:
- Monitor https://yourdomain.com/health every 5 minutes
- Alert if response code != 200
- Alert if response time > 2 seconds

## Troubleshooting

### API Not Starting

**Symptoms**: Service fails to start

**Diagnosis**:
```bash
# Check service status
systemctl status lossprevention-api

# Check logs
journalctl -u lossprevention-api -n 100 --no-pager

# Check port binding
netstat -tlnp | grep :5000
```

**Common Causes**:
1. Port already in use → Change port in launchSettings.json
2. Missing dependencies → `dotnet restore`
3. Configuration error → Validate appsettings.json
4. Permissions issue → `chown -R www-data:www-data /var/www/lossprevention-api`

### High CPU Usage

**Diagnosis**:
```bash
# Check processes
top
htop

# MongoDB slow queries
mongosh --eval "db.currentOp({ 'secs_running': { '\$gte': 1 } })"

# .NET profiling
dotnet-trace collect --process-id $(pgrep -f LossPrevention.API)
```

**Solutions**:
- Optimize slow queries
- Add indexes
- Scale horizontally
- Cache frequently accessed data

### Database Connection Issues

**Symptoms**: "MongoConnectionException"

**Diagnosis**:
```bash
# Test MongoDB connectivity
mongosh mongodb://localhost:27017

# Check MongoDB is running
systemctl status mongod

# Check firewall
sudo ufw status
```

**Solutions**:
- Verify connection string
- Check MongoDB is running
- Verify firewall rules
- Check authentication credentials

### Slow Queries

**Diagnosis**:
```javascript
// Enable profiling
db.setProfilingLevel(2)

// View slow queries
db.system.profile.find({ millis: { $gt: 100 } }).sort({ ts: -1 }).limit(10)
```

**Solutions**:
- Add indexes on queried fields
- Optimize aggregation pipelines
- Limit result sets
- Use projection to return only needed fields

---

**Document Version**: 1.0  
**Last Updated**: February 12, 2026  
**Next Review**: Monthly  
**Contact**: ops-team@yourdomain.com
