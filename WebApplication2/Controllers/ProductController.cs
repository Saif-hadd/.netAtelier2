using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WebApplication2.Models;
using WebApplication2.Models.Repositories;
using WebApplication2.ViewModels;
using System.IO;
using System;

namespace Gestion_des_articles.Controllers
{
    public class ProductController : Controller
    {
        private readonly IRepository<Product> ProductRepository;
        private readonly IWebHostEnvironment hostingEnvironment;

        public ProductController(IRepository<Product> ProdRepository, IWebHostEnvironment hostingEnvironment)
        {
            ProductRepository = ProdRepository;
            this.hostingEnvironment = hostingEnvironment;
        }

        // GET: ProductController/Index
        public ActionResult Index()
        {
            var products = ProductRepository.GetAll();
            return View(products);
        }

        // GET: ProductController/Details/5
        public ActionResult Details(int id)
        {
            var product = ProductRepository.Get(id);
            if (product == null)
            {
                return NotFound(); // Si le produit n'existe pas, renvoie une page 404
            }
            return View(product);
        }

        // GET: ProductController/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: ProductController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(CreateViewModel model)
        {
            if (ModelState.IsValid)
            {
                string uniqueFileName = ProcessUploadedFile(model);

                // Créer un nouvel objet Product
                Product newProduct = new Product
                {
                    Désignation = model.Désignation,
                    Prix = model.Prix,
                    Quantite = model.Quantite,
                    Image = uniqueFileName
                };

                // Ajouter le produit dans le repository
                ProductRepository.Add(newProduct);

                // Rediriger vers la page des détails du produit
                return RedirectToAction("Details", new { id = newProduct.Id });
            }

            // Si le modèle est invalide, retourner la vue avec les erreurs
            return View(model);
        }

        // GET: ProductController/Edit/5
        public ActionResult Edit(int id)
        {
            var product = ProductRepository.Get(id);
            if (product == null)
            {
                return NotFound();
            }

            // Créer un EditViewModel pour remplir le formulaire de modification
            EditViewModel productEditViewModel = new EditViewModel
            {
                Id = product.Id,
                Désignation = product.Désignation,
                Prix = product.Prix,
                Quantite = product.Quantite,
                ExistingImagePath = product.Image
            };
            return View(productEditViewModel);
        }

        // POST: ProductController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(EditViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Récupérer le produit à modifier
                Product product = ProductRepository.Get(model.Id);
                if (product == null)
                {
                    return NotFound();
                }

                // Mettre à jour les informations du produit
                product.Désignation = model.Désignation;
                product.Prix = model.Prix;
                product.Quantite = model.Quantite;

                // Gérer le remplacement de l'image si une nouvelle est fournie
                if (model.ImagePath != null)
                {
                    if (model.ExistingImagePath != null)
                    {
                        string oldImagePath = Path.Combine(hostingEnvironment.WebRootPath, "images", model.ExistingImagePath);
                        if (System.IO.File.Exists(oldImagePath))
                        {
                            System.IO.File.Delete(oldImagePath);
                        }
                    }
                    product.Image = ProcessUploadedFile(model);
                }

                Product updatedProduct = ProductRepository.Update(product);
                if (updatedProduct != null)
                {
                    return RedirectToAction("Index");
                }
                else
                {
                    return NotFound();
                }
            }

            // Retourner la vue avec les erreurs si le modèle est invalide
            return View(model);
        }

        // GET: ProductController/Delete/5
        public ActionResult Delete(int id)
        {
            var product = ProductRepository.Get(id);
            if (product == null)
            {
                return NotFound();
            }
            return View(product);
        }

        // POST: ProductController/Delete/5
        // POST: ProductController/Delete/5
        // POST: ProductController/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            try
            {
                var product = ProductRepository.Get(id);
                if (product == null)
                {
                    return NotFound(); // Produit non trouvé
                }

                // Supprimer l'image associée au produit
                if (product.Image != null)
                {
                    string imagePath = Path.Combine(hostingEnvironment.WebRootPath, "images", product.Image);
                    if (System.IO.File.Exists(imagePath))
                    {
                        System.IO.File.Delete(imagePath);
                    }
                }

                ProductRepository.Delete(id); // Suppression du produit dans le repository
                return RedirectToAction(nameof(Index)); // Redirection après la suppression
            }
            catch (Exception ex)
            {
                // En cas d'erreur, afficher un message ou rediriger vers une page d'erreur
                ViewBag.ErrorMessage = "Une erreur est survenue lors de la suppression du produit. Veuillez réessayer.";
                return View("Error"); // Assurez-vous d'avoir une vue d'erreur
            }
        }


        // Ajout de la méthode de recherche
        public ActionResult Search(string term)
        {
            var results = ProductRepository.Search(term); // Appel de la méthode Search dans le repository
            return View("Index", results); // Rediriger vers la vue Index avec les résultats de recherche
        }


        // Méthode pour traiter l'upload d'images
        [NonAction]
        private string ProcessUploadedFile(CreateViewModel model)
        {
            string uniqueFileName = null;
            if (model.ImagePath != null)
            {
                string uploadsFolder = Path.Combine(hostingEnvironment.WebRootPath, "images");
                uniqueFileName = Guid.NewGuid().ToString() + "_" + model.ImagePath.FileName;
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    model.ImagePath.CopyTo(fileStream);
                }
            }
            return uniqueFileName;
        }
    }
}
