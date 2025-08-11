using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace WebAppRnocDataCenterAPIGeneral.WebAPIASPModels.NSN.SleepingCell;

public partial class ConnectionsInformationSleepingCellDbContext : DbContext
{
    public ConnectionsInformationSleepingCellDbContext()
    {
    }

    public ConnectionsInformationSleepingCellDbContext(DbContextOptions<ConnectionsInformationSleepingCellDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Objtable4gkpireport> Objtable4gkpireports { get; set; }

    public virtual DbSet<Objtable4gkpireportresult> Objtable4gkpireportresults { get; set; }

    public virtual DbSet<Objtable4gkpireportresultarchive> Objtable4gkpireportresultarchives { get; set; }

    public virtual DbSet<Objtableaccountssh> Objtableaccountsshes { get; set; }

    public virtual DbSet<Objtablefilterltekpireport> Objtablefilterltekpireports { get; set; }

    public virtual DbSet<ObjtablemrbtsInfor> ObjtablemrbtsInfors { get; set; }

    public virtual DbSet<Objtableresetsitecountlimit> Objtableresetsitecountlimits { get; set; }

    public virtual DbSet<Objtableresetsitesleepingcell> Objtableresetsitesleepingcells { get; set; }

    public virtual DbSet<Objtablescheduler> Objtableschedulers { get; set; }

    public virtual DbSet<Outlook> Outlooks { get; set; }

    public virtual DbSet<Tablefilepath> Tablefilepaths { get; set; }

    public virtual DbSet<VKpiArchiveSummary> VKpiArchiveSummaries { get; set; }

    public virtual DbSet<VKpiResultToday> VKpiResultTodays { get; set; }

    

    
    

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Objtable4gkpireport>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("objtable4gkpireport_pkey");

            entity.ToTable("objtable4gkpireport", "system_nsn_sleepingcell");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CellAvail)
                .HasPrecision(8, 3)
                .HasColumnName("cell_avail");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_at");
            entity.Property(e => e.DnMrbtsSite)
                .HasMaxLength(200)
                .HasColumnName("dn_mrbts_site");
            entity.Property(e => e.LnbtsName)
                .HasMaxLength(100)
                .HasColumnName("lnbts_name");
            entity.Property(e => e.LncelName)
                .HasMaxLength(100)
                .HasColumnName("lncel_name");
            entity.Property(e => e.MaxPdcpDl).HasColumnName("max_pdcp_dl");
            entity.Property(e => e.MaxPdcpUl).HasColumnName("max_pdcp_ul");
            entity.Property(e => e.MaxUes).HasColumnName("max_ues");
            entity.Property(e => e.MrbtsName)
                .HasMaxLength(100)
                .HasColumnName("mrbts_name");
            entity.Property(e => e.PdcpVolumeDl)
                .HasPrecision(12, 3)
                .HasColumnName("pdcp_volume_dl");
            entity.Property(e => e.PdcpVolumeUl)
                .HasPrecision(12, 3)
                .HasColumnName("pdcp_volume_ul");
            entity.Property(e => e.PeriodStartTime)
                .HasMaxLength(20)
                .HasColumnName("period_start_time");
        });

        modelBuilder.Entity<Objtable4gkpireportresult>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("objtable4gkpireportresult_pkey");

            entity.ToTable("objtable4gkpireportresult", "system_nsn_sleepingcell", tb => tb.HasComment("Bảng kết quả reset ngày hôm nay - cùng structure với archive table"));

            entity.HasIndex(e => new { e.DataDate, e.Province, e.Vendor, e.LncelName }, "idx_result_aggregation");

            entity.HasIndex(e => e.LnbtsName, "idx_result_bts_name");

            entity.HasIndex(e => e.LncelName, "idx_result_cell_name");

            entity.HasIndex(e => e.DataDate, "idx_result_data_date");

            entity.HasIndex(e => new { e.DataDate, e.LncelName }, "idx_result_date_cell");

            entity.HasIndex(e => new { e.DataDate, e.Province }, "idx_result_date_province");

            entity.HasIndex(e => e.District, "idx_result_district");

            entity.HasIndex(e => e.Province, "idx_result_province");

            entity.HasIndex(e => new { e.Province, e.DataYear, e.DataMonth }, "idx_result_province_month");

            entity.HasIndex(e => new { e.DataYear, e.DataQuarter }, "idx_result_quarter");

            entity.HasIndex(e => e.Region, "idx_result_region");

            entity.HasIndex(e => e.MrbtsName, "idx_result_site_name");

            entity.HasIndex(e => e.Vendor, "idx_result_vendor");

            entity.HasIndex(e => new { e.Vendor, e.DataDate }, "idx_result_vendor_date");

            entity.HasIndex(e => new { e.DataYear, e.DataMonth }, "idx_result_year_month");

            entity.HasIndex(e => new { e.OriginalId, e.DataDate }, "uk_result_unique").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.ArchivedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasComment("Thời gian xử lý (processing time)")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("archived_at");
            entity.Property(e => e.ArchivedBy)
                .HasMaxLength(100)
                .HasDefaultValueSql("'SYSTEM_AUTO'::character varying")
                .HasColumnName("archived_by");
            entity.Property(e => e.CellAvail)
                .HasPrecision(8, 3)
                .HasColumnName("cell_avail");
            entity.Property(e => e.DataDate)
                .HasComment("Ngày của data (thường là CURRENT_DATE)")
                .HasColumnName("data_date");
            entity.Property(e => e.DataDay)
                .HasComputedColumnSql("EXTRACT(day FROM data_date)", true)
                .HasColumnName("data_day");
            entity.Property(e => e.DataMonth)
                .HasComputedColumnSql("EXTRACT(month FROM data_date)", true)
                .HasColumnName("data_month");
            entity.Property(e => e.DataQuarter)
                .HasComputedColumnSql("EXTRACT(quarter FROM data_date)", true)
                .HasColumnName("data_quarter");
            entity.Property(e => e.DataWeek)
                .HasComputedColumnSql("EXTRACT(week FROM data_date)", true)
                .HasColumnName("data_week");
            entity.Property(e => e.DataYear)
                .HasComputedColumnSql("EXTRACT(year FROM data_date)", true)
                .HasColumnName("data_year");
            entity.Property(e => e.District)
                .HasMaxLength(50)
                .HasColumnName("district");
            entity.Property(e => e.DnMrbtsSite)
                .HasMaxLength(200)
                .HasColumnName("dn_mrbts_site");
            entity.Property(e => e.LnbtsName)
                .HasMaxLength(100)
                .HasColumnName("lnbts_name");
            entity.Property(e => e.LncelName)
                .HasMaxLength(100)
                .HasColumnName("lncel_name");
            entity.Property(e => e.MaxPdcpDl).HasColumnName("max_pdcp_dl");
            entity.Property(e => e.MaxPdcpUl).HasColumnName("max_pdcp_ul");
            entity.Property(e => e.MaxUes).HasColumnName("max_ues");
            entity.Property(e => e.MrbtsName)
                .HasMaxLength(100)
                .HasColumnName("mrbts_name");
            entity.Property(e => e.OriginalCreatedAt)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("original_created_at");
            entity.Property(e => e.OriginalId).HasColumnName("original_id");
            entity.Property(e => e.PdcpVolumeDl)
                .HasPrecision(12, 3)
                .HasColumnName("pdcp_volume_dl");
            entity.Property(e => e.PdcpVolumeUl)
                .HasPrecision(12, 3)
                .HasColumnName("pdcp_volume_ul");
            entity.Property(e => e.PeriodStartTime)
                .HasMaxLength(20)
                .HasColumnName("period_start_time");
            entity.Property(e => e.Province)
                .HasMaxLength(50)
                .HasColumnName("province");
            entity.Property(e => e.Region)
                .HasMaxLength(20)
                .HasColumnName("region");
            entity.Property(e => e.Vendor)
                .HasMaxLength(20)
                .HasColumnName("vendor");
        });

        modelBuilder.Entity<Objtable4gkpireportresultarchive>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("objtable4gkpireportresultarchive_pkey");

            entity.ToTable("objtable4gkpireportresultarchive", "system_nsn_sleepingcell", tb => tb.HasComment("Bảng archive KPI data với support query linh hoạt theo ngày/tháng/địa điểm/tên"));

            entity.HasIndex(e => new { e.DataDate, e.Province, e.Vendor, e.LncelName }, "idx_archive_aggregation");

            entity.HasIndex(e => e.LnbtsName, "idx_archive_bts_name");

            entity.HasIndex(e => e.LncelName, "idx_archive_cell_name");

            entity.HasIndex(e => e.DataDate, "idx_archive_data_date");

            entity.HasIndex(e => new { e.DataDate, e.LncelName }, "idx_archive_date_cell");

            entity.HasIndex(e => new { e.DataDate, e.Province }, "idx_archive_date_province");

            entity.HasIndex(e => e.District, "idx_archive_district");

            entity.HasIndex(e => e.Province, "idx_archive_province");

            entity.HasIndex(e => new { e.Province, e.DataYear, e.DataMonth }, "idx_archive_province_month");

            entity.HasIndex(e => new { e.DataYear, e.DataQuarter }, "idx_archive_quarter");

            entity.HasIndex(e => e.Region, "idx_archive_region");

            entity.HasIndex(e => e.MrbtsName, "idx_archive_site_name");

            entity.HasIndex(e => e.Vendor, "idx_archive_vendor");

            entity.HasIndex(e => new { e.Vendor, e.DataDate }, "idx_archive_vendor_date");

            entity.HasIndex(e => new { e.DataYear, e.DataMonth }, "idx_archive_year_month");

            entity.HasIndex(e => new { e.OriginalId, e.DataDate }, "uk_archive_unique").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.ArchivedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("archived_at");
            entity.Property(e => e.ArchivedBy)
                .HasMaxLength(100)
                .HasDefaultValueSql("'SYSTEM_AUTO'::character varying")
                .HasColumnName("archived_by");
            entity.Property(e => e.CellAvail)
                .HasPrecision(8, 3)
                .HasColumnName("cell_avail");
            entity.Property(e => e.DataDate)
                .HasComment("Ngày của data KPI (để query theo thời gian)")
                .HasColumnName("data_date");
            entity.Property(e => e.DataDay)
                .HasComputedColumnSql("EXTRACT(day FROM data_date)", true)
                .HasColumnName("data_day");
            entity.Property(e => e.DataMonth)
                .HasComputedColumnSql("EXTRACT(month FROM data_date)", true)
                .HasColumnName("data_month");
            entity.Property(e => e.DataQuarter)
                .HasComputedColumnSql("EXTRACT(quarter FROM data_date)", true)
                .HasColumnName("data_quarter");
            entity.Property(e => e.DataWeek)
                .HasComputedColumnSql("EXTRACT(week FROM data_date)", true)
                .HasColumnName("data_week");
            entity.Property(e => e.DataYear)
                .HasComputedColumnSql("EXTRACT(year FROM data_date)", true)
                .HasColumnName("data_year");
            entity.Property(e => e.District)
                .HasMaxLength(50)
                .HasColumnName("district");
            entity.Property(e => e.DnMrbtsSite)
                .HasMaxLength(200)
                .HasColumnName("dn_mrbts_site");
            entity.Property(e => e.LnbtsName)
                .HasMaxLength(100)
                .HasColumnName("lnbts_name");
            entity.Property(e => e.LncelName)
                .HasMaxLength(100)
                .HasColumnName("lncel_name");
            entity.Property(e => e.MaxPdcpDl).HasColumnName("max_pdcp_dl");
            entity.Property(e => e.MaxPdcpUl).HasColumnName("max_pdcp_ul");
            entity.Property(e => e.MaxUes).HasColumnName("max_ues");
            entity.Property(e => e.MrbtsName)
                .HasMaxLength(100)
                .HasColumnName("mrbts_name");
            entity.Property(e => e.OriginalCreatedAt)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("original_created_at");
            entity.Property(e => e.OriginalId).HasColumnName("original_id");
            entity.Property(e => e.PdcpVolumeDl)
                .HasPrecision(12, 3)
                .HasColumnName("pdcp_volume_dl");
            entity.Property(e => e.PdcpVolumeUl)
                .HasPrecision(12, 3)
                .HasColumnName("pdcp_volume_ul");
            entity.Property(e => e.PeriodStartTime)
                .HasMaxLength(20)
                .HasColumnName("period_start_time");
            entity.Property(e => e.Province)
                .HasMaxLength(50)
                .HasComment("Tỉnh/Thành phố (extract từ site name)")
                .HasColumnName("province");
            entity.Property(e => e.Region)
                .HasMaxLength(20)
                .HasComment("Miền: North/Central/South")
                .HasColumnName("region");
            entity.Property(e => e.Vendor)
                .HasMaxLength(20)
                .HasColumnName("vendor");
        });

        modelBuilder.Entity<Objtableaccountssh>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("objtableaccountssh_pkey");

            entity.ToTable("objtableaccountssh", "system_nsn_sleepingcell");

            entity.Property(e => e.Id)
                .UseIdentityAlwaysColumn()
                .HasColumnName("id");
            entity.Property(e => e.Active).HasColumnName("active");
            entity.Property(e => e.Password)
                .HasColumnType("character varying")
                .HasColumnName("password");
            entity.Property(e => e.Port).HasColumnName("port");
            entity.Property(e => e.Sttaccountssh).HasColumnName("sttaccountssh");
            entity.Property(e => e.System)
                .HasColumnType("character varying")
                .HasColumnName("system");
            entity.Property(e => e.Usename)
                .HasColumnType("character varying")
                .HasColumnName("usename");
        });

        modelBuilder.Entity<Objtablefilterltekpireport>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("objtablefilterltekpireport_pkey");

            entity.ToTable("objtablefilterltekpireport", "system_nsn_sleepingcell");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CellAvail)
                .HasPrecision(5, 2)
                .HasColumnName("cell_avail");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_at");
            entity.Property(e => e.DnMrbtsSite).HasColumnName("dn_mrbts_site");
            entity.Property(e => e.LnbtsName)
                .HasMaxLength(50)
                .HasColumnName("lnbts_name");
            entity.Property(e => e.LncelName)
                .HasMaxLength(50)
                .HasColumnName("lncel_name");
            entity.Property(e => e.MaxPdcpDl)
                .HasPrecision(12, 3)
                .HasColumnName("max_pdcp_dl");
            entity.Property(e => e.MaxPdcpUl)
                .HasPrecision(12, 3)
                .HasColumnName("max_pdcp_ul");
            entity.Property(e => e.MaxUes).HasColumnName("max_ues");
            entity.Property(e => e.MrbtsName)
                .HasMaxLength(100)
                .HasColumnName("mrbts_name");
            entity.Property(e => e.PdcpVolumeDl)
                .HasPrecision(12, 3)
                .HasColumnName("pdcp_volume_dl");
            entity.Property(e => e.PdcpVolumeUl)
                .HasPrecision(12, 3)
                .HasColumnName("pdcp_volume_ul");
            entity.Property(e => e.PeriodStartTime)
                .HasMaxLength(20)
                .HasColumnName("period_start_time");
        });

        modelBuilder.Entity<ObjtablemrbtsInfor>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("objtablemrbts_infor_pkey");

            entity.ToTable("objtablemrbts_infor", "system_nsn_sleepingcell");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Blacklist).HasColumnName("blacklist");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_at");
            entity.Property(e => e.Enodebname)
                .HasMaxLength(100)
                .HasColumnName("enodebname");
            entity.Property(e => e.MrbtsId)
                .HasPrecision(12)
                .HasColumnName("mrbts_id");
            entity.Property(e => e.Mrbtsname)
                .HasMaxLength(100)
                .HasColumnName("mrbtsname");
            entity.Property(e => e.Note).HasColumnName("note");
            entity.Property(e => e.Oam)
                .HasMaxLength(50)
                .HasColumnName("oam");
            entity.Property(e => e.Reset).HasColumnName("reset");
            entity.Property(e => e.Stt).HasColumnName("stt");
            entity.Property(e => e.Vendor)
                .HasMaxLength(20)
                .HasDefaultValueSql("'NSN'::character varying")
                .HasColumnName("vendor");
        });

        modelBuilder.Entity<Objtableresetsitecountlimit>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("objtable_reset_limits_pkey");

            entity.ToTable("objtableresetsitecountlimits", "system_nsn_sleepingcell", tb => tb.HasComment("Bảng giới hạn số sites được reset mỗi ngày để bảo vệ hệ thống"));

            entity.HasIndex(e => e.LimitDate, "idx_reset_limits_date");

            entity.HasIndex(e => e.AutoResetEnabled, "idx_reset_limits_enabled");

            entity.HasIndex(e => e.LimitDate, "uk_limit_date").IsUnique();

            entity.Property(e => e.Id)
                .HasDefaultValueSql("nextval('system_nsn_sleepingcell.objtable_reset_limits_id_seq'::regclass)")
                .HasColumnName("id");
            entity.Property(e => e.AutoResetEnabled)
                .HasDefaultValue(true)
                .HasComment("Bật/tắt chức năng auto reset")
                .HasColumnName("auto_reset_enabled");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_at");
            entity.Property(e => e.EmergencyOverride)
                .HasDefaultValue(false)
                .HasComment("Cho phép vượt limit trong trường hợp khẩn cấp")
                .HasColumnName("emergency_override");
            entity.Property(e => e.EricssonSitesReset)
                .HasDefaultValue(0)
                .HasColumnName("ericsson_sites_reset");
            entity.Property(e => e.HuaweiSitesReset)
                .HasDefaultValue(0)
                .HasColumnName("huawei_sites_reset");
            entity.Property(e => e.LastResetTime)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("last_reset_time");
            entity.Property(e => e.LimitDate)
                .HasDefaultValueSql("CURRENT_DATE")
                .HasColumnName("limit_date");
            entity.Property(e => e.MaxEricssonSites)
                .HasDefaultValue(15)
                .HasColumnName("max_ericsson_sites");
            entity.Property(e => e.MaxHuaweiSites)
                .HasDefaultValue(15)
                .HasColumnName("max_huawei_sites");
            entity.Property(e => e.MaxNsnSites)
                .HasDefaultValue(20)
                .HasColumnName("max_nsn_sites");
            entity.Property(e => e.MaxSitesPerDay)
                .HasDefaultValue(50)
                .HasComment("Tối đa số sites được reset trong 1 ngày")
                .HasColumnName("max_sites_per_day");
            entity.Property(e => e.NsnSitesReset)
                .HasDefaultValue(0)
                .HasColumnName("nsn_sites_reset");
            entity.Property(e => e.SitesResetToday)
                .HasDefaultValue(0)
                .HasComment("Số sites đã reset hôm nay")
                .HasColumnName("sites_reset_today");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("updated_at");
            entity.Property(e => e.UpdatedBy)
                .HasMaxLength(100)
                .HasColumnName("updated_by");
        });

        modelBuilder.Entity<Objtableresetsitesleepingcell>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("objtableresetsitesleepingcell_pkey");

            entity.ToTable("objtableresetsitesleepingcell", "system_nsn_sleepingcell");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CellAvail)
                .HasPrecision(5, 2)
                .HasColumnName("cell_avail");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_at");
            entity.Property(e => e.DnMrbtsSite).HasColumnName("dn_mrbts_site");
            entity.Property(e => e.LnbtsName)
                .HasMaxLength(50)
                .HasColumnName("lnbts_name");
            entity.Property(e => e.LncelName)
                .HasMaxLength(50)
                .HasColumnName("lncel_name");
            entity.Property(e => e.MaxPdcpDl)
                .HasPrecision(12, 3)
                .HasColumnName("max_pdcp_dl");
            entity.Property(e => e.MaxPdcpUl)
                .HasPrecision(12, 3)
                .HasColumnName("max_pdcp_ul");
            entity.Property(e => e.MaxUes).HasColumnName("max_ues");
            entity.Property(e => e.MrbtsName)
                .HasMaxLength(100)
                .HasColumnName("mrbts_name");
            entity.Property(e => e.PdcpVolumeDl)
                .HasPrecision(12, 3)
                .HasColumnName("pdcp_volume_dl");
            entity.Property(e => e.PdcpVolumeUl)
                .HasPrecision(12, 3)
                .HasColumnName("pdcp_volume_ul");
            entity.Property(e => e.PeriodStartTime)
                .HasMaxLength(20)
                .HasColumnName("period_start_time");
        });

        modelBuilder.Entity<Objtablescheduler>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("objtablescheduler_pkey");

            entity.ToTable("objtablescheduler", "system_nsn_sleepingcell");

            entity.Property(e => e.Id)
                .UseIdentityAlwaysColumn()
                .HasColumnName("id");
            entity.Property(e => e.Active).HasColumnName("active");
            entity.Property(e => e.Endtime)
                .HasColumnType("character varying")
                .HasColumnName("endtime");
            entity.Property(e => e.Starttime)
                .HasColumnType("character varying")
                .HasColumnName("starttime");
            entity.Property(e => e.Sttschedulertime).HasColumnName("sttschedulertime");
        });

        modelBuilder.Entity<Outlook>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("sleepingcell_outlook_pkey");

            entity.ToTable("outlook", "system_nsn_sleepingcell");

            entity.Property(e => e.Id)
                .UseIdentityAlwaysColumn()
                .HasColumnName("ID");
            entity.Property(e => e.Email).HasMaxLength(150);
            entity.Property(e => e.Password)
                .HasMaxLength(255)
                .HasColumnName("password");
            entity.Property(e => e.SttEmail).HasColumnName("sttEmail");
        });

        modelBuilder.Entity<Tablefilepath>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("filepath_pkey");

            entity.ToTable("tablefilepath", "system_nsn_sleepingcell");

            entity.Property(e => e.Id)
                .UseIdentityAlwaysColumn()
                .HasColumnName("id");
            entity.Property(e => e.Active).HasColumnName("active");
            entity.Property(e => e.Filepath)
                .HasColumnType("character varying")
                .HasColumnName("filepath");
            entity.Property(e => e.Host)
                .HasColumnType("character varying")
                .HasColumnName("host");
            entity.Property(e => e.Oss)
                .HasColumnType("character varying")
                .HasColumnName("oss");
            entity.Property(e => e.Password)
                .HasMaxLength(255)
                .HasColumnName("password");
            entity.Property(e => e.Port).HasColumnName("port");
            entity.Property(e => e.Protocol)
                .HasColumnType("character varying")
                .HasColumnName("protocol");
            entity.Property(e => e.SttFilePath).HasColumnName("sttFilePath");
            entity.Property(e => e.Username)
                .HasColumnType("character varying")
                .HasColumnName("username");
        });

        modelBuilder.Entity<VKpiArchiveSummary>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("v_kpi_archive_summary", "system_nsn_sleepingcell");

            entity.Property(e => e.AvgAvailability).HasColumnName("avg_availability");
            entity.Property(e => e.AvgDlTraffic).HasColumnName("avg_dl_traffic");
            entity.Property(e => e.AvgUlTraffic).HasColumnName("avg_ul_traffic");
            entity.Property(e => e.DataDate).HasColumnName("data_date");
            entity.Property(e => e.DataMonth).HasColumnName("data_month");
            entity.Property(e => e.DataQuarter).HasColumnName("data_quarter");
            entity.Property(e => e.DataYear).HasColumnName("data_year");
            entity.Property(e => e.District)
                .HasMaxLength(50)
                .HasColumnName("district");
            entity.Property(e => e.Province)
                .HasMaxLength(50)
                .HasColumnName("province");
            entity.Property(e => e.Region)
                .HasMaxLength(20)
                .HasColumnName("region");
            entity.Property(e => e.SleepingCells).HasColumnName("sleeping_cells");
            entity.Property(e => e.TotalCells).HasColumnName("total_cells");
            entity.Property(e => e.TotalDlTraffic).HasColumnName("total_dl_traffic");
            entity.Property(e => e.TotalUlTraffic).HasColumnName("total_ul_traffic");
            entity.Property(e => e.Vendor)
                .HasMaxLength(20)
                .HasColumnName("vendor");
        });

        modelBuilder.Entity<VKpiResultToday>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("v_kpi_result_today", "system_nsn_sleepingcell");

            entity.Property(e => e.AvgAvailability).HasColumnName("avg_availability");
            entity.Property(e => e.AvgDlTraffic).HasColumnName("avg_dl_traffic");
            entity.Property(e => e.AvgUlTraffic).HasColumnName("avg_ul_traffic");
            entity.Property(e => e.DataDate).HasColumnName("data_date");
            entity.Property(e => e.DataMonth).HasColumnName("data_month");
            entity.Property(e => e.DataQuarter).HasColumnName("data_quarter");
            entity.Property(e => e.DataYear).HasColumnName("data_year");
            entity.Property(e => e.District)
                .HasMaxLength(50)
                .HasColumnName("district");
            entity.Property(e => e.Province)
                .HasMaxLength(50)
                .HasColumnName("province");
            entity.Property(e => e.Region)
                .HasMaxLength(20)
                .HasColumnName("region");
            entity.Property(e => e.SleepingCells).HasColumnName("sleeping_cells");
            entity.Property(e => e.TotalCells).HasColumnName("total_cells");
            entity.Property(e => e.TotalDlTraffic).HasColumnName("total_dl_traffic");
            entity.Property(e => e.TotalUlTraffic).HasColumnName("total_ul_traffic");
            entity.Property(e => e.Vendor)
                .HasMaxLength(20)
                .HasColumnName("vendor");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);


}
