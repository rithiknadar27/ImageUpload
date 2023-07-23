using Microsoft.AspNetCore.Mvc;
using ImageUpload.Models;
using System.Data.SqlClient;
using System.Data;
using System.ComponentModel;
using System.Buffers.Text;
using System.Text;

namespace ImageUpload.Controllers
{
    public class ImageFileUploadController : Controller
    {
        private readonly IWebHostEnvironment _env; // wwwroot linking
        string _con;
        public ImageFileUploadController(IConfiguration configuration, IWebHostEnvironment env)
        {
            _con = configuration.GetConnectionString("_con");
            _env = env;
        }
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Image()
        {
            Image image = new Image();
            image.Images =GetImage();
            return View(image);
        }

        [HttpPost]
        public JsonResponse InsertFile(Image image)
        {
            JsonResponse response = new JsonResponse();
            SqlConnection sqlConnection = new SqlConnection(_con);
            SqlCommand sqlCommand = new SqlCommand("Rithik.sp_ImageFileUpload", sqlConnection);
            sqlCommand.CommandType = CommandType.StoredProcedure;
            sqlCommand.Parameters.AddWithValue("@ImageID", image.ImageID);
            sqlCommand.Parameters.AddWithValue("@UserName", image.Name);
            sqlCommand.Parameters.AddWithValue("@File", image.FileUpload);
            sqlCommand.Parameters.AddWithValue("@FileType", image.FileType);
            sqlCommand.Parameters.AddWithValue("@FileData", image.FileData);
            sqlCommand.Parameters.AddWithValue("@Base64", image.Base64);
            sqlCommand.Parameters.AddWithValue("@Flag", "InsertUpdate");
            SqlDataAdapter adapter = new SqlDataAdapter(sqlCommand);
            DataTable dt = new DataTable();
            adapter.Fill(dt);
            if (dt != null && dt.Rows.Count > 0)
            {
                response.Status = Convert.ToString(dt.Rows[0]["Status"]);
                response.Message = Convert.ToString(dt.Rows[0]["Message"]);
                response.Data = Convert.ToString(dt.Rows[0]["ID"]);
            }
            return response;

        }

        public List<Image> GetImage()
        {
            List<Image> images = new List<Image>();
            SqlConnection sqlConnection = new SqlConnection(_con);
            SqlCommand sqlCommand = new SqlCommand("Rithik.sp_ImageFileUpload", sqlConnection);
            sqlCommand.CommandType = CommandType.StoredProcedure;
            sqlCommand.Parameters.AddWithValue("@Flag", "Get");
            SqlDataAdapter adapter = new SqlDataAdapter(sqlCommand);
            DataTable dt = new DataTable();
            adapter.Fill(dt);
            if (dt != null && dt.Rows.Count > 0)
            {
                for(int i = 0;i< dt.Rows.Count;i++)
                {
                    Image data = new Image();
                    data.ImageID = Convert.ToInt32(dt.Rows[i]["ImageID"]);
                    data.FileUpload = Convert.ToString(dt.Rows[i]["File"]);
                    images.Add(data);
                }
            }
            return images;
        }

        public Image ViewImage (int id)
        {
            string[]? array;
            Image image = new Image();
            SqlConnection sqlConnection = new SqlConnection(_con);
            SqlCommand sqlCommand = new SqlCommand("Rithik.sp_GetImageFileUpload", sqlConnection);
            sqlCommand.CommandType = CommandType.StoredProcedure;
            sqlCommand.Parameters.AddWithValue("@ImageID", id);
            SqlDataAdapter adapter = new SqlDataAdapter(sqlCommand);
            DataTable dt = new DataTable();
            adapter.Fill(dt);
            if (dt != null && dt.Rows.Count > 0)
            {
                image.ImageID = Convert.ToInt32(dt.Rows[0]["ImageID"]);
                image.FileUpload = Convert.ToString(dt.Rows[0]["File"]);
                array = image.FileUpload.Split(",");
                image.FileUploadArray = array;

                // Convert the text to base64.
                //image.Base64 = (byte[])dt.Rows[0]["Base64"];

       
              
            }
                return image;
            }


        [HttpPost] //single base64 upload
        public async Task<JsonResponse> UploadFile(IList<IFormFile> filesData, int id)
        {
            Image image = new Image();
            JsonResponse response = new JsonResponse();
            string uploads = Path.Combine(_env.WebRootPath, "uploads");
            try
            {
                foreach (IFormFile file in filesData)
                {
                    if(file.Length >0)
                    {
                        image.ImageID = id;
                        image.FileUpload += file.FileName + ",";
                        image.FileType = file.ContentType;
                        var File = image.FileUpload;
                        File = File.Remove(File.Length - 1, 1);
                        image.FileUpload = File;

                        string filePath = Path.Combine(uploads, file.FileName);

                        using (Stream stream = new FileStream(filePath, FileMode.Create))
                        {
                            await file.CopyToAsync(stream);
                        }

                        byte[] imageArray = System.IO.File.ReadAllBytes(filePath);

                        string base64 = Convert.ToBase64String(imageArray);
                        image.Base64 = Convert.FromBase64String(base64);
                    }
                }
                response = InsertFile(image);
            }
            catch (Exception ex)
            {
                response.Message = Convert.ToString(ex.Message);
            }
            return response;
        }

        [HttpPost] //multiple base64 upload
        public async Task<JsonResponse> UploadBase64File(IList<IFormFile> filesData, int id)
        {
            Image image = new Image();
            JsonResponse response = new JsonResponse();
            string uploads = Path.Combine(_env.WebRootPath, "uploads");
            try
            {
                string newBase64 ="";
                foreach (IFormFile file in filesData)
                {
                    if (file.Length > 0)
                    {
                        image.ImageID = id;
                        //image.FileUpload += file.FileName + ",";
                        //var File = image.FileUpload;
                        //File = File.Remove(File.Length - 1, 1);
                        //image.FileUpload = File;
                        image.FileType = file.ContentType;

                        string filePath = Path.Combine(uploads, file.FileName);

                        using (Stream stream = new FileStream(filePath, FileMode.Create))
                        {
                            await file.CopyToAsync(stream);
                        }

                        byte[] imageArray = System.IO.File.ReadAllBytes(filePath);

                         string base64 = Convert.ToBase64String(imageArray);
                        newBase64 += base64 + ",";
                       
                    }
                    
                }
                newBase64 = newBase64.Remove(newBase64.Length - 1, 1);
                image.FileUpload = newBase64;
                response = InsertFile(image);
            }
            catch (Exception ex)
            {
                response.Message = Convert.ToString(ex.Message);
            }
            return response;
        }

    }
}
