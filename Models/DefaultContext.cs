using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Vacation24.Models {
    public interface IDbContext {
        int SaveChanges();
        EntityEntry<TEntity> Entry<TEntity>(TEntity entity) where TEntity : class;
        EntityEntry Entry(object entity);
    }

    public class DefaultContext: DbContext, IDbContext, IUniqueViewsContext, IPaymentServicesContext, IOrdersContext, INotesContext
    {
        //Places db set
        public DbSet<Place> Places { get; set; }
        public DbSet<Price> Prices { get; set; }
        public DbSet<Photo> Photos { get; set; }
        public DbSet<Video> Videos { get; set; }
        public DbSet<Comment> Comments { get; set; }

        //Users db sets
        public DbSet<Profile> Profiles { get; set; }
        public DbSet<PrevilegedContact> PrevilegedContacts { get; set; }

        //News dp set
        public DbSet<News> News { get; set; }

        //Favorites
        public DbSet<Favorite> Favorites { get; set; }

        //UniqueViews
        public DbSet<UniqueView> UniqueViews { get; set; }

        //Payment
        public DbSet<Service> Services { get; set; }
        public DbSet<Subscription> Subscriptions { get; set; }
        public DbSet<SpecialOffer> SpecialOffers { get; set; }

        public DbSet<Order> Orders { get; set; }

        public DbSet<LoggedUserIp> UserIps { get; set; }

        //Notes
        public DbSet<ProfileNote> ProfileNotes { get; set; }
    }

    public interface IUniqueViewsContext: IDbContext
    {
        DbSet<UniqueView> UniqueViews { get; set; }
    }

    public interface IPaymentServicesContext: IDbContext
    {
        DbSet<Service> Services { get; set; }
        DbSet<Subscription> Subscriptions { get; set; }
        DbSet<SpecialOffer> SpecialOffers { get; set; }
    }

    public interface IOrdersContext: IDbContext
    {
        DbSet<Profile> Profiles { get; set; }
        DbSet<Service> Services { get; set; }
        DbSet<Order> Orders { get; set; }
    }

    public interface INotesContext: IDbContext
    {
        DbSet<ProfileNote> ProfileNotes { get; set; }
    }
}