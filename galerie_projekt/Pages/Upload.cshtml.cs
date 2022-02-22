using galerie_projekt.Data;
using galerie_projekt.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Processing;
using System.Security.Claims;

namespace galerie_projekt.Pages
{
    [Authorize]
    public class UploadModel : PageModel
    {
        private IWebHostEnvironment _environment;
        private ApplicationDbContext _context;
        private IConfiguration _configuration;

        [TempData]
        public string SuccessMessage { get; set; }
        [TempData]
        public string ErrorMessage { get; set; }
        [BindProperty]
        public ICollection<IFormFile> Upload { get; set; }
        [BindProperty]
        public bool ImageIsPublic { get; set; }
        public int _sameAspectRatioHeigth;
        public int _squareSize;



        public UploadModel(IWebHostEnvironment environment, ApplicationDbContext context, IConfiguration configuration)
        {
            _environment = environment;
            _context = context;
            _configuration = configuration;

            if (Int32.TryParse(_configuration["Thumbnails:SquareSize"], out _squareSize) == false) _squareSize = 256; // získej data z konfigurace nebo použij 64
            if (Int32.TryParse(_configuration["Thumbnails:SameAspectRatioHeigth"], out _sameAspectRatioHeigth) == false) _sameAspectRatioHeigth = 128;
        }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var userId = User.Claims.Where(c => c.Type == ClaimTypes.NameIdentifier).FirstOrDefault().Value; // získáme id pøihlášeného uživatele
            int successfulProcessing = 0;
            int failedProcessing = 0;
            foreach (var uploadedFile in Upload)
            {
                var fileRecord = new Model.StoredImage
                {
                    OriginalName = uploadedFile.FileName,
                    UploaderId = userId,
                    UploadedAt = DateTime.Now,
                    ContentType = uploadedFile.ContentType,
                    IsPublic = ImageIsPublic
                };
                if (uploadedFile.ContentType.StartsWith("image")) // je soubor obrázek?
                {
                    fileRecord.Thumbnails = new List<Thumbnail>();
                    MemoryStream ims = new MemoryStream(); // proud pro pøíchozí obrázek
                    MemoryStream oms1 = new MemoryStream(); // proud pro ètvercový náhled
                    MemoryStream oms2 = new MemoryStream(); // proud pro obdélníkový náhled
                    uploadedFile.CopyTo(ims); // vlož obsah do vstupního proudu
                    IImageFormat format; // zde si uložíme formát obrázku (JPEG, GIF, ...), budeme ho potøebovat pøi ukládání
                    using (Image image = Image.Load(ims.ToArray(), out format)) // vytvoøíme ètvercový náhled
                    {
                        int largestSize = Math.Max(image.Height, image.Width); // jaká je orientace obrázku?
                        if (image.Width > image.Height) // podle orientace zmìníme velikost obrázku
                        {
                            image.Mutate(x => x.Resize(0, _squareSize));
                        }
                        else
                        {
                            image.Mutate(x => x.Resize(_squareSize, 0));
                        }
                        image.Mutate(x => x.Crop(new Rectangle((image.Width - _squareSize) / 2, (image.Height - _squareSize) / 2, _squareSize, _squareSize)));
                        // obrázek oøízneme na ètverec
                        image.Save(oms1, format); // vložíme ho do výstupního proudu
                        fileRecord.Thumbnails.Add(new Thumbnail { Type = ThumbnailType.Square, Blob = oms1.ToArray() }); // a uložíme do databáze jako pole bytù
                    }
                    using (Image image = Image.Load(ims.ToArray(), out format)) // obdélníkový náhled zaèíná zde
                    {
                        image.Mutate(x => x.Resize(0, _sameAspectRatioHeigth)); // staèí jen zmìnit jeho velikost
                        image.Save(oms2, format); // a pøes proud ho uložit do databáze
                        fileRecord.Thumbnails.Add(new Thumbnail { Type = ThumbnailType.SameAspectRatio, Blob = oms2.ToArray() });
                    }
                }// vytvoøíme záznam
                try
                {
                    _context.Images.Add(fileRecord); // a uložíme ho
                    await _context.SaveChangesAsync(); // tím se nám vygeneruje jeho klíè ve formátu Guid
                    var file = Path.Combine(_environment.ContentRootPath, "Uploads", fileRecord.Id.ToString());
                    // pod tímto klíèem uložíme soubor i fyzicky na disk
                    using (var fileStream = new FileStream(file, FileMode.Create))
                    {
                        await uploadedFile.CopyToAsync(fileStream);
                    };
                    successfulProcessing++;
                }
                catch
                {
                    failedProcessing++;
                }
                if (failedProcessing == 0)
                {
                    SuccessMessage = "All files has been uploaded successfuly.";
                }
                else
                {
                    ErrorMessage = "There were " + failedProcessing + " errors during uploading and processing of files.";
                }
            }
            return RedirectToPage("/Index");
        }
    }
}
