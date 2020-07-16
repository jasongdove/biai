using System;
using System.Globalization;
using System.IO;
using LanguageExt;
using LanguageExt.Common;

namespace BiAi.Models
{
    public class Image
    {
        public static Either<Error, Image> FromFullPath(string fullPath)
        {
            var fileName = Path.GetFileNameWithoutExtension(fullPath);
            var components = fileName.Split('.');
            if (components.Length != 2)
            {
                return Error.New($"Unexpected image file name {fileName}");
            }

            var cameraName = components[0];
            var timestampString = components[1];

            if (!DateTime.TryParseExact(timestampString, "yyyyMMdd_HHmmssfff", CultureInfo.CurrentUICulture,
                DateTimeStyles.None,
                out DateTime timestamp))
            {
                return Error.New($"Could not parse timestamp from image file {fileName}");
            }

            return new Image(fullPath, cameraName, timestamp);
        }

        private Image(string fullPath, string cameraName, DateTime timestamp)
        {
            FullPath = fullPath;
            CameraName = cameraName;
            Timestamp = timestamp;
        }

        public string FullPath { get; }

        public string CameraName { get; }

        public DateTime Timestamp { get; }
    }
}