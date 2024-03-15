using AutoMapper;
using Crud.Tests.Entities;
using Crud.Tests.Mapping;
using Juulsgaard.Crud.Models;
using Microsoft.EntityFrameworkCore;

namespace Crud.Tests.Database;

public class DatabaseContext : DbContext, ICrudDbContext, ITestDbContext
{

	public IMapper Mapper { get; set; }

	public DatabaseContext(DbContextOptions options) : base(options)
	{
		Mapper = new MapperConfiguration(cfg => { cfg.AddProfile<DogMapping>(); }).CreateMapper();
	}
	
	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		modelBuilder.Entity<OwnerEntity>().HasKey(x => x.Id);

		modelBuilder.Entity<DogEntity>().HasKey(x => x.Id);

		modelBuilder.Entity<DogEntity>()
		   .HasOne(x => x.Owner)
		   .WithMany(x => x.Dogs)
		   .HasForeignKey(x => x.OwnerId)
		   .OnDelete(DeleteBehavior.Restrict);
	}

	public DbSet<OwnerEntity> Owners => Set<OwnerEntity>();

	public DbSet<DogEntity> Dogs => Set<DogEntity>();
}