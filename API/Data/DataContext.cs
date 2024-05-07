using System;
using System.Collections.Generic;
using System.Text;
using API.DTOs;
using API.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Newtonsoft.Json;

namespace API.Data;

public partial class DataContext : DbContext
{
    public DataContext(DbContextOptions<DataContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Airline> Airlines { get; set; }

    public virtual DbSet<Airport> Airports { get; set; }

    public virtual DbSet<TailNumber> TailNumbers { get; set; }

    public virtual DbSet<Training> Training { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<UserPrediction> UserPredictions { get; set; }

    //     public override async Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
    //     {
    //         var addedEntities = ChangeTracker.Entries<Training>()
    //             .Where(e => e.State == EntityState.Added)
    //             .ToList();

    //         int result = await base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);

    //         var optionsBuilder = new DbContextOptionsBuilder<SqliteDataContext>();
    //         optionsBuilder.UseSqlite("Data source=apiCallTracker.db");

    //         using var sqliteDataContext = new SqliteDataContext(optionsBuilder.Options);
    //         // Get the current count from the tracker table
    //         var tracker = await sqliteDataContext.ApiCallTracker.FirstOrDefaultAsync();
    //         if (tracker == null)
    //         {
    //             tracker = new ApiCallTracker();
    //             await sqliteDataContext.ApiCallTracker.AddAsync(tracker);
    //         }

    //         tracker.Count += addedEntities.Count;

    //         // Check if a certain number of rows have been added
    //         if (tracker.Count >= 1000)
    //         {
    //             // Reset the count
    //             tracker.Count = 0;

    // #pragma warning disable 4014
    //             Task.Run(() => CallLongRunningApi())
    //                 .ContinueWith(t =>
    //                 {
    //                     if (t.IsFaulted)
    //                     {
    //                         // Log the exception
    //                         var exception = t.Exception;
    //                         // Handle the exception
    //                         using var loggerFactory = LoggerFactory.Create(builder =>
    //                         {
    //                             builder.AddConsole();
    //                         });
    //                         ILogger logger = loggerFactory.CreateLogger<Program>();
    //                         logger.LogError(exception, "An error occurred while calling the API.");
    //                     }
    //                 });
    // #pragma warning restore 4014
    //         }

    //         // Save the updated count
    //         await sqliteDataContext.SaveChangesAsync();

    //         return result;
    //     }

    private async Task CallLongRunningApi()
    {
        // Create a new instance of the SqliteDataContext
        using var client = new HttpClient();
        var response = await client.GetAsync("https://localhost:5001/api/model/train");
        var r = await response.Content.ReadAsStringAsync();
        using var loggerFactory = LoggerFactory.Create(builder =>
                {
                    builder.AddConsole();
                });
        ILogger logger = loggerFactory.CreateLogger<Program>();
        logger.LogInformation(r);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Airline>(entity =>
        {
            entity.HasKey(e => e.AirlineName).HasName("PK__Airlines__E6B3FF59897A8205");

            entity.Property(e => e.AirlineName)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.UniqueCarrier)
                .IsRequired()
                .HasMaxLength(255)
                .IsUnicode(false);
        });

        modelBuilder.Entity<Airport>(entity =>
        {
            entity.HasKey(e => e.AirportName).HasName("PK__Airports__34A7AB6E169FA657");

            entity.Property(e => e.AirportName)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("Airport_Name");
            entity.Property(e => e.AirportCode)
                .IsRequired()
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("Airport_Code");
        });

        modelBuilder.Entity<TailNumber>(entity =>
        {
            entity.HasKey(e => e.TailNum).HasName("PK__Tail_Num__90AA504830AF2851");

            entity.ToTable("Tail_Numbers");

            entity.Property(e => e.TailNum)
                .HasMaxLength(255)
                .IsUnicode(false);
        });

        modelBuilder.Entity<Training>(entity =>
        {
            entity.HasKey(e => new { e.ScheduledDepTime, e.AirlineName, e.TailNum, e.Date }).HasName("PK__Training__39DE829EBC998803");

            entity.Property(e => e.AirlineName)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.TailNum)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.DestAirport)
                .IsRequired()
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("Dest_Airport");
            entity.Property(e => e.OrgAirport)
                .IsRequired()
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("Org_Airport");

            entity.HasOne(d => d.AirlineNameNavigation).WithMany(p => p.Training)
                .HasForeignKey(d => d.AirlineName)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Training__Airlin__40F9A68C");

            entity.HasOne(d => d.DestAirportNavigation).WithMany(p => p.TrainingDestAirportNavigations)
                .HasForeignKey(d => d.DestAirport)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Training__Dest_A__43D61337");

            entity.HasOne(d => d.OrgAirportNavigation).WithMany(p => p.TrainingOrgAirportNavigations)
                .HasForeignKey(d => d.OrgAirport)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Training__Org_Ai__42E1EEFE");

            entity.HasOne(d => d.TailNumNavigation).WithMany(p => p.Training)
                .HasForeignKey(d => d.TailNum)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Training__TailNu__41EDCAC5");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Username).HasName("PK__Users__536C85E50075D7BD");

            entity.Property(e => e.Username)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.Password)
                .IsRequired()
                .HasMaxLength(255)
                .IsUnicode(false);
        });

        modelBuilder.Entity<UserPrediction>(entity =>
        {
            entity.HasKey(e => new { e.Username, e.ScheduledDepTime, e.Date }).HasName("PK__User_Pre__86FE9BD31D1DBDAC");

            entity.ToTable("User_Predictions");

            entity.Property(e => e.Username)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.AirlineName)
                .IsRequired()
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.DestAirport)
                .IsRequired()
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("Dest_Airport");
            entity.Property(e => e.OrgAirport)
                .IsRequired()
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("Org_Airport");
            entity.Property(e => e.TailNum)
                .IsRequired()
                .HasMaxLength(255)
                .IsUnicode(false);

            entity.HasOne(d => d.AirlineNameNavigation).WithMany(p => p.UserPredictions)
                .HasForeignKey(d => d.AirlineName)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__User_Pred__Airli__634EBE90");

            entity.HasOne(d => d.DestAirportNavigation).WithMany(p => p.UserPredictionDestAirportNavigations)
                .HasForeignKey(d => d.DestAirport)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__User_Pred__Dest___662B2B3B");

            entity.HasOne(d => d.OrgAirportNavigation).WithMany(p => p.UserPredictionOrgAirportNavigations)
                .HasForeignKey(d => d.OrgAirport)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__User_Pred__Org_A__65370702");

            entity.HasOne(d => d.TailNumNavigation).WithMany(p => p.UserPredictions)
                .HasForeignKey(d => d.TailNum)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__User_Pred__TailN__6442E2C9");

            entity.HasOne(d => d.UsernameNavigation).WithMany(p => p.UserPredictions)
                .HasForeignKey(d => d.Username)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__User_Pred__Usern__625A9A57");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
