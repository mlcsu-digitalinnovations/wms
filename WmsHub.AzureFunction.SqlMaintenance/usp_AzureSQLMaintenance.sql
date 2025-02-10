/****** Object:  StoredProcedure [dbo].[usp_AzureSQLMaintenance]    Script Date: 13/03/2022 09:45:10 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[usp_AzureSQLMaintenance]
@operation NVARCHAR (10)='all', @mode NVARCHAR (10)='smart', @ResumableIndexRebuild BIT=0, @RebuildHeaps BIT=0, @LogToTable BIT=1, @debug NVARCHAR='none'
AS
BEGIN
    SET NOCOUNT ON;
    SET QUOTED_IDENTIFIER ON;
    DECLARE @idxIdentifierBegin AS CHAR (1), @idxIdentifierEnd AS CHAR (1);
    DECLARE @statsIdentifierBegin AS CHAR (1), @statsIdentifierEnd AS CHAR (1);
    DECLARE @msg AS NVARCHAR (MAX);
    DECLARE @minPageCountForIndex AS INT;
    SET @minPageCountForIndex = 40;
    DECLARE @OperationTime AS DATETIME2;
    SET @OperationTime = sysdatetime();
    DECLARE @KeepXOperationInLog AS INT;
    SET @KeepXOperationInLog = 3;
    DECLARE @ScriptHasAnError AS INT;
    SET @ScriptHasAnError = 0;
    DECLARE @ResumableIndexRebuildSupported AS INT;
    DECLARE @indexStatsMode AS sysname;
    DECLARE @LowFragmentationBoundry AS INT;
    SET @LowFragmentationBoundry = 5;
    DECLARE @HighFragmentationBoundry AS INT;
    SET @HighFragmentationBoundry = 30;
    SET @operation = lower(@operation);
    SET @mode = lower(@mode);
    SET @debug = lower(@debug);
    IF @mode NOT IN ('smart', 'dummy')
        SET @mode = 'smart';
    IF @operation NOT IN ('index', 'statistics', 'all')
       OR @operation IS NULL
        BEGIN
            RAISERROR ('@operation (varchar(10)) [mandatory]', 0, 0);
            RAISERROR (' Select operation to perform:', 0, 0);
            RAISERROR ('     "index" to perform index maintenance', 0, 0);
            RAISERROR ('     "statistics" to perform statistics maintenance', 0, 0);
            RAISERROR ('     "all" to perform indexes and statistics maintenance', 0, 0);
            RAISERROR (' ', 0, 0);
            RAISERROR ('@mode(varchar(10)) [optional]', 0, 0);
            RAISERROR (' optionaly you can supply second parameter for operation mode: ', 0, 0);
            RAISERROR ('     "smart" (Default) using smart decision about what index or stats should be touched.', 0, 0);
            RAISERROR ('     "dummy" going through all indexes and statistics regardless thier modifications or fragmentation.', 0, 0);
            RAISERROR (' ', 0, 0);
            RAISERROR ('@ResumableIndexRebuild(bit) [optional]', 0, 0);
            RAISERROR (' optionaly you can choose to rebuild indexes as resumable operation: ', 0, 0);
            RAISERROR ('     "0" (Default) using non resumable index rebuild.', 0, 0);
            RAISERROR ('     "1" using resumable index rebuild when it is supported.', 0, 0);
            RAISERROR (' ', 0, 0);
            RAISERROR ('@RebuildHeaps(bit) [optional]', 0, 0);
            RAISERROR (' Logging option: @LogToTable(bit)', 0, 0);
            RAISERROR ('     0 - do not log operation to table', 0, 0);
            RAISERROR ('     1 - (Default) log operation to table', 0, 0);
            RAISERROR ('		for logging option only 3 last execution will be kept by default. this can be changed by easily in the procedure body.', 0, 0);
            RAISERROR ('		Log table will be created automatically if not exists.', 0, 0);
            RAISERROR (' ', 0, 0);
            RAISERROR ('@LogToTable(bit) [optional]', 0, 0);
            RAISERROR (' Rebuild HEAPS to fix forwarded records issue on tables with no clustered index', 0, 0);
            RAISERROR ('     0 - (Default) do not rebuild heaps', 0, 0);
            RAISERROR ('     1 - Rebuild heaps based on @mode parameter, @mode=dummy will rebuild all heaps', 0, 0);
            RAISERROR (' ', 0, 0);
            RAISERROR ('Example:', 0, 0);
            RAISERROR ('		exec  AzureSQLMaintenance ''all'', @LogToTable=1', 0, 0);
        END
    ELSE
        BEGIN
            IF object_id('AzureSQLMaintenanceLog') IS NULL
               AND @LogToTable = 1
                BEGIN
                    CREATE TABLE AzureSQLMaintenanceLog (
                        id            BIGINT         IDENTITY (1, 1) PRIMARY KEY,
                        OperationTime DATETIME2     ,
                        command       VARCHAR (4000),
                        ExtraInfo     VARCHAR (4000),
                        StartTime     DATETIME2     ,
                        EndTime       DATETIME2     ,
                        StatusMessage VARCHAR (1000)
                    );
                END
            IF OBJECT_ID('AzureSQLMaintenanceCMDQueue') IS NOT NULL
                BEGIN
                    IF EXISTS (SELECT *
                               FROM   AzureSQLMaintenanceCMDQueue
                               WHERE  ID = -1)
                        BEGIN
                            SET @operation = 'resume';
                            SELECT TOP 1 @LogToTable = JSON_VALUE(ExtraInfo, '$.LogToTable'),
                                         @mode = JSON_VALUE(ExtraInfo, '$.mode'),
                                         @ResumableIndexRebuild = JSON_VALUE(ExtraInfo, '$.ResumableIndexRebuild')
                            FROM   AzureSQLMaintenanceCMDQueue
                            WHERE  ID = -1;
                            RAISERROR ('-----------------------', 0, 0);
                            SET @msg = 'Resuming previous operation';
                            RAISERROR (@msg, 0, 0);
                            RAISERROR ('-----------------------', 0, 0);
                        END
                    ELSE
                        BEGIN
                            DROP TABLE [AzureSQLMaintenanceCMDQueue];
                        END
                END
            RAISERROR ('-----------------------', 0, 0);
            SET @msg = 'set operation = ' + @operation;
            RAISERROR (@msg, 0, 0);
            SET @msg = 'set mode = ' + @mode;
            RAISERROR (@msg, 0, 0);
            SET @msg = 'set ResumableIndexRebuild = ' + CAST (@ResumableIndexRebuild AS VARCHAR (1));
            RAISERROR (@msg, 0, 0);
            SET @msg = 'set RebuildHeaps = ' + CAST (@RebuildHeaps AS VARCHAR (1));
            RAISERROR (@msg, 0, 0);
            SET @msg = 'set LogToTable = ' + CAST (@LogToTable AS VARCHAR (1));
            RAISERROR (@msg, 0, 0);
            RAISERROR ('-----------------------', 0, 0);
        END
    IF @LogToTable = 1
        INSERT  INTO AzureSQLMaintenanceLog
        VALUES (@OperationTime, NULL, NULL, sysdatetime(), sysdatetime(), 'Starting operation: Operation=' + @operation + ' Mode=' + @mode + ' Keep log for last ' + CAST (@KeepXOperationInLog AS VARCHAR (10)) + ' operations');
    IF @operation != 'resume'
        CREATE TABLE AzureSQLMaintenanceCMDQueue (
            ID        INT            IDENTITY PRIMARY KEY,
            txtCMD    NVARCHAR (MAX),
            ExtraInfo VARCHAR (MAX) 
        );
    IF @ResumableIndexRebuild = 1
        BEGIN
            IF CAST (SERVERPROPERTY('EngineEdition') AS INT) >= 5
               OR CAST (SERVERPROPERTY('ProductMajorVersion') AS INT) >= 14
                BEGIN
                    SET @ResumableIndexRebuildSupported = 1;
                END
            ELSE
                BEGIN
                    SET @ResumableIndexRebuildSupported = 0;
                    SET @msg = 'Resumable index rebuild is not supported on this database';
                    RAISERROR (@msg, 0, 0);
                    IF @LogToTable = 1
                        INSERT  INTO AzureSQLMaintenanceLog
                        VALUES (@OperationTime, NULL, NULL, sysdatetime(), sysdatetime(), @msg);
                END
        END
    IF @operation IN ('index', 'all')
        BEGIN
            IF @mode = 'smart'
               AND @RebuildHeaps = 1
                SET @indexStatsMode = 'SAMPLED';
            ELSE
                SET @indexStatsMode = 'LIMITED';
            RAISERROR ('Get index information...(wait)', 0, 0)
                WITH NOWAIT;
            SELECT   idxs.[object_id],
                     OBJECT_SCHEMA_NAME(idxs.object_id) AS ObjectSchema,
                     object_name(idxs.object_id) AS ObjectName,
                     idxs.name AS IndexName,
                     idxs.type,
                     idxs.type_desc,
                     i.avg_fragmentation_in_percent,
                     i.page_count,
                     i.index_id,
                     i.partition_number,
                     i.avg_page_space_used_in_percent,
                     i.record_count,
                     i.ghost_record_count,
                     i.forwarded_record_count,
                     NULL AS OnlineOpIsNotSupported,
                     NULL AS ObjectDoesNotSupportResumableOperation,
                     0 AS SkipIndex,
                     replicate('', 128) AS SkipReason
            INTO     #idxBefore
            FROM     sys.indexes AS idxs
                     INNER JOIN
                     sys.dm_db_index_physical_stats(DB_ID(), NULL, NULL, NULL, @indexStatsMode) AS i
                     ON i.object_id = idxs.object_id
                        AND i.index_id = idxs.index_id
            WHERE    idxs.type IN (0, 1, 2, 5, 6)
                     AND (alloc_unit_type_desc = 'IN_ROW_DATA'
                          OR alloc_unit_type_desc IS NULL)
                     AND OBJECT_SCHEMA_NAME(idxs.object_id) != 'sys'
                     AND idxs.is_disabled = 0
            ORDER BY i.avg_fragmentation_in_percent DESC, i.page_count DESC;
            UPDATE #idxBefore
            SET    OnlineOpIsNotSupported = 1
            WHERE  [object_id] IN (SELECT [object_id]
                                   FROM   #idxBefore
                                   WHERE  [type] = 3);
            UPDATE #idxBefore
            SET    OnlineOpIsNotSupported = 1
            WHERE  index_id = 1
                   AND [object_id] IN (SELECT object_id
                                       FROM   sys.columns AS c
                                              INNER JOIN
                                              sys.types AS t
                                              ON c.user_type_id = t.user_type_id
                                       WHERE  t.name IN ('text', 'ntext', 'image'));
            UPDATE #idxBefore
            SET    OnlineOpIsNotSupported = 1
            WHERE  CONVERT (VARCHAR (100), serverproperty('Edition')) LIKE '%Express%'
                   OR CONVERT (VARCHAR (100), serverproperty('Edition')) LIKE '%Standard%'
                   OR CONVERT (VARCHAR (100), serverproperty('Edition')) LIKE '%Web%';
            UPDATE idx
            SET    ObjectDoesNotSupportResumableOperation = 1
            FROM   #idxBefore AS idx
                   INNER JOIN
                   sys.index_columns AS ic
                   ON idx.object_id = ic.object_id
                      AND idx.index_id = ic.index_id
                   INNER JOIN
                   sys.columns AS c
                   ON ic.object_id = c.object_id
                      AND ic.column_id = c.column_id
            WHERE  c.is_computed = 1
                   OR system_type_id = 189;
            UPDATE #idxBefore
            SET    SkipIndex  = 1,
                   SkipReason = 'Maintenance is not needed as table is too small'
            WHERE  ((page_count <= @minPageCountForIndex))
                   AND @mode != 'dummy';
            UPDATE #idxBefore
            SET    SkipIndex  = 1,
                   SkipReason = 'Maintenance is not needed as fragmentation % is low'
            WHERE  ((page_count > @minPageCountForIndex
                     AND avg_fragmentation_in_percent < @LowFragmentationBoundry))
                   AND @mode != 'dummy';
            UPDATE #idxBefore
            SET    SkipIndex  = 1,
                   SkipReason = 'Columnstore index'
            WHERE  (type IN (5, 6))
                   AND @mode != 'dummy';
            RAISERROR ('---------------------------------------', 0, 0)
                WITH NOWAIT;
            RAISERROR ('Index Information:', 0, 0)
                WITH NOWAIT;
            RAISERROR ('---------------------------------------', 0, 0)
                WITH NOWAIT;
            SELECT @msg = count(*)
            FROM   #idxBefore;
            SET @msg = 'Total Indexes: ' + @msg;
            RAISERROR (@msg, 0, 0)
                WITH NOWAIT;
            SELECT @msg = avg(avg_fragmentation_in_percent)
            FROM   #idxBefore
            WHERE  page_count > @minPageCountForIndex;
            SET @msg = 'Average Fragmentation: ' + @msg;
            RAISERROR (@msg, 0, 0)
                WITH NOWAIT;
            SELECT @msg = sum(IIF (avg_fragmentation_in_percent >= @LowFragmentationBoundry
                                   AND page_count > @minPageCountForIndex, 1, 0))
            FROM   #idxBefore;
            SET @msg = 'Fragmented Indexes: ' + @msg;
            RAISERROR (@msg, 0, 0)
                WITH NOWAIT;
            RAISERROR ('---------------------------------------', 0, 0)
                WITH NOWAIT;
            IF EXISTS (SELECT 1
                       FROM   #idxBefore
                       WHERE  IndexName LIKE '%[%'
                              OR IndexName LIKE '%]%'
                              OR ObjectSchema LIKE '%[%'
                              OR ObjectSchema LIKE '%]%'
                              OR ObjectName LIKE '%[%'
                              OR ObjectName LIKE '%]%')
                BEGIN
                    SET @idxIdentifierBegin = '"';
                    SET @idxIdentifierEnd = '"';
                END
            ELSE
                BEGIN
                    SET @idxIdentifierBegin = '[';
                    SET @idxIdentifierEnd = ']';
                END
            INSERT INTO AzureSQLMaintenanceCMDQueue (txtCMD, ExtraInfo)
            SELECT 'ALTER INDEX ' + @idxIdentifierBegin + IndexName + @idxIdentifierEnd + ' ON ' + @idxIdentifierBegin + ObjectSchema + @idxIdentifierEnd + '.' + @idxIdentifierBegin + ObjectName + @idxIdentifierEnd + ' ' + CASE WHEN (avg_fragmentation_in_percent BETWEEN @LowFragmentationBoundry AND @HighFragmentationBoundry
                                                                                                                                                                                                                                          AND @mode = 'smart')
                                                                                                                                                                                                                                         OR (@mode = 'dummy'
                                                                                                                                                                                                                                             AND type IN (5, 6)) THEN 'REORGANIZE;' WHEN OnlineOpIsNotSupported = 1 THEN 'REBUILD WITH(ONLINE=OFF,MAXDOP=1);' WHEN ObjectDoesNotSupportResumableOperation = 1
                                                                                                                                                                                                                                                                                                                                                                   OR @ResumableIndexRebuildSupported = 0
                                                                                                                                                                                                                                                                                                                                                                   OR @ResumableIndexRebuild = 0 THEN 'REBUILD WITH(ONLINE=ON,MAXDOP=1);' ELSE 'REBUILD WITH(ONLINE=ON,MAXDOP=1, RESUMABLE=ON);' END AS txtCMD,
                   CASE WHEN type IN (5, 6) THEN 'Dummy mode, reorganize columnstore indexes' ELSE 'Current fragmentation: ' + format(avg_fragmentation_in_percent / 100, 'p') + ' with ' + CAST (page_count AS NVARCHAR (20)) + ' pages' END AS ExtraInfo
            FROM   #idxBefore
            WHERE  SkipIndex = 0
                   AND type != 0;
            IF @RebuildHeaps = 1
                BEGIN
                    INSERT INTO AzureSQLMaintenanceCMDQueue (txtCMD, ExtraInfo)
                    SELECT 'ALTER TABLE ' + @idxIdentifierBegin + ObjectSchema + @idxIdentifierEnd + '.' + @idxIdentifierBegin + ObjectName + @idxIdentifierEnd + ' REBUILD;' AS txtCMD,
                           'Rebuilding heap - forwarded records ' + CAST (forwarded_record_count AS VARCHAR (100)) + ' out of ' + CAST (record_count AS VARCHAR (100)) + ' record in the table' AS ExtraInfo
                    FROM   #idxBefore
                    WHERE  type = 0
                           AND (@mode = 'dummy'
                                OR (forwarded_record_count / NULLIF (record_count, 0) > 0.3)
                                OR (forwarded_record_count > 105000));
                END
        END
    IF @operation IN ('statistics', 'all')
        BEGIN
            RAISERROR ('Get statistics information...', 0, 0)
                WITH NOWAIT;
            SELECT   OBJECT_SCHEMA_NAME(s.object_id) AS ObjectSchema,
                     object_name(s.object_id) AS ObjectName,
                     s.object_id,
                     s.stats_id,
                     s.name AS StatsName,
                     sp.last_updated,
                     sp.rows,
                     sp.rows_sampled,
                     sp.modification_counter,
                     i.type,
                     i.type_desc,
                     0 AS SkipStatistics
            INTO     #statsBefore
            FROM     sys.stats AS s CROSS APPLY sys.dm_db_stats_properties(s.object_id, s.stats_id) AS sp
                     LEFT OUTER JOIN
                     sys.indexes AS i
                     ON sp.object_id = i.object_id
                        AND sp.stats_id = i.index_id
            WHERE    OBJECT_SCHEMA_NAME(s.object_id) != 'sys'
                     AND (isnull(sp.modification_counter, 0) > 0
                          OR @mode = 'dummy')
            ORDER BY sp.last_updated ASC;
            IF @operation = 'all'
                UPDATE _stats
                SET    SkipStatistics = 1
                FROM   #statsBefore AS _stats
                       INNER JOIN
                       #idxBefore AS _idx
                       ON _idx.ObjectSchema = _stats.ObjectSchema
                          AND _idx.ObjectName = _stats.ObjectName
                          AND _idx.IndexName = _stats.StatsName
                WHERE  _idx.SkipIndex = 0;
            UPDATE #statsBefore
            SET    SkipStatistics = 1
            WHERE  type IN (5, 6);
            IF @ResumableIndexRebuildSupported = 1
                BEGIN
                    UPDATE _stats
                    SET    SkipStatistics = 1
                    FROM   #statsBefore AS _stats
                           INNER JOIN
                           sys.index_resumable_operations AS iro
                           ON _stats.object_id = iro.object_id
                              AND _stats.stats_id = iro.index_id;
                END
            RAISERROR ('---------------------------------------', 0, 0)
                WITH NOWAIT;
            RAISERROR ('Statistics Information:', 0, 0)
                WITH NOWAIT;
            RAISERROR ('---------------------------------------', 0, 0)
                WITH NOWAIT;
            SELECT @msg = sum(modification_counter)
            FROM   #statsBefore;
            SET @msg = 'Total Modifications: ' + @msg;
            RAISERROR (@msg, 0, 0)
                WITH NOWAIT;
            SELECT @msg = sum(IIF (modification_counter > 0, 1, 0))
            FROM   #statsBefore;
            SET @msg = 'Modified Statistics: ' + @msg;
            RAISERROR (@msg, 0, 0)
                WITH NOWAIT;
            RAISERROR ('---------------------------------------', 0, 0)
                WITH NOWAIT;
            IF EXISTS (SELECT 1
                       FROM   #statsBefore
                       WHERE  StatsName LIKE '%[%'
                              OR StatsName LIKE '%]%'
                              OR ObjectSchema LIKE '%[%'
                              OR ObjectSchema LIKE '%]%'
                              OR ObjectName LIKE '%[%'
                              OR ObjectName LIKE '%]%')
                BEGIN
                    SET @statsIdentifierBegin = '"';
                    SET @statsIdentifierEnd = '"';
                END
            ELSE
                BEGIN
                    SET @statsIdentifierBegin = '[';
                    SET @statsIdentifierEnd = ']';
                END
            INSERT INTO AzureSQLMaintenanceCMDQueue (txtCMD, ExtraInfo)
            SELECT 'UPDATE STATISTICS ' + @statsIdentifierBegin + ObjectSchema + +@statsIdentifierEnd + '.' + @statsIdentifierBegin + ObjectName + @statsIdentifierEnd + ' (' + @statsIdentifierBegin + StatsName + @statsIdentifierEnd + ') WITH FULLSCAN;' AS txtCMD,
                   '#rows:' + CAST ([rows] AS VARCHAR (100)) + ' #modifications:' + CAST (modification_counter AS VARCHAR (100)) + ' modification percent: ' + format((1.0 * modification_counter / rows), 'p') AS ExtraInfo
            FROM   #statsBefore
            WHERE  SkipStatistics = 0;
        END
    IF @operation IN ('statistics', 'index', 'all', 'resume')
        BEGIN
            DECLARE @SQLCMD AS NVARCHAR (MAX);
            DECLARE @ID AS INT;
            DECLARE @ExtraInfo AS NVARCHAR (MAX);
            IF @debug != 'none'
                BEGIN
                    DROP TABLE IF EXISTS idxBefore;
                    DROP TABLE IF EXISTS statsBefore;
                    DROP TABLE IF EXISTS cmdQueue;
                    IF object_id('tempdb..#idxBefore') IS NOT NULL
                        SELECT *
                        INTO   idxBefore
                        FROM   #idxBefore;
                    IF object_id('tempdb..#statsBefore') IS NOT NULL
                        SELECT *
                        INTO   statsBefore
                        FROM   #statsBefore;
                    IF object_id('tempdb..AzureSQLMaintenanceCMDQueue') IS NOT NULL
                        SELECT *
                        INTO   cmdQueue
                        FROM   AzureSQLMaintenanceCMDQueue;
                END
            IF @operation != 'resume'
                BEGIN
                    SET @ExtraInfo = (SELECT TOP 1 @LogToTable AS LogToTable,
                                                   @operation AS operation,
                                                   @OperationTime AS operationTime,
                                                   @mode AS mode,
                                                   @ResumableIndexRebuild AS ResumableIndexRebuild
                                      FROM   sys.tables
                                      FOR    JSON PATH, WITHOUT_ARRAY_WRAPPER);
                    SET IDENTITY_INSERT AzureSQLMaintenanceCMDQueue ON;
                    INSERT  INTO AzureSQLMaintenanceCMDQueue (ID, txtCMD, ExtraInfo)
                    VALUES                                  (-1, 'parameters to be used by resume code path', @ExtraInfo);
                    SET IDENTITY_INSERT AzureSQLMaintenanceCMDQueue OFF;
                END
            SET ANSI_WARNINGS ON;
            RAISERROR ('Start executing commands...', 0, 0)
                WITH NOWAIT;
            DECLARE @T TABLE (
                ID        INT           ,
                txtCMD    NVARCHAR (MAX),
                ExtraInfo NVARCHAR (MAX));
            WHILE EXISTS (SELECT *
                          FROM   AzureSQLMaintenanceCMDQueue
                          WHERE  ID > 0)
                BEGIN
                    UPDATE TOP (1)
                     AzureSQLMaintenanceCMDQueue
                    SET    txtCMD = txtCMD
                    OUTPUT deleted.* INTO @T
                    WHERE  ID > 0;
                    SELECT TOP (1) @ID = ID,
                                   @SQLCMD = txtCMD,
                                   @ExtraInfo = ExtraInfo
                    FROM   @T;
                    RAISERROR (@SQLCMD, 0, 0)
                        WITH NOWAIT;
                    IF @LogToTable = 1
                        INSERT  INTO AzureSQLMaintenanceLog
                        VALUES (@OperationTime, @SQLCMD, @ExtraInfo, sysdatetime(), NULL, 'Started');
                    BEGIN TRY
                        EXECUTE (@SQLCMD);
                        IF @LogToTable = 1
                            UPDATE AzureSQLMaintenanceLog
                            SET    EndTime       = sysdatetime(),
                                   StatusMessage = 'Succeeded'
                            WHERE  id = SCOPE_IDENTITY();
                    END TRY
                    BEGIN CATCH
                        SET @ScriptHasAnError = 1;
                        SET @msg = 'FAILED : ' + CAST (ERROR_NUMBER() AS VARCHAR (50)) + ERROR_MESSAGE();
                        RAISERROR (@msg, 0, 0)
                            WITH NOWAIT;
                        IF @LogToTable = 1
                            UPDATE AzureSQLMaintenanceLog
                            SET    EndTime       = sysdatetime(),
                                   StatusMessage = @msg
                            WHERE  id = SCOPE_IDENTITY();
                    END CATCH
                    DELETE AzureSQLMaintenanceCMDQueue
                    WHERE  ID = @ID;
                    DELETE @T;
                END
            DROP TABLE AzureSQLMaintenanceCMDQueue;
        END
    IF @LogToTable = 1
        BEGIN
            DELETE AzureSQLMaintenanceLog
            FROM   AzureSQLMaintenanceLog AS L
                   INNER JOIN
                   (SELECT   DISTINCT OperationTime
                    FROM     AzureSQLMaintenanceLog
                    ORDER BY OperationTime DESC
                    OFFSET @KeepXOperationInLog ROWS) AS F
                   ON L.OperationTime = F.OperationTime;
            INSERT  INTO AzureSQLMaintenanceLog
            VALUES (@OperationTime, NULL, CAST (@@rowcount AS VARCHAR (100)) + ' rows purged from log table because number of operations to keep is set to: ' + CAST (@KeepXOperationInLog AS VARCHAR (100)), sysdatetime(), sysdatetime(), 'Cleanup Log Table');
        END
    IF @ScriptHasAnError = 0
        RAISERROR ('Done', 0, 0);
    IF @LogToTable = 1
        INSERT  INTO AzureSQLMaintenanceLog
        VALUES (@OperationTime, NULL, NULL, sysdatetime(), sysdatetime(), 'End of operation');
    IF @ScriptHasAnError = 1
        RAISERROR ('Script has errors - please review the log.', 16, 1);
END

GO

