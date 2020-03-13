using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Excel_Accounts_Backend.Data.CloudStorage;
using Excel_Accounts_Backend.Dtos.Values;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Drawing;
using System.Drawing.Imaging;
using QRCoder;
using Microsoft.AspNetCore.Http;

namespace Excel_Accounts_Backend.Controllers
{
    [Authorize]
    [Route("[controller]")]
    [ApiController]
    public class ValuesController : ControllerBase
    {
        private readonly ICloudStorage _cloudStorage;
        private readonly IConfiguration _configuration;
        public ValuesController(ICloudStorage cloudStorage, IConfiguration configuration)
        {
            _cloudStorage = cloudStorage;
            _configuration = configuration;
        }
        // POST api/values
        [HttpGet]
        public IActionResult Get()
        {
            return Ok(new { Response = "Success" });
        }

        [HttpPost("upload")]
        public async Task<IActionResult> Post([FromForm]DataForFileUploadDto dataForFileUpload)
        {
            await UploadFile(dataForFileUpload);
            string ImageUrl = _configuration.GetValue<string>("CloudStorageUrl") + dataForFileUpload.ImageStorageName;
            return Ok(new { Response = ImageUrl });
        }

        private async Task UploadFile(DataForFileUploadDto dataForFileUpload)
        {
            string fileNameForStorage = "accounts/qr-code/" + FormFileName(dataForFileUpload.Name, dataForFileUpload.Image.FileName);
            dataForFileUpload.ImageUrl = await _cloudStorage.UploadFileAsync(dataForFileUpload.Image, fileNameForStorage);
            dataForFileUpload.ImageStorageName = fileNameForStorage;
        }

        private static string FormFileName(string title, string fileName)
        {
            var fileExtension = Path.GetExtension(fileName);
            var fileNameForStorage = $"{title}-{DateTime.Now.ToString("yyyyMMddHHmmss")}{fileExtension}";
            return fileNameForStorage;
        }

        [HttpPost("qrcode")]
        public async Task<IActionResult> CreateQrCode([FromForm]string ExcelId)
        {
            DataForFileUploadDto qrCodeDto = new DataForFileUploadDto();
            qrCodeDto.Name = ExcelId;
            Bitmap qrCodeImage = GenerateQrCode(qrCodeDto);
            BitmapToImageFile(qrCodeImage, qrCodeDto);

            await UploadFile(qrCodeDto);
            string ImageUrl = _configuration.GetValue<string>("CloudStorageUrl") + qrCodeDto.ImageStorageName;
            return Ok(new { Response = ImageUrl });
        }
        
        // Generates Bitmap image of the string
        private static Bitmap GenerateQrCode(DataForFileUploadDto qrCodeDto)
        {
            QRCodeGenerator qrGenerator = new QRCodeGenerator();
            QRCodeData qrCodeData = qrGenerator.CreateQrCode(qrCodeDto.Name, QRCodeGenerator.ECCLevel.Q);
            QRCode qrCode = new QRCode(qrCodeData);
            Bitmap qrCodeImage = qrCode.GetGraphic(20);
            return qrCodeImage;
        }

        // Converts the Bitmap Image to a PNG File
        private static void BitmapToImageFile(Bitmap qrCodeImage, DataForFileUploadDto qrCodeDto)
        {    
            byte[] bitmapBytes = BitmapToBytes(qrCodeImage);             
            var stream = new MemoryStream(bitmapBytes);     
            IFormFile Image = new FormFile(stream, 0, bitmapBytes.Length, qrCodeDto.Name, qrCodeDto.Name + ".png"); 
            qrCodeDto.Image = Image;
        }

        // Converts the Bitmap to byte array
        private static byte[] BitmapToBytes(Bitmap img)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                img.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
                return stream.ToArray();
            }
        }
    }
}