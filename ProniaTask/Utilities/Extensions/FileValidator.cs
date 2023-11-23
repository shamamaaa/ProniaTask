using System;
using ProniaTask.Models;

namespace ProniaTask.Utilities.Extensions
{
	public static class FileValidator
	{
		public static bool ValidateType(this IFormFile file, string type="image/")
		{
			if (file.ContentType.Contains(type))
			{
				return true;
			}
			return false;
		}

        public static bool ValidateSize(this IFormFile file, int kb)
        {
            if (file.Length<=kb*1024)
            {
                return true;
            }
            return false;
        }

		public static async Task<string> CreateFile(this IFormFile file, params string[] folders)
		{
            string filename = file.GenerateName();
            string path = folders.Aggregate(Path.Combine);
            path = Path.Combine(path, filename);

            using (FileStream stream = new FileStream(path, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return (filename);
        }

        public static async void DeleteFile(this string filename, string root, params string[] folders)
        {
            string path = root;

            for (int i = 0; i < folders.Length; i++)
            {
                path = Path.Combine(path, folders[i]);
            }
            path = Path.Combine(path, filename);

            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }

        public static string GenerateName(this IFormFile file)
        {
            int index = file.FileName.LastIndexOf('.');
            string filename = Guid.NewGuid().ToString() + file.FileName.Substring(index);
            return filename;
        }
    }
}

