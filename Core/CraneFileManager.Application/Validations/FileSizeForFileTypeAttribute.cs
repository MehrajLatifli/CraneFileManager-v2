using System;
using System.ComponentModel.DataAnnotations;
using System.IO;
using Microsoft.AspNetCore.Http;

namespace CraneFileManager.Application.Validations
{
    public class FileSizeForFileTypeAttribute : ValidationAttribute
    {
        private readonly string[] _archiveExtensions = { ".zip", ".rar", ".tar", ".gz", ".z", ".img", ".iso", ".7z", ".xz", ".ace", ".lz", ".lzh" };
        private readonly string[] _videoExtensions = { ".mp4", ".avi", ".mkv", ".3gp", ".mov", ".wmv", ".webm", ".avchd", ".flv", ".mpeg", ".vob", ".divx", ".ogv", ".m4v", ".f4v", ".rm", ".xvid" };
        private readonly string[] _audioExtensions = { ".mp3", ".wma", ".wav", ".aac", ".midi", ".flac", ".3ga", ".au", ".ogg", ".alac", ".m4a", ".aiff", ".opus" };
        private readonly string[] _documentExtensions = { ".pdf", ".epub", ".doc", ".ppt", ".xls", ".djvu", ".txt", ".odt", ".docx", ".xlsx", ".pptx", ".odp" };
        private readonly string[] _imageExtensions = { ".jpg", ".png", ".jpeg", ".gif", ".webp", ".tiff", ".ico", ".bmp", ".jpeg2000", ".raw", ".svg", ".wmf" };
        private readonly string[] _otherExtensions = { ".exe", ".apk", ".ipa", ".dmg", ".jar", ".sql", ".xml", ".json", ".yaml", ".csv",".bin" };

        private readonly long _minFileSize = 1024; // 1 KB
        private readonly long _maxArchiveSize = 1073741824; // 1 GB
        private readonly long _maxVideoSize = 1073741824; // 1 GB
        private readonly long _maxAudioSize = 524288000; // 500 MB
        private readonly long _maxDocumentSize = 10485760; // 10 MB
        private readonly long _maxImageSize = 52428800; // 50 MB
        private readonly long _maxOtherSize = 52428800; // 50 MB

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value is IFormFile file)
            {
                var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
                long fileSize = file.Length;

                return ValidateFileSize(extension, fileSize);
            }
            return ValidationResult.Success;
        }

        private ValidationResult ValidateFileSize(string extension, long fileSize)
        {
            switch (extension)
            {
                case string ext when Array.Exists(_archiveExtensions, e => e == ext):
                    if (fileSize < _minFileSize || fileSize > _maxArchiveSize)
                        return new ValidationResult($"File size for archive files ({extension}) must be between {_minFileSize / 1024} KB and {_maxArchiveSize / (1024 * 1024)} MB.");
                    break;

                case string ext when Array.Exists(_videoExtensions, e => e == ext):
                    if (fileSize < _minFileSize || fileSize > _maxVideoSize)
                        return new ValidationResult($"File size for video files ({extension}) must be between {_minFileSize / 1024} KB and {_maxVideoSize / (1024 * 1024)} MB.");
                    break;

                case string ext when Array.Exists(_audioExtensions, e => e == ext):
                    if (fileSize < _minFileSize || fileSize > _maxAudioSize)
                        return new ValidationResult($"File size for audio files ({extension}) must be between {_minFileSize / 1024} KB and {_maxAudioSize / (1024 * 1024)} MB.");
                    break;

                case string ext when Array.Exists(_documentExtensions, e => e == ext):
                    if (fileSize < _minFileSize || fileSize > _maxDocumentSize)
                        return new ValidationResult($"File size for document files ({extension}) must be between {_minFileSize / 1024} KB and {_maxDocumentSize / (1024 * 1024)} MB.");
                    break;

                case string ext when Array.Exists(_imageExtensions, e => e == ext):
                    if (fileSize < _minFileSize || fileSize > _maxImageSize)
                        return new ValidationResult($"File size for image files ({extension}) must be between {_minFileSize / 1024} KB and {_maxImageSize / (1024 * 1024)} MB.");
                    break;

                case string ext when Array.Exists(_otherExtensions, e => e == ext):
                    if (fileSize < _minFileSize || fileSize > _maxOtherSize)
                        return new ValidationResult($"File size for other files ({extension}) must be between {_minFileSize / 1024} KB and {_maxOtherSize / (1024 * 1024)} MB.");
                    break;

                default:
                    return new ValidationResult(GetErrorMessage(extension));
            }
            return ValidationResult.Success;
        }

        private string GetErrorMessage(string extension)
        {
            return $"The file type '{extension}' is not supported or has no defined size limits.";
        }
    }
}
