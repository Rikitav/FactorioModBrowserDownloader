using System.IO;
using System.Windows.Media.Imaging;

namespace FactorioNexus.Utilities
{
    public static class FileSystemExtensions
    {
        public static FileInfo IndexFile(this DirectoryInfo directory, string fileName)
            => new FileInfo(Path.Combine(directory.FullName, fileName));

        public static BitmapSource LoadThumbnailFile(this FileInfo thumbnailFile)
        {
            BitmapImage openedImage = new BitmapImage();
            openedImage.BeginInit();

            // Copying from filestream
            using (FileStream fileStream = thumbnailFile.OpenRead())
            {
                openedImage.StreamSource = fileStream;
                openedImage.EndInit();
            }

            openedImage.Freeze();
            return openedImage;
        }
    }
}
