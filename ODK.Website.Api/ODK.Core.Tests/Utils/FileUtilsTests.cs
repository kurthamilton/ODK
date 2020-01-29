using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using NUnit.Framework;
using ODK.Core.Utils;

namespace ODK.Core.Tests.Utils
{
    public static class FileUtilsTests
    {
        [TestCase(".a.2.c", ".jpg")]
        [TestCase(".a.2.c....", ".png")]
        [TestCase("###\\/\\/.a.2.c....", ".jpeg")]
        [TestCase("###   /.a.2.c", ".gif")]
        public static void AlhpaNumericImageFileName_ReturnsAlphaNumericFileName(string fileName, string extension)
        {
            fileName = FileUtils.AlphaNumericImageFileName(fileName + extension);
            Assert.AreEqual("a2c" + extension, fileName);
        }
    }
}
