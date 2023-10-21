using Booky.DataAccess.Data;
using Booky.DataAccess.Repository.IRepository;
using Booky.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Booky.DataAccess.Repository
{
    public class ProductRepository: Repository<Product>, IProductRepository
    {
        private readonly ApplicationDbContext _db;

        public ProductRepository(ApplicationDbContext db): base(db) 
        { 
            _db = db;
        }

        public void Update(Product product)
        {
            var prodFromDB = _db.Products.FirstOrDefault(u=> u.Id == product.Id);
            if (prodFromDB != null)
            {
                prodFromDB.Title = product.Title;
                prodFromDB.ISBN = product.ISBN;
                prodFromDB.Price = product.Price;
                prodFromDB.Price50 = product.Price50;
                prodFromDB.ListPrice = product.ListPrice;
                prodFromDB.Price100 = product.Price100;
                prodFromDB.Description = product.Description;
                prodFromDB.CategoryId = product.CategoryId;
                prodFromDB.Author = product.Author;
                prodFromDB.ProductImages = product.ProductImages;
                //if(prodFromDB.ImageUrl != null)
                //{
                //    prodFromDB.ImageUrl = product.ImageUrl;
                //}
            }
        }
    }
}
