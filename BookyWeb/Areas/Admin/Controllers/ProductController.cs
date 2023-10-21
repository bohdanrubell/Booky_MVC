 using Booky.DataAccess.Repository.IRepository;
using Booky.Models;
using Booky.Models.ViewModels;
using Booky.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Stripe;

namespace BookyWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]
    public class ProductController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWebHostEnvironment _webHostEnvironment;
        public ProductController(IUnitOfWork unitOfWork, IWebHostEnvironment webHostEnvironment)
        {
            _unitOfWork = unitOfWork;
            _webHostEnvironment = webHostEnvironment;
        }
        public IActionResult Index()
        {
            List<Booky.Models.Product> objCategoryList = _unitOfWork.Product.GetAll(includeProperties: "Category").ToList();
            return View(objCategoryList);
        }

        public IActionResult Upsert(int? id)
        {

            ProductVM productVM = new()
            {
                CategoryList = _unitOfWork.Category.GetAll().Select(u => new SelectListItem
                {
                    Text = u.Name,
                    Value = u.Id.ToString()
                }),
                Product = new Booky.Models.Product()
            };
            if (id == null || id == 0)
            {
                //create
                return View(productVM);
            }
            else
            {
                //update
                productVM.Product = _unitOfWork.Product.Get(u => u.Id == id, includeProperties:"ProductImages");
                return View(productVM);
            }

        }
        [HttpPost]
        public IActionResult Upsert(ProductVM productVM, List<IFormFile> files)
        {
            if (ModelState.IsValid)
            {
                if (ModelState.IsValid)
                {
                    if (productVM.Product.Id == 0)
                    {
                        _unitOfWork.Product.Add(productVM.Product);
                    }
                    else
                    {
                        _unitOfWork.Product.Update(productVM.Product);
                    }

                    _unitOfWork.Save();
                }
                string wwwRootPath = _webHostEnvironment.WebRootPath;
                if (files != null)
                {
                    foreach(IFormFile file in files)
                    {
                        string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                        string productPath = @"images\products\product-" + productVM.Product.Id;
                        string finalProductPart = Path.Combine(wwwRootPath, productPath);

                        if(!Directory.Exists(finalProductPart))
                            Directory.CreateDirectory(finalProductPart);
                        using (var fileStream = new FileStream(Path.Combine(finalProductPart, fileName), FileMode.Create))
                        {
                            file.CopyTo(fileStream);
                        }

                        ProductImage image = new()
                        {
                            ImageURL = @"\" + productPath + @"\" + fileName,
                            ProductId = productVM.Product.Id
                        };

                        if (productVM.Product.ProductImages == null)
                            productVM.Product.ProductImages = new List<ProductImage>();

                        productVM.Product.ProductImages.Add(image);
                    }

                    _unitOfWork.Product.Update(productVM.Product);
                    _unitOfWork.Save();
                }
                TempData["success"] = "Product created/updated successfully";
                return RedirectToAction("Index");
            }
            else
            {
                productVM.CategoryList = _unitOfWork.Category.GetAll().Select(u => new SelectListItem
                {
                    Text = u.Name,
                    Value = u.Id.ToString()
                });
                return View(productVM);
            }
        }

        public IActionResult DeleteImage(int imageId)
        {
            var imageToBeDeleted = _unitOfWork.ProductImage.Get(u => u.Id == imageId);
            int prodId = imageToBeDeleted.ProductId;
            if(imageToBeDeleted != null)
            {
                if (!string.IsNullOrEmpty(imageToBeDeleted.ImageURL))
                {
                    var oldTmagePath = Path.Combine(_webHostEnvironment.WebRootPath,
                        imageToBeDeleted.ImageURL.TrimStart('\\'));

                    if (System.IO.File.Exists(oldTmagePath))
                    {
                        System.IO.File.Delete(oldTmagePath);
                    }
                }
                _unitOfWork.ProductImage.Remove(imageToBeDeleted);
                _unitOfWork.Save();

                TempData["success"] = "Deleted successfully";
            }
            return RedirectToAction(nameof(Upsert), new {id = prodId});
        }


        #region API CALLS
        [HttpGet]
        public IActionResult GetAll()
        {
            List<Booky.Models.Product> objCategoryList = _unitOfWork.Product.GetAll(includeProperties: "Category").ToList();
            return Json(new {data = objCategoryList});
        }

        [HttpDelete]
        public IActionResult Delete(int? id)
        {
            var productToBeDel = _unitOfWork.Product.Get(u => u.Id == id);
            if (productToBeDel == null)
            {
                return Json(new { success = false, message = "Error while deleting" });
            }
            string productPath = @"images\products\product-" + id;
            string finalProductPart = Path.Combine(_webHostEnvironment.WebRootPath, productPath);

            if (Directory.Exists(finalProductPart))
            {
                string[] filePaths = Directory.GetFiles(finalProductPart);
                foreach(string filePath in filePaths)
                {
                    System.IO.File.Delete(filePath);
                }
                Directory.Delete(finalProductPart);
            }
            _unitOfWork.Product.Remove(productToBeDel);
            _unitOfWork.Save();

            return Json(new { success = true, message = "Delete successing!" });
        }
        #endregion
    }
}
