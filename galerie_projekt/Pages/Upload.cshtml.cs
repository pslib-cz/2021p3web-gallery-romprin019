using galerie_projekt.Data;
using galerie_projekt.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Metadata.Profiles.Exif;
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

            if (Int32.TryParse(_configuration["Thumbnails:SquareSize"], out _squareSize) == false) _squareSize = 64; // z�skej data z konfigurace nebo pou�ij 64
            if (Int32.TryParse(_configuration["Thumbnails:SameAspectRatioHeigth"], out _sameAspectRatioHeigth) == false) _sameAspectRatioHeigth = 128;
        }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var userId = User.Claims.Where(c => c.Type == ClaimTypes.NameIdentifier).FirstOrDefault().Value; // z�sk�me id p�ihl�en�ho u�ivatele
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

                if (uploadedFile.ContentType.StartsWith("image")) // je soubor obr�zek?
                {


                    fileRecord.Thumbnails = new List<Thumbnail>();
                    MemoryStream ims = new MemoryStream(); // proud pro p��choz� obr�zek
                    MemoryStream oms1 = new MemoryStream(); // proud pro �tvercov� n�hled
                    MemoryStream oms2 = new MemoryStream(); // proud pro obd�ln�kov� n�hled
                    uploadedFile.CopyTo(ims); // vlo� obsah do vstupn�ho proudu
                    IImageFormat format; // zde si ulo��me form�t obr�zku (JPEG, GIF, ...), budeme ho pot�ebovat p�i ukl�d�n�
                    using (Image image = Image.Load(ims.ToArray(), out format)) // vytvo��me �tvercov� n�hled
                    {
                        image.Metadata.ExifProfile = new ExifProfile();
                        var exifProfile = image.Metadata.ExifProfile;
                        if (image.Metadata.ExifProfile != null)
                        {
                            if (exifProfile.GetValue(ExifTag.DateTimeOriginal) != null)
                            {
                                //Obr�zek m� EXIF data
                                fileRecord.TakenAt = exifProfile.GetValue(ExifTag.DateTimeOriginal).ToString();
                            }
                            else
                            {
                                fileRecord.TakenAt = DateTime.Now.ToString();
                            }
                        }
                        int largestSize = Math.Max(image.Height, image.Width);
                        if (image.Width > 2000)
                        {
                            return BadRequest("Image is too big, width > 2000px");
                        }// jak� je orientace obr�zku?
                        if (image.Width > image.Height) // podle orientace zm�n�me velikost obr�zku
                        {
                            image.Mutate(x => x.Resize(0, _squareSize));
                        }
                        else
                        {
                            image.Mutate(x => x.Resize(_squareSize, 0));
                        }
                        image.Mutate(x => x.Crop(new Rectangle((image.Width - _squareSize) / 2, (image.Height - _squareSize) / 2, _squareSize, _squareSize)));
                        // obr�zek o��zneme na �tverec
                        image.Save(oms1, format); // vlo��me ho do v�stupn�ho proudu
                        fileRecord.Thumbnails.Add(new Thumbnail { Type = ThumbnailType.Square, Blob = oms1.ToArray() }); // a ulo��me do datab�ze jako pole byt�

                        

                    }
                    using (Image image = Image.Load(ims.ToArray(), out format)) // obd�ln�kov� n�hled za��n� zde
                    {
                        image.Mutate(x => x.Resize(0, _sameAspectRatioHeigth)); // sta�� jen zm�nit jeho velikost
                        image.Save(oms2, format); // a p�es proud ho ulo�it do datab�ze
                        fileRecord.Thumbnails.Add(new Thumbnail { Type = ThumbnailType.SameAspectRatio, Blob = oms2.ToArray() });
                    }

                    // vytvo��me z�znam
                    try
                    {

                        _context.Images.Add(fileRecord);// a ulo��me ho
                        _context.SaveChanges(); // t�m se n�m vygeneruje jeho kl�� ve form�tu Guid
                        var defaultalbum = _context.Albums.Where(p => p.Id == Guid.Parse(userId)).FirstOrDefault();
                        var albumimage = new AlbumImage
                        {
                            AlbumId = defaultalbum.Id,
                            FileId = fileRecord.Id,
                            StoredImage = fileRecord,
                            Album = defaultalbum,
                            Description = "Uploaded at " + fileRecord.UploadedAt


                        };
                        _context.AlbumImages.Add(albumimage);
                        _context.SaveChanges();

                        if (!Directory.Exists("Uploads"))
                        {
                            Directory.CreateDirectory("Uploads");
                        }
                        var file = Path.Combine(_environment.ContentRootPath, "Uploads", fileRecord.Id.ToString());
                        // pod t�mto kl��em ulo��me soubor i fyzicky na disk
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
                }
                else
                {
                    ErrorMessage = "We only accept images!";
                }

            }
            return RedirectToPage("/Index");
        }
    }
}
