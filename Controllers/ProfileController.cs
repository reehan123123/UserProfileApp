using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.IO;
using System.Security.Claims;
using UserProfileApp.Data;
using UserProfileApp.Models;

namespace UserProfileApp.Controllers
{

    [Authorize]
    public class ProfileController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ApplicationDbContext _context;
        private readonly string _uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\uploads");
        private readonly string _defaultProfilePicture = "/assets/default.jfif";
        private static int uploadStatusCount = 0;
        private static string userId = string.Empty;

        public ProfileController(UserManager<IdentityUser> userManager, ApplicationDbContext context)
        {
            _userManager = userManager;
            _context = context;


        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            if (userId == string.Empty)
            {
                userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            }
            // Check if the user exists in the UserProfiles table
            var userProfile = await _context.UserProfiles.FirstOrDefaultAsync(u => u.UserId == userId);

            var userDirectory = Path.Combine(_uploadPath, userId);

            // Construct profile picture path
            var profilePicturePath = Path.Combine(userDirectory, $"{userId}_profile.jpg"); // Assuming profile picture is saved as "{UserId}_profile.jpg"
            var profilePictureUrl = System.IO.File.Exists(profilePicturePath) ? "/uploads/" + userId + "/" + userId + "_profile.jpg" : _defaultProfilePicture;

            var model = new UserProfile
            {
                UserId = userId,
                FirstName = userProfile?.FirstName,
                LastName = userProfile?.LastName,
                Address = userProfile?.Address,
                PhoneNumber = userProfile?.PhoneNumber,
                ProfilePicturePath = profilePictureUrl
            };

            if (Directory.Exists(userDirectory))
            {
                var files = Directory.GetFiles(userDirectory)
                                     .Select(filePath => new UserFile
                                     {
                                         FileName = Path.GetFileName(filePath),
                                         FilePath = filePath
                                     })
                                     .ToList();

                model.Files = files;
            }

            return View(model);
        }


        [HttpPost]
        public async Task<IActionResult> Index(UserProfile model)
        {
            TempData["Submitted"] = string.Empty;

            var userDirectory = Path.Combine(_uploadPath, userId);
            var fileName = $"{userId}_profile.jpg";
            var filePath = Path.Combine(userDirectory, fileName);

            model.UserId = userId;
            model.ProfilePicturePath = "/uploads/" + userId + "/" + userId + "_profile.jpg";

            if (Directory.Exists(userDirectory))
            {
                var files = Directory.GetFiles(userDirectory)
                                     .Select(filePath => new UserFile
                                     {
                                         FileName = Path.GetFileName(filePath),
                                         FilePath = filePath
                                     })
                                     .ToList();

                model.Files = files;
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            if (uploadStatusCount < 2)
            {
                TempData["UploadStatus"] = "Kindly Upload the files";
                return View(model);
            }
            else
            {
                TempData["UploadStatus"] = null;
            }

            if (ModelState.IsValid)
            {
                var existingProfile = await _context.UserProfiles.FirstOrDefaultAsync(u => u.UserId == userId);

                if (existingProfile != null)
                {
                    // If the profile exists, update the existing profile details
                    existingProfile.FirstName = model.FirstName;
                    existingProfile.LastName = model.LastName;
                    existingProfile.Address = model.Address;
                    existingProfile.PhoneNumber = model.PhoneNumber;

                    _context.UserProfiles.Update(existingProfile);
                }
                else
                {
                    // If the profile doesn't exist, set the UserId and add the new profile
                    _context.UserProfiles.Add(model);
                }

                // Save changes to the inmemory
                await _context.SaveChangesAsync();

                // Re-render the view with the model to show validation errors
                TempData["Submitted"] = "User Profile Successfully Updated!!!";
                return View(model);
            }
            // Redirect to avoid re-posting the form on refresh
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> UploadProfilePicture(IFormFile ProfilePicturePath, UserProfile model)
        {
            if (ProfilePicturePath == null)
            {
                TempData["InvalidProfilePic"] = "Please select a profile picture to upload.";
                return RedirectToAction("Index");
            }

            // Check file extension
            var extension = Path.GetExtension(ProfilePicturePath.FileName).ToLowerInvariant();
            if (extension != ".jpg" && extension != ".jpeg" && extension != ".png" && extension != ".gif")
            {
                TempData["InvalidProfilePic"] = "Only image files (JPG, PNG, GIF) are allowed.";
                return RedirectToAction("Index"); // Return to the same view with an error message
            }

            // Check MIME type
            var mimeType = ProfilePicturePath.ContentType;
            if (mimeType != "image/jpeg" && mimeType != "image/png" && mimeType != "image/gif")
            {
                TempData["InvalidProfilePic"] = "Only image files (JPG, PNG, GIF) are allowed.";
                return RedirectToAction("Index"); // Return to the same view with an error message
            }


            var fileName = $"{userId}_profile.jpg";

            // Define the user-specific directory path
            var userDirectory = Path.Combine(_uploadPath, userId);

            // Ensure the directory exists. If it doesn't, create it.
            if (!Directory.Exists(userDirectory))
            {
                Directory.CreateDirectory(userDirectory);
            }

            // Combine the user directory with the file name to get the full file path

            var filePath = Path.Combine(userDirectory, fileName);

            // Save the uploaded file
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await ProfilePicturePath.CopyToAsync(stream);
            }

            uploadStatusCount += 1;
            // Redirect to the Index action to show the updated profile picture
            return RedirectToAction("Index");

        }


        [HttpPost]
        public async Task<IActionResult> UploadFile(IFormFile file, UserProfile model)
        {

            if (file == null)
            {
                TempData["InvalidFile"] = "Please select a file to upload.";
                return RedirectToAction("Index");
            }

            // Check file extension
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (extension != ".docx" && extension != ".doc" && extension != ".pdf")
            {
                TempData["InvalidFile"] = "Only documet files (DOCX, DOC, PDF) are allowed.";
                return RedirectToAction("Index"); // Return to the same view with an error message
            }


            // Define the user-specific directory path
            var userDirectory = Path.Combine(_uploadPath, userId);

            // Ensure the directory exists. If it doesn't, create it.
            if (!Directory.Exists(userDirectory))
            {
                Directory.CreateDirectory(userDirectory);
            }

            // Combine the user directory with the file name to get the full file 

            var filePath = Path.Combine(userDirectory, Path.GetFileName(file.FileName));
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }


            uploadStatusCount += 1;
            return RedirectToAction("Index");
        }

        [HttpGet]
        public IActionResult DownloadFile(string fileName)
        {
            var userDirectory = Path.Combine(_uploadPath, userId);

            var filePath = Path.Combine(userDirectory, fileName);
            if (!System.IO.File.Exists(filePath))
            {
                return NotFound();
            }

            var fileBytes = System.IO.File.ReadAllBytes(filePath);
            return File(fileBytes, "application/octet-stream", fileName);
        }

        [HttpGet]
        public IActionResult DeleteFile(string fileName)
        {
            var userDirectory = Path.Combine(_uploadPath, userId);

            var filePath = Path.Combine(userDirectory, fileName);
            if (System.IO.File.Exists(filePath))
            {
                System.IO.File.Delete(filePath);
            }

            return RedirectToAction("Index");
        }
    }

}
