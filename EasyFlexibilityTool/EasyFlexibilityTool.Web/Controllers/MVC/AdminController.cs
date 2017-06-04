using AutoMapper;
using EasyFlexibilityTool.Storage;
using EasyFlexibilityTool.Web.Controllers.MVC.Base;
using EasyFlexibilityTool.Web.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace EasyFlexibilityTool.Web.Controllers.MVC
{
    public class AdminController : BaseController
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult ManageAdmin()
        {
            ViewBag.UserRole = WebAPI.AdminController.getUserRole(DbContext);
            return View();
        }

        public ActionResult ManageUser()
        {
            return View();
        }

        public ActionResult UserProfile(string userId)
        {
            ViewBag.User = DbContext.Users.Find(userId);
            ViewBag.AngleMeasurementCategories = Mapper.Map<List<AngleMeasurementCategoryModel>>(DbContext.AngleMeasurementCategories.ToList());
            ViewBag.UserPrograms = DbContext.UserPrograms.Where(up => up.UserId == userId).ToList();
            return View();
        }

        public ActionResult MessageUsers()
        {
            return View();
        }

        public ActionResult EmailTemplate()
        {
            return View();
        }

        public FileContentResult ExportEmail()
        {
            string emailCsvString = "";
            foreach (var user in DbContext.Users)
            {
                emailCsvString += "," + user.Email;
            }
            return File(new System.Text.UTF8Encoding().GetBytes(emailCsvString.Substring(1)), "text/csv", "emails.csv");
        }

        public async Task<ActionResult> ExportProfile(string userId)
        {
            AzureStorageService service = await AzureStorageService.GetInstanceAsync(AppSettings.StorageConnectionString, AppSettings.StoragePhotoContainerName);
            var random = new Random();
            var user = DbContext.Users.SingleOrDefault(u => u.Id == userId);
            var angleMeasurements = DbContext.AngleMeasurements.Where(am => am.UserId == userId);
            using (var zipMemoryStream = new MemoryStream())
            {
                using (var zipArchive = new ZipArchive(zipMemoryStream, ZipArchiveMode.Create, true))
                {
                    foreach (var angleMeasurement in angleMeasurements)
                    {
                        var imageName = string.Format("{0}_{1}_{2}(degree)_{3}.png",
                            user.Email,
                            angleMeasurement.DateTimeStamp.ToString("yyyy-MM-dd"),
                            Math.Round(angleMeasurement.Angle),
                            random.Next(1000, 9999)
                        );
                        MemoryStream pngMemoryStream = await service.DownloadBlobToStream($"{angleMeasurement.Id}.{AppSettings.StorageBlobExtension}");
                        var entry = zipArchive.CreateEntry(imageName);
                        using (var entryStream = entry.Open())
                        {
                            byte[] bytes = pngMemoryStream.ToArray();
                            entryStream.Write(bytes, 0, bytes.Length);
                        }
                    }
                }
                return File(zipMemoryStream.ToArray(), "application/zip", string.Format("{0}.zip", user.Email));
            }
        }
    }
}
