﻿using System;

namespace SJP.Schematic.SqlServer.Query
{
    internal class ServerProperties2008
    {
        /// <summary>
        /// <para>Version of the Microsoft .NET Framework common language runtime (CLR) that was used while building the instance of SQL Server.</para>
        /// <para><code>NULL</code> means input is not valid, an error, or not applicable</para>
        /// </summary>
        public string? BuildClrVersion { get; set; }

        /// <summary>
        /// <para>Name of the default collation for the server.</para>
        /// <para><code>NULL</code> means input is not valid, or an error.</para>
        /// </summary>
        public string? Collation { get; set; }

        /// <summary>
        /// ID of the SQL Server collation.
        /// </summary>
        public int CollationID { get; set; }

        /// <summary>
        /// Windows comparison style of the collation.
        /// </summary>
        public int ComparisonStyle { get; set; }

        /// <summary>
        /// <para>NetBIOS name of the local computer on which the instance of SQL Server is currently running.</para>
        /// <para>For a clustered instance of SQL Server on a failover cluster, this value changes as the instance of SQL Server fails over to other nodes in the failover cluster.</para>
        /// <para>On a stand-alone instance of SQL Server, this value remains constant and returns the same value as the MachineName property.</para>
        /// <para>Note: If the instance of SQL Server is in a failover cluster and you want to obtain the name of the failover clustered instance, use the MachineName property.</para>
        /// <para><code>NULL</code> means input is not valid, an error, or not applicable</para>
        /// </summary>
        public string? ComputerNamePhysicalNetBIOS { get; set; }

        /// <summary>
        /// <para>Installed product edition of the instance of SQL Server. Use the value of this property to determine the features and the limits, such as Compute Capacity Limits by Edition of SQL Server. 64-bit versions of the Database Engine append (64-bit) to the version.</para>
        /// Returns: <list type="bullet">
        /// <item>Enterprise Edition</item>
        /// <item>Enterprise Edition: Core-based Licensing</item>
        /// <item>Enterprise Evaluation Edition</item>
        /// <item>Business Intelligence Edition</item>
        /// <item>Developer Edition</item>
        /// <item>Express Edition</item>
        /// <item>Express Edition with Advanced Services</item>
        /// <item>Standard Edition</item>
        /// <item>Web Edition</item>
        /// <item>'SQL Azure' indicates SQL Database or SQL Data Warehouse</item>
        /// </list>
        /// </summary>
        public string Edition { get; set; } = default!;

        /// <summary>
        /// <para>Installed product edition of the instance of SQL Server. Use the value of this property to determine the features and the limits, such as Compute Capacity Limits by Edition of SQL Server. 64-bit versions of the Database Engine append (64-bit) to the version.</para>
        /// Returns: <list type="bullet">
        /// <item>1804890536 = Enterprise</item>
        /// <item>1872460670 = Enterprise Edition: Core-based Licensing</item>
        /// <item>610778273 = Enterprise Evaluation</item>
        /// <item>284895786 = Business Intelligence</item>
        /// <item>-2117995310 = Developer</item>
        /// <item>-1592396055 = Express</item>
        /// <item>-133711905= Express with Advanced Services</item>
        /// <item>-1534726760 = Standard</item>
        /// <item>1293598313 = Web</item>
        /// <item>1674378470 = SQL Database or SQL Data Warehouse</item>
        /// </list>
        /// </summary>
        public long EditionID { get; set; }

        /// <summary>
        /// <para>Database Engine edition of the instance of SQL Server installed on the server.</para>
        /// Returns: <list type="bullet">
        /// <item>1 = Personal or Desktop Engine (Not available in SQL Server 2005 (9.x) and later versions.)</item>
        /// <item>2 = Standard (This is returned for Standard, Web, and Business Intelligence.)</item>
        /// <item>3 = Enterprise (This is returned for Evaluation, Developer, and both Enterprise editions.)</item>
        /// <item>4 = Express (This is returned for Express, Express with Tools and Express with Advanced Services)</item>
        /// <item>5 = SQL Database</item>
        /// <item>6 = SQL Data Warehouse</item>
        /// <item>8 = Managed Instance</item>
        /// </list>
        /// </summary>
        public int EngineEdition { get; set; }

        /// <summary>
        /// <para>Name of the instance to which the user is connected.</para>
        /// <para>Returns <code>NULL</code> if the instance name is the default instance, if the input is not valid, or error.</para>
        /// </summary>
        public string? InstanceName { get; set; }

        /// <summary>
        /// Returns <code>true</code> if the Advanced Analytics feature was installed during setup; <code>false</code> if Advanced Analytics was not installed.
        /// </summary>
        public bool IsAdvancedAnalyticsInstalled { get; set; }

        /// <summary>
        /// <para>Server instance is configured in a failover cluster.</para>
        /// <para>Returns <code>NULL</code> if the input is not valid, an error, or not applicable.</para>
        /// </summary>
        public int? IsClustered { get; set; }

        /// <summary>
        /// <para>The full-text and semantic indexing components are installed on the current instance of SQL Server.</para>
        /// <para>Returns <code>NULL</code> if the input is not valid, an error, or not applicable.</para>
        /// </summary>
        public int? IsFullTextInstalled { get; set; }

        /// <summary>
        /// <para>Server is in integrated security mode.</para>
        /// <para>Returns <code>NULL</code> if the input is not valid, an error, or not applicable.</para>
        /// </summary>
        public int? IsIntegratedSecurityOnly { get; set; }

        /// <summary>
        /// <para>Server is in single-user mode.</para>
        /// <para>Returns <code>NULL</code> if the input is not valid, an error, or not applicable.</para>
        /// </summary>
        public int? IsSingleUser { get; set; }

        /// <summary>
        /// Windows locale identifier (LCID) of the collation.
        /// </summary>
        public int LCID { get; set; }

        /// <summary>
        /// Unused. License information is not preserved or maintained by the SQL Server product. Always returns DISABLED.
        /// </summary>
        public string? LicenseType { get; set; }

        /// <summary>
        /// <para>Windows computer name on which the server instance is running.</para>
        /// <para>For a clustered instance, an instance of SQL Server running on a virtual server on Microsoft Cluster Service, it returns the name of the virtual server.</para>
        /// <para>Returns <code>NULL</code> if the input is not valid, an error, or not applicable.</para>
        /// </summary>
        public string? MachineName { get; set; }

        /// <summary>
        /// Unused. License information is not preserved or maintained by the SQL Server product. Always returns NULL.
        /// </summary>
        public int? NumLicenses { get; set; }

        /// <summary>
        /// <para>Process ID of the SQL Server service. ProcessID is useful in identifying which Sqlservr.exe belongs to this instance.</para>
        /// <para>Returns <code>NULL</code> if the input is not valid, an error, or not applicable.</para>
        /// </summary>
        public int? ProcessID { get; set; }

        /// <summary>
        /// <para>Level of the version of the instance of SQL Server.</para>
        /// Returns: <list type="bullet">
        /// <item>'RTM' = Original release version</item>
        /// <item>'SPn' = Service pack version</item>
        /// <item>'CTPn' = Community Technology Preview version</item>
        /// </list>
        /// </summary>
        public string? ProductLevel { get; set; }

        /// <summary>
        /// Version of the instance of SQL Server, in the form of 'major.minor.build.revision'.
        /// </summary>
        public string? ProductVersion { get; set; }

        /// <summary>
        /// Returns the date and time that the Resource database was last updated.
        /// </summary>
        public DateTime ResourceLastUpdateDateTime { get; set; }

        /// <summary>
        /// Returns the version Resource database.
        /// </summary>
        public string? ResourceVersion { get; set; }

        /// <summary>
        /// <para>Both the Windows server and instance information associated with a specified instance of SQL Server.</para>
        /// <para>Returns <code>NULL</code> if the input is not valid, or an error.</para>
        /// </summary>
        public string? ServerName { get; set; }

        /// <summary>
        /// The SQL character set ID from the collation ID.
        /// </summary>
        public int SqlCharSet { get; set; }

        /// <summary>
        /// The SQL character set name from the collation.
        /// </summary>
        public string? SqlCharSetName { get; set; }

        /// <summary>
        /// The SQL sort order ID from the collation.
        /// </summary>
        public int SqlSortOrder { get; set; }

        /// <summary>
        /// The SQL sort order name from the collation.
        /// </summary>
        public string? SqlSortOrderName { get; set; }

        /// <summary>
        /// <para>The name of the share used by FILESTREAM.</para>
        /// <para>Returns <code>NULL</code> if the input is not valid, an error, or not applicable.</para>
        /// </summary>
        public string? FilestreamShareName { get; set; }

        /// <summary>
        /// The configured level of FILESTREAM access.
        /// </summary>
        public int FilestreamConfiguredLevel { get; set; }

        /// <summary>
        /// The effective level of FILESTREAM access. This value can be different than the FilestreamConfiguredLevel if the level has changed and either an instance restart or a computer restart is pending.
        /// </summary>
        public int FilestreamEffectiveLevel { get; set; }
    }
}
