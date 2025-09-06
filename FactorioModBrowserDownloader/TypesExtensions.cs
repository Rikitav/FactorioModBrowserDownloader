using FactorioNexus.ApplicationArchitecture.DataBases;
using FactorioNexus.ApplicationArchitecture.Dependencies;
using FactorioNexus.ApplicationArchitecture.Services;
using FactorioNexus.ApplicationInterface.Dependencies;
using FactorioNexus.ApplicationInterface.ViewModels;
using FactorioNexus.ApplicationPresentation.Markups.MainWindow;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.IO;
using System.Windows.Media.Imaging;

namespace FactorioNexus
{
    public static class TypesExtensions
    {
        public static IServiceCollection RegisterApplicationDefaults(this IServiceCollection services)
        {
            // Configure Logging
            services.AddLogging(builder => builder.AddConsole());

            // Register Services
            services
                .AddSingleton<IDatabaseIndexer, DatabaseIndexer>()
                .AddSingleton<IDependencyResolver, DependencyResolver>()
                .AddSingleton<IDownloadingManager, DownloadingManager>()
                .AddSingleton<IFactorioNexusClient, FactorioNexusClient>()
                .AddSingleton<IStoringManager, StoringManager>()
                .AddSingleton<IThumbnailsResolver, ThumbnailsResolver>();

            // Register ViewModels
            services
                .AddSingleton<IModsBrowserViewModel, ModsBrowserViewModel>()
                .AddSingleton<IModsStorageViewModel, ModsStorageViewModel>()
                .AddSingleton<IApplicationSettingViewModel, ApplicationSettingsViewModel>();

            // Adding database contexts
            services.AddDbContext<IndexedModPortalDatabase>(ServiceLifetime.Transient, ServiceLifetime.Singleton);

            // Register Views
            services.AddSingleton<MainWindowMarkup>();

            return services;
        }

        public static T ThrowIfNull<T>(this T? obj, string message)
            => obj is null ? throw new NullReferenceException(message) : obj;

        public static IEnumerable<T> OrderBy<T, Key>(this IEnumerable<T> values, bool descending, Func<T, Key> keySelector)
            => descending ? values.OrderByDescending(keySelector) : values.OrderBy(keySelector);

        public static async Task CopyToAsync(this Stream source, Stream destination, int bufferSize, Action<long>? progress = null, CancellationToken cancellationToken = default)
        {
            long totalBytesRead = 0;
            byte[] buffer = new byte[bufferSize];

            while (true)
            {
                int bytesRead = await source.ReadAsync(buffer, cancellationToken);
                if (bytesRead == 0)
                {
                    progress?.Invoke(totalBytesRead);
                    break;
                }

                await destination.WriteAsync(buffer.AsMemory(0, bytesRead), cancellationToken);
                totalBytesRead += bytesRead;
                progress?.Invoke(totalBytesRead);
            }
        }

        public static async Task CopyToAsync(this Stream source, Stream destination, int bufferSize, IProgress<long>? progress = null, CancellationToken cancellationToken = default)
        {
            long totalBytesRead = 0;
            byte[] buffer = new byte[bufferSize];

            while (true)
            {
                int bytesRead = await source.ReadAsync(buffer, cancellationToken);
                if (bytesRead == 0)
                {
                    progress?.Report(totalBytesRead);
                    break;
                }

                await destination.WriteAsync(buffer.AsMemory(0, bytesRead), cancellationToken);
                totalBytesRead += bytesRead;
                progress?.Report(totalBytesRead);
            }
        }

        public static string FirstLetterToUpper(this string source)
        {
            char[] chars = source.ToCharArray();
            for (int i = 0; i < chars.Length; i++)
            {
                char lookChar = chars[i];
                if (!char.IsLetter(lookChar))
                    continue;

                if (char.IsUpper(lookChar))
                    return source;

                chars[i] = char.ToUpper(lookChar);
                return new string(chars);
            }

            return source;
        }

        public static bool Aggreagate<T>(this Exception exception)
        {
            if (exception is T)
                return true;

            if (exception.InnerException == null)
                return false;

            return Aggreagate<T>(exception.InnerException);
        }

        public static FileInfo IndexFile(this DirectoryInfo directory, string fileName)
            => new FileInfo(Path.Combine(directory.FullName, fileName));

        public static BitmapSource LoadThumbnailFile(this FileInfo thumbnailFile)
        {
            using FileStream fileStream = thumbnailFile.OpenRead();
            BitmapImage openedImage = new BitmapImage();

            // Copying from filestream
            openedImage.BeginInit();
            openedImage.StreamSource = new MemoryStream();
            fileStream.CopyTo(openedImage.StreamSource);
            openedImage.EndInit();

            return openedImage;
        }
    }
}
