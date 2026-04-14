using Application.Messages.Validators;
using FluentValidation.TestHelper;
using Xunit;

namespace Tests.Common.Validators;

public class FileUploadValidatorTests
{
    private readonly FileUploadValidator _validator = new();

    #region Valid File Tests

    [Theory]
    [InlineData("document.pdf", "pdf", 1024)]
    [InlineData("spreadsheet.xlsx", "xlsx", 5 * 1024 * 1024)]
    [InlineData("presentation.pptx", "pptx", 100 * 1024 * 1024)]
    [InlineData("photo.jpg", "jpg", 10 * 1024 * 1024)]
    [InlineData("image.png", "png", 50 * 1024 * 1024)]
    [InlineData("video.mp4", "mp4", 100 * 1024 * 1024)]
    [InlineData("movie.avi", "avi", 500 * 1024 * 1024)]
    public void Validate_WithValidFile_ShouldSucceed(string fileName, string fileType, long sizeBytes)
    {
        // Arrange
        var request = new FileUploadRequest
        {
            FileName = fileName,
            FileType = fileType,
            FileSizeBytes = sizeBytes
        };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WithDotPrefixedFileType_ShouldSucceed()
    {
        // Arrange
        var request = new FileUploadRequest
        {
            FileName = "resume.pdf",
            FileType = ".pdf",
            FileSizeBytes = 1024
        };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    #endregion

    #region Invalid File Type Tests

    [Theory]
    [InlineData("executable.exe")]
    [InlineData("library.dll")]
    [InlineData("script.js")]
    [InlineData("markup.html")]
    [InlineData("data.json")]
    public void Validate_WithUnsupportedFileType_ShouldFail(string fileType)
    {
        // Arrange
        var request = new FileUploadRequest
        {
            FileName = "file.txt",
            FileType = fileType,
            FileSizeBytes = 1024
        };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.FileType)
            .WithErrorMessage("File type is not supported. Supported: pdf, doc, docx, xlsx, pptx, txt, zip, jpg, jpeg, png, gif, webp, bmp, mp4, webm, avi, mov, mkv, flv, m4v");
    }

    [Fact]
    public void Validate_WithEmptyFileType_ShouldFail()
    {
        // Arrange
        var request = new FileUploadRequest
        {
            FileName = "file.txt",
            FileType = "",
            FileSizeBytes = 1024
        };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.FileType);
    }

    #endregion

    #region File Size Tests

    [Fact]
    public void Validate_WithDocumentExceedingMaxSize_ShouldFail()
    {
        // Arrange
        var request = new FileUploadRequest
        {
            FileName = "large.pdf",
            FileType = "pdf",
            FileSizeBytes = 101 * 1024 * 1024  // 101 MB, exceeds 100 MB limit
        };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.FileSizeBytes)
            .WithErrorMessage("File exceeds maximum size for documents (100 MB)");
    }

    [Fact]
    public void Validate_WithImageExceedingMaxSize_ShouldFail()
    {
        // Arrange
        var request = new FileUploadRequest
        {
            FileName = "large.jpg",
            FileType = "jpg",
            FileSizeBytes = 51 * 1024 * 1024  // 51 MB, exceeds 50 MB limit
        };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.FileSizeBytes)
            .WithErrorMessage("File exceeds maximum size for images (50 MB)");
    }

    [Fact]
    public void Validate_WithVideoExceedingMaxSize_ShouldFail()
    {
        // Arrange
        var request = new FileUploadRequest
        {
            FileName = "large.mp4",
            FileType = "mp4",
            FileSizeBytes = 501 * 1024 * 1024  // 501 MB, exceeds 500 MB limit
        };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.FileSizeBytes)
            .WithErrorMessage("File exceeds maximum size for videos (500 MB)");
    }

    [Fact]
    public void Validate_WithZeroFileSize_ShouldFail()
    {
        // Arrange
        var request = new FileUploadRequest
        {
            FileName = "empty.pdf",
            FileType = "pdf",
            FileSizeBytes = 0
        };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.FileSizeBytes)
            .WithErrorMessage("File size must be greater than 0 bytes");
    }

    #endregion

    #region File Name Tests

    [Fact]
    public void Validate_WithEmptyFileName_ShouldFail()
    {
        // Arrange
        var request = new FileUploadRequest
        {
            FileName = "",
            FileType = "pdf",
            FileSizeBytes = 1024
        };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.FileName)
            .WithErrorMessage("File name is required");
    }

    [Fact]
    public void Validate_WithVeryLongFileName_ShouldFail()
    {
        // Arrange
        var request = new FileUploadRequest
        {
            FileName = new string('a', 256),
            FileType = "pdf",
            FileSizeBytes = 1024
        };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.FileName)
            .WithErrorMessage("File name must not exceed 255 characters");
    }

    #endregion

    #region Helper Method Tests

    [Theory]
    [InlineData("pdf", true)]
    [InlineData("doc", true)]
    [InlineData("docx", true)]
    [InlineData("xlsx", true)]
    [InlineData("jpg", false)]
    [InlineData("mp4", false)]
    public void IsDocumentType_WithVariousFileTypes_ShouldReturnCorrectValue(string fileType, bool expected)
    {
        // Act
        var result = FileUploadValidator.IsDocumentType(fileType);

        // Assert
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("jpg", true)]
    [InlineData("png", true)]
    [InlineData("gif", true)]
    [InlineData("webp", true)]
    [InlineData("pdf", false)]
    [InlineData("mp4", false)]
    public void IsImageType_WithVariousFileTypes_ShouldReturnCorrectValue(string fileType, bool expected)
    {
        // Act
        var result = FileUploadValidator.IsImageType(fileType);

        // Assert
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("mp4", true)]
    [InlineData("webm", true)]
    [InlineData("avi", true)]
    [InlineData("mov", true)]
    [InlineData("pdf", false)]
    [InlineData("jpg", false)]
    public void IsVideoType_WithVariousFileTypes_ShouldReturnCorrectValue(string fileType, bool expected)
    {
        // Act
        var result = FileUploadValidator.IsVideoType(fileType);

        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    public void GetAllowedFileTypes_ShouldReturnFormattedString()
    {
        // Act
        var result = FileUploadValidator.GetAllowedFileTypes();

        // Assert
        Assert.Contains("Documents:", result);
        Assert.Contains("Images:", result);
        Assert.Contains("Videos:", result);
        Assert.Contains("pdf", result);
        Assert.Contains("jpg", result);
        Assert.Contains("mp4", result);
    }

    #endregion

    #region Max Attachments Tests

    [Fact]
    public void MaxAttachmentsPerMessage_ShouldBeSix()
    {
        // Assert
        Assert.Equal(6, FileUploadValidator.MaxAttachmentsPerMessage);
    }

    #endregion
}
